using DesertPaths.Constants;
using DesertPaths.Data;
using DesertPaths.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Controllers;

[Authorize(Policy = "CanManageContent")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalLands = await _context.Lands.CountAsync();
        ViewBag.TotalJourneys = await _context.Journeys.CountAsync();
        ViewBag.TotalBookings = await _context.Bookings.CountAsync();
        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.PendingBookings = await _context.Bookings.CountAsync(b => b.Status == Models.Enums.BookingStatus.Pending);
        ViewBag.RecentBookings = await _context.Bookings
            .Include(b => b.Journey)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View();
    }

    #region Lands Management

    public async Task<IActionResult> Lands()
    {
        var lands = await _context.Lands
            .Include(l => l.Journeys)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
        return View(lands);
    }

    public IActionResult CreateLand()
    {
        return View(new Land());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLand(Land land)
    {
        if (ModelState.IsValid)
        {
            land.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(land.Slug))
            {
                land.Slug = GenerateSlug(land.Name);
            }
            _context.Lands.Add(land);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Land created successfully!";
            return RedirectToAction(nameof(Lands));
        }
        return View(land);
    }

    public async Task<IActionResult> EditLand(int id)
    {
        var land = await _context.Lands.FindAsync(id);
        if (land == null) return NotFound();
        return View(land);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLand(int id, Land land)
    {
        if (id != land.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(land);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Land updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Lands.AnyAsync(l => l.Id == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Lands));
        }
        return View(land);
    }

    [Authorize(Policy = "CanDeleteContent")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLand(int id)
    {
        var land = await _context.Lands.FindAsync(id);
        if (land == null) return NotFound();

        _context.Lands.Remove(land);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Land deleted successfully!";
        return RedirectToAction(nameof(Lands));
    }

    #endregion

    #region Journeys Management

    public async Task<IActionResult> Journeys()
    {
        var journeys = await _context.Journeys
            .Include(j => j.Land)
            .Include(j => j.DefaultStyle)
            .OrderBy(j => j.Land.Name)
            .ThenBy(j => j.Title)
            .ToListAsync();
        return View(journeys);
    }

    public async Task<IActionResult> CreateJourney()
    {
        ViewBag.Lands = await _context.Lands.Where(l => l.IsActive).ToListAsync();
        ViewBag.Styles = await _context.JourneyStyles.Where(s => s.IsActive).ToListAsync();
        return View(new Journey());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJourney(Journey journey)
    {
        if (ModelState.IsValid)
        {
            journey.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(journey.Slug))
            {
                journey.Slug = GenerateSlug(journey.Title);
            }
            _context.Journeys.Add(journey);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Journey created successfully!";
            return RedirectToAction(nameof(Journeys));
        }
        ViewBag.Lands = await _context.Lands.Where(l => l.IsActive).ToListAsync();
        ViewBag.Styles = await _context.JourneyStyles.Where(s => s.IsActive).ToListAsync();
        return View(journey);
    }

    public async Task<IActionResult> EditJourney(int id)
    {
        var journey = await _context.Journeys.FindAsync(id);
        if (journey == null) return NotFound();

        ViewBag.Lands = await _context.Lands.Where(l => l.IsActive).ToListAsync();
        ViewBag.Styles = await _context.JourneyStyles.Where(s => s.IsActive).ToListAsync();
        return View(journey);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJourney(int id, Journey journey)
    {
        if (id != journey.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(journey);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Journey updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Journeys.AnyAsync(j => j.Id == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Journeys));
        }
        ViewBag.Lands = await _context.Lands.Where(l => l.IsActive).ToListAsync();
        ViewBag.Styles = await _context.JourneyStyles.Where(s => s.IsActive).ToListAsync();
        return View(journey);
    }

    [Authorize(Policy = "CanDeleteContent")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJourney(int id)
    {
        var journey = await _context.Journeys.FindAsync(id);
        if (journey == null) return NotFound();

        _context.Journeys.Remove(journey);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Journey deleted successfully!";
        return RedirectToAction(nameof(Journeys));
    }

    #endregion

    #region Bookings Management

    public async Task<IActionResult> Bookings(string? status = null)
    {
        var query = _context.Bookings
            .Include(b => b.Journey)
            .Include(b => b.User)
            .Include(b => b.Style)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Models.Enums.BookingStatus>(status, out var bookingStatus))
        {
            query = query.Where(b => b.Status == bookingStatus);
        }

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        ViewBag.SelectedStatus = status;
        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.Status = Models.Enums.BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Booking confirmed!";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.Status = Models.Enums.BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Booking cancelled.";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.IsPaid = true;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Booking {booking.BookingReference} marked as paid!";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.Status = Models.Enums.BookingStatus.Completed;
        booking.IsPaid = true; // Mark as paid when completing
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Booking {booking.BookingReference} marked as completed!";
        return RedirectToAction(nameof(Bookings));
    }

    #endregion

    #region Users Management

    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    [Authorize(Policy = "CanManageUsers")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToManager(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        await _userManager.RemoveFromRoleAsync(user, AppRoles.Customer);
        await _userManager.AddToRoleAsync(user, AppRoles.Manager);

        TempData["Success"] = $"{user.FullName} promoted to Manager!";
        return RedirectToAction(nameof(Users));
    }

    [Authorize(Policy = "CanManageUsers")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteToCustomer(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        await _userManager.RemoveFromRoleAsync(user, AppRoles.Manager);
        await _userManager.AddToRoleAsync(user, AppRoles.Customer);

        TempData["Success"] = $"{user.FullName} demoted to Customer.";
        return RedirectToAction(nameof(Users));
    }

    [Authorize(Policy = "CanBlockUsers")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlockUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Managers can only block Customers
        var currentUserRoles = await _userManager.GetRolesAsync(user);
        if (!User.IsInRole(AppRoles.Admin) && currentUserRoles.Contains(AppRoles.Manager))
        {
            TempData["Error"] = "You cannot block a Manager.";
            return RedirectToAction(nameof(Users));
        }

        user.IsBlocked = !user.IsBlocked;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = user.IsBlocked 
            ? $"{user.FullName} has been blocked." 
            : $"{user.FullName} has been unblocked.";
        return RedirectToAction(nameof(Users));
    }

    #endregion

    #region Reviews Management

    public async Task<IActionResult> Reviews()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Journey)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        review.IsApproved = true;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Review approved!";
        return RedirectToAction(nameof(Reviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Review deleted.";
        return RedirectToAction(nameof(Reviews));
    }

    #endregion

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
    }
}
