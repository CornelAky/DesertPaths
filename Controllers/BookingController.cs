using DesertPaths.Data;
using DesertPaths.Models.Entities;
using DesertPaths.Models.Enums;
using DesertPaths.Services;
using DesertPaths.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingService _bookingService;

    public BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBookingService bookingService)
    {
        _context = context;
        _userManager = userManager;
        _bookingService = bookingService;
    }

    // GET: /Booking/Create/{slug}
    public async Task<IActionResult> Create(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var journey = await _context.Journeys
            .Include(j => j.Land)
            .Include(j => j.DefaultStyle)
            .FirstOrDefaultAsync(j => j.Slug == slug && j.IsActive);

        if (journey == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        var styles = await _context.JourneyStyles
            .Where(s => s.IsActive)
            .ToListAsync();

        var viewModel = new CreateBookingViewModel
        {
            JourneyId = journey.Id,
            JourneyTitle = journey.Title,
            JourneySlug = journey.Slug,
            LandName = journey.Land.Name,
            DurationDays = journey.DurationDays,
            DurationNights = journey.DurationNights,
            BasePrice = journey.PriceFrom,
            HeroImageUrl = journey.HeroImageUrl,
            MaxGroupSize = journey.MaxGroupSize,
            StyleId = journey.DefaultStyleId,
            ContactEmail = user?.Email ?? "",
            ContactPhone = user?.PhoneNumber ?? "",
            AvailableStyles = styles.Select(s => new StyleOption
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                PriceMultiplier = s.PriceMultiplier,
                CalculatedPrice = journey.PriceFrom * s.PriceMultiplier
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: /Booking/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        var journey = await _context.Journeys
            .Include(j => j.Land)
            .FirstOrDefaultAsync(j => j.Id == model.JourneyId && j.IsActive);

        if (journey == null)
            return NotFound();

        var style = await _context.JourneyStyles.FindAsync(model.StyleId);
        if (style == null)
        {
            ModelState.AddModelError("StyleId", "Invalid style selected");
        }

        // Validate travel date (must be at least 7 days in the future)
        if (model.TravelDate < DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7).ToDateTime(TimeOnly.MinValue))
        {
            ModelState.AddModelError("TravelDate", "Travel date must be at least 7 days from today");
        }

        // Validate number of guests
        if (model.NumberOfGuests > journey.MaxGroupSize)
        {
            ModelState.AddModelError("NumberOfGuests", $"Maximum group size is {journey.MaxGroupSize}");
        }

        if (!ModelState.IsValid)
        {
            // Reload styles for the view
            var styles = await _context.JourneyStyles.Where(s => s.IsActive).ToListAsync();
            model.JourneyTitle = journey.Title;
            model.JourneySlug = journey.Slug;
            model.LandName = journey.Land.Name;
            model.DurationDays = journey.DurationDays;
            model.DurationNights = journey.DurationNights;
            model.BasePrice = journey.PriceFrom;
            model.HeroImageUrl = journey.HeroImageUrl;
            model.MaxGroupSize = journey.MaxGroupSize;
            model.AvailableStyles = styles.Select(s => new StyleOption
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                PriceMultiplier = s.PriceMultiplier,
                CalculatedPrice = journey.PriceFrom * s.PriceMultiplier
            }).ToList();

            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Check if user is blocked
        if (user.IsBlocked)
        {
            TempData["Error"] = "Your account has been blocked. Please contact support.";
            return RedirectToAction("Index", "Home");
        }

        var totalPrice = _bookingService.CalculateTotalPrice(
            journey.PriceFrom,
            style!.PriceMultiplier,
            model.NumberOfGuests);

        var booking = new Booking
        {
            BookingReference = _bookingService.GenerateBookingReference(),
            JourneyId = journey.Id,
            UserId = user.Id,
            StyleId = model.StyleId,
            TravelDate = model.TravelDate,
            NumberOfGuests = model.NumberOfGuests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            PaymentMethod = model.PaymentMethod,
            IsPaid = false,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            SpecialRequests = model.SpecialRequests,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // If payment method is Online, redirect to payment page
        // If payment method is OnArrival, show confirmation directly
        if (model.PaymentMethod == Models.Enums.PaymentMethod.Online)
        {
            return RedirectToAction("Checkout", "Payment", new { bookingId = booking.Id });
        }
        else
        {
            // Pay on Arrival - show confirmation, wait for admin to confirm
            return RedirectToAction(nameof(Confirmation), new { id = booking.Id });
        }
    }

    // GET: /Booking/Confirmation/{id}
    public async Task<IActionResult> Confirmation(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Journey)
                .ThenInclude(j => j.Land)
            .Include(b => b.Style)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

        if (booking == null)
            return NotFound();

        var viewModel = new BookingConfirmationViewModel
        {
            BookingId = booking.Id,
            BookingReference = booking.BookingReference,
            JourneyTitle = booking.Journey.Title,
            LandName = booking.Journey.Land.Name,
            StyleName = booking.Style.Name,
            TravelDate = booking.TravelDate,
            NumberOfGuests = booking.NumberOfGuests,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CustomerName = user.FullName,
            CustomerEmail = booking.ContactEmail ?? user.Email ?? "",
            ContactPhone = booking.ContactPhone,
            PaymentMethod = booking.PaymentMethod.ToString(),
            IsPaid = booking.IsPaid
        };

        return View(viewModel);
    }

    // GET: /Booking/MyBookings
    public async Task<IActionResult> MyBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var bookings = await _context.Bookings
            .Include(b => b.Journey)
                .ThenInclude(j => j.Land)
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        // Get user's reviewed journey IDs
        var reviewedJourneyIds = await _context.Reviews
            .Where(r => r.UserId == user.Id)
            .Select(r => r.JourneyId)
            .ToListAsync();

        var viewModel = new MyBookingsViewModel
        {
            Bookings = bookings.Select(b => new BookingListItem
            {
                Id = b.Id,
                BookingReference = b.BookingReference,
                JourneyTitle = b.Journey.Title,
                JourneyImageUrl = b.Journey.HeroImageUrl,
                LandName = b.Journey.Land.Name,
                JourneyId = b.JourneyId,
                TravelDate = b.TravelDate,
                NumberOfGuests = b.NumberOfGuests,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                PaymentMethod = b.PaymentMethod.ToString(),
                IsPaid = b.IsPaid,
                CreatedAt = b.CreatedAt,
                CanCancel = b.Status == BookingStatus.Pending && b.TravelDate > DateTime.Today.AddDays(14),
                CanReview = b.Status == BookingStatus.Completed && !reviewedJourneyIds.Contains(b.JourneyId),
                HasReviewed = reviewedJourneyIds.Contains(b.JourneyId),
                CanPay = b.Status == BookingStatus.Pending && b.PaymentMethod == Models.Enums.PaymentMethod.Online && !b.IsPaid
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: /Booking/Cancel/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

        if (booking == null)
            return NotFound();

        // Can only cancel pending bookings at least 14 days before travel
        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "Only pending bookings can be cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }

        if (booking.TravelDate <= DateTime.Today.AddDays(14))
        {
            TempData["Error"] = "Bookings can only be cancelled at least 14 days before the travel date.";
            return RedirectToAction(nameof(MyBookings));
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Your booking has been cancelled successfully.";
        return RedirectToAction(nameof(MyBookings));
    }

    // GET: /Booking/Review/{bookingId}
    public async Task<IActionResult> Review(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Journey)
                .ThenInclude(j => j.Land)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

        if (booking == null)
            return NotFound();

        // Can only review completed bookings
        if (booking.Status != BookingStatus.Completed)
        {
            TempData["Error"] = "You can only review completed journeys.";
            return RedirectToAction(nameof(MyBookings));
        }

        // Check if already reviewed
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.JourneyId == booking.JourneyId && r.UserId == user.Id);

        if (existingReview != null)
        {
            TempData["Error"] = "You have already reviewed this journey.";
            return RedirectToAction(nameof(MyBookings));
        }

        var viewModel = new CreateReviewViewModel
        {
            BookingId = booking.Id,
            JourneyId = booking.JourneyId,
            JourneyTitle = booking.Journey.Title,
            JourneyImageUrl = booking.Journey.HeroImageUrl,
            LandName = booking.Journey.Land.Name,
            TravelDate = booking.TravelDate
        };

        return View(viewModel);
    }

    // POST: /Booking/Review
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(CreateReviewViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Journey)
            .FirstOrDefaultAsync(b => b.Id == model.BookingId && b.UserId == user.Id);

        if (booking == null || booking.Status != BookingStatus.Completed)
        {
            return NotFound();
        }

        // Check if already reviewed
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.JourneyId == booking.JourneyId && r.UserId == user.Id);

        if (existingReview != null)
        {
            TempData["Error"] = "You have already reviewed this journey.";
            return RedirectToAction(nameof(MyBookings));
        }

        if (!ModelState.IsValid)
        {
            model.JourneyTitle = booking.Journey.Title;
            model.JourneyImageUrl = booking.Journey.HeroImageUrl;
            return View(model);
        }

        var review = new Review
        {
            JourneyId = booking.JourneyId,
            UserId = user.Id,
            Rating = model.Rating,
            Comment = model.Comment,
            IsApproved = false, // Requires admin approval
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thank you for your review! It will be visible after approval.";
        return RedirectToAction(nameof(MyBookings));
    }
}
