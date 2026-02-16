using DesertPaths.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Controllers;

public class LandsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LandsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lands = await _context.Lands
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
        
        return View(lands);
    }

    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var land = await _context.Lands
            .Include(l => l.Journeys.Where(j => j.IsActive))
            .FirstOrDefaultAsync(l => l.Slug == slug && l.IsActive);

        if (land == null)
            return NotFound();

        return View(land);
    }
}
