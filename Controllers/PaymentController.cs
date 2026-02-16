using DesertPaths.Data;
using DesertPaths.Models.Entities;
using DesertPaths.Models.Enums;
using DesertPaths.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPaymentService paymentService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _userManager = userManager;
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: /Payment/Checkout/{bookingId}
    public async Task<IActionResult> Checkout(int bookingId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Journey)
                .ThenInclude(j => j.Land)
            .Include(b => b.Style)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id);

        if (booking == null)
            return NotFound();

        // Only allow payment for pending bookings
        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "This booking cannot be paid for.";
            return RedirectToAction("MyBookings", "Booking");
        }

        ViewBag.PaymentProvider = _paymentService.ProviderName;
        return View(booking);
    }

    // POST: /Payment/ProcessPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(int bookingId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Journey)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id);

        if (booking == null)
            return NotFound();

        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "This booking cannot be paid for.";
            return RedirectToAction("MyBookings", "Booking");
        }

        // Build callback URLs
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var callbackUrl = $"{baseUrl}/Payment/Callback";
        var returnUrl = $"{baseUrl}/Payment/Return?bookingId={bookingId}";

        // Get currency from config
        var currency = _configuration["PayTabs:Currency"] ?? "USD";

        // Create payment request using the interface
        var paymentRequest = new PaymentRequest
        {
            OrderId = booking.BookingReference,
            Description = $"Desert Paths Journey: {booking.Journey.Title}",
            Amount = booking.TotalPrice,
            Currency = currency,
            CustomerName = user.FullName,
            CustomerEmail = booking.ContactEmail ?? user.Email ?? "",
            CustomerPhone = booking.ContactPhone ?? user.PhoneNumber,
            CallbackUrl = callbackUrl,
            ReturnUrl = returnUrl
        };

        _logger.LogInformation("Creating payment via {Provider} for booking {BookingRef}", 
            _paymentService.ProviderName, booking.BookingReference);

        var result = await _paymentService.CreatePaymentAsync(paymentRequest);

        if (result.Success && result.RedirectUrl != null)
        {
            // Store transaction reference for later verification
            var payment = new Payment
            {
                BookingId = booking.Id,
                TransactionReference = result.TransactionReference ?? "",
                Amount = booking.TotalPrice,
                Currency = currency,
                Status = PaymentStatus.Pending,
                PaymentMethod = _paymentService.ProviderName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Redirecting to payment page: {Url}", result.RedirectUrl);
            return Redirect(result.RedirectUrl);
        }

        TempData["Error"] = result.ErrorMessage ?? "Unable to initiate payment. Please try again.";
        return RedirectToAction(nameof(Checkout), new { bookingId });
    }

    // GET: /Payment/MockCheckout - Mock payment page for development
    [AllowAnonymous]
    public IActionResult MockCheckout(string transRef)
    {
        var paymentData = MockPaymentService.GetPaymentData(transRef);
        if (paymentData == null)
            return NotFound("Payment not found");

        ViewBag.TransactionReference = transRef;
        ViewBag.Amount = paymentData.Amount;
        ViewBag.Currency = paymentData.Currency;
        ViewBag.OrderId = paymentData.OrderId;
        ViewBag.Email = paymentData.CustomerEmail;
        ViewBag.ReturnUrl = paymentData.ReturnUrl;

        return View();
    }

    // POST: /Payment/MockComplete - Complete mock payment
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> MockComplete(string transRef, bool success, string returnUrl)
    {
        _logger.LogInformation("🧪 MOCK: Completing payment {TransRef}, Success: {Success}", transRef, success);

        MockPaymentService.CompletePayment(transRef, success);

        // Find and update the payment
        var payment = await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.TransactionReference == transRef);

        if (payment != null)
        {
            payment.UpdatedAt = DateTime.UtcNow;

            if (success)
            {
                payment.Status = PaymentStatus.Completed;
                payment.ResponseCode = "000";
                payment.ResponseMessage = "Mock Payment Successful";
                payment.Booking.Status = BookingStatus.Confirmed;
                payment.Booking.IsPaid = true;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ResponseCode = "E01";
                payment.ResponseMessage = "Mock Payment Declined";
            }

            await _context.SaveChangesAsync();
        }

        return Redirect(returnUrl);
    }

    // POST: /Payment/Callback - Server-to-server callback (for real providers)
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Callback()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogInformation("Payment Callback received: {Body}", body);

            var callback = System.Text.Json.JsonSerializer.Deserialize<PayTabsCallback>(body, 
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (callback == null)
            {
                _logger.LogWarning("Invalid callback payload");
                return BadRequest();
            }

            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.TransactionReference == callback.TransactionReference);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for transaction: {TransRef}", callback.TransactionReference);
                return NotFound();
            }

            payment.ResponseCode = callback.ResponseCode;
            payment.ResponseMessage = callback.ResponseMessage;
            payment.UpdatedAt = DateTime.UtcNow;

            if (callback.IsSuccess)
            {
                payment.Status = PaymentStatus.Completed;
                payment.Booking.Status = BookingStatus.Confirmed;
                payment.Booking.IsPaid = true;
                _logger.LogInformation("Payment successful for booking: {BookingRef}", payment.Booking.BookingReference);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                _logger.LogWarning("Payment failed: {Message}", callback.ResponseMessage);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment callback");
            return StatusCode(500);
        }
    }

    // GET: /Payment/Return - User returns from payment page
    [AllowAnonymous]
    public async Task<IActionResult> Return(int bookingId, string? tranRef)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return NotFound();

        var payment = booking.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        if (payment == null)
            return RedirectToAction("Confirmation", "Booking", new { id = bookingId });

        // If still pending, query the payment provider
        if (payment.Status == PaymentStatus.Pending && !string.IsNullOrEmpty(payment.TransactionReference))
        {
            var queryResult = await _paymentService.QueryPaymentAsync(payment.TransactionReference);

            if (queryResult.Success)
            {
                payment.ResponseCode = queryResult.ResponseCode;
                payment.ResponseMessage = queryResult.ResponseMessage;
                payment.UpdatedAt = DateTime.UtcNow;

                if (queryResult.Status == PaymentTransactionStatus.Authorized)
                {
                    payment.Status = PaymentStatus.Completed;
                    booking.Status = BookingStatus.Confirmed;
                    booking.IsPaid = true;
                }
                else if (queryResult.Status == PaymentTransactionStatus.Declined || 
                         queryResult.Status == PaymentTransactionStatus.Cancelled)
                {
                    payment.Status = PaymentStatus.Failed;
                }

                await _context.SaveChangesAsync();
            }
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            TempData["Success"] = "Payment successful! Your booking is confirmed.";
            return RedirectToAction("Confirmation", "Booking", new { id = bookingId });
        }
        else
        {
            TempData["Error"] = $"Payment was not successful: {payment.ResponseMessage ?? "Unknown error"}";
            return RedirectToAction(nameof(Failed), new { bookingId });
        }
    }

    // GET: /Payment/Failed
    public async Task<IActionResult> Failed(int bookingId)
    {
        var user = await _userManager.GetUserAsync(User);

        var booking = await _context.Bookings
            .Include(b => b.Journey)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == bookingId && (user == null || b.UserId == user.Id));

        if (booking == null)
            return NotFound();

        var payment = booking.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        ViewBag.ErrorMessage = payment?.ResponseMessage ?? "Payment was declined";

        return View(booking);
    }
}
