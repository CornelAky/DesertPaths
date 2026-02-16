using DesertPaths.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Controllers;

public class JourneysController : Controller
{
    private readonly ApplicationDbContext _context;

    public JourneysController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? land = null)
    {
        var query = _context.Journeys
            .Include(j => j.Land)
            .Include(j => j.DefaultStyle)
            .Include(j => j.Reviews)
            .Where(j => j.IsActive);

        if (!string.IsNullOrEmpty(land))
        {
            query = query.Where(j => j.Land.Slug == land);
        }

        var journeys = await query
            .OrderByDescending(j => j.IsFeatured)
            .ThenBy(j => j.Title)
            .ToListAsync();

        ViewBag.SelectedLand = land;
        ViewBag.Lands = await _context.Lands.Where(l => l.IsActive).ToListAsync();

        return View(journeys);
    }

    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var journey = await _context.Journeys
            .Include(j => j.Land)
            .Include(j => j.DefaultStyle)
            .Include(j => j.Itineraries.OrderBy(i => i.DayNumber))
            .Include(j => j.Images.OrderBy(i => i.DisplayOrder))
            .Include(j => j.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(j => j.Slug == slug && j.IsActive);

        if (journey == null)
            return NotFound();

        ViewBag.Styles = await _context.JourneyStyles.Where(s => s.IsActive).ToListAsync();
        
        return View(journey);
    }
}
