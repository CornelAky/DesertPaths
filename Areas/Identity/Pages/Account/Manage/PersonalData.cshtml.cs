using System.Text.Json;
using DesertPaths.Data;
using DesertPaths.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Areas.Identity.Pages.Account.Manage;

public class PersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PersonalDataModel> _logger;

    public PersonalDataModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<PersonalDataModel> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadPersonalDataAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

        // Get user's bookings
        var bookings = await _context.Bookings
            .Include(b => b.Journey)
            .Include(b => b.Style)
            .Where(b => b.UserId == user.Id)
            .Select(b => new
            {
                b.BookingReference,
                Journey = b.Journey.Title,
                Style = b.Style.Name,
                b.TravelDate,
                b.NumberOfGuests,
                b.TotalPrice,
                Status = b.Status.ToString(),
                b.CreatedAt
            })
            .ToListAsync();

        // Get user's reviews
        var reviews = await _context.Reviews
            .Include(r => r.Journey)
            .Where(r => r.UserId == user.Id)
            .Select(r => new
            {
                Journey = r.Journey.Title,
                r.Rating,
                r.Comment,
                r.CreatedAt
            })
            .ToListAsync();

        // Compile personal data
        var personalData = new
        {
            Profile = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed
            },
            Bookings = bookings,
            Reviews = reviews
        };

        Response.Headers.Append("Content-Disposition", "attachment; filename=PersonalData.json");
        return new FileContentResult(
            JsonSerializer.SerializeToUtf8Bytes(personalData, new JsonSerializerOptions { WriteIndented = true }), 
            "application/json");
    }
}
