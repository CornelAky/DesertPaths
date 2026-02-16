using DesertPaths.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.ViewComponents;

public class NavigationLandsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NavigationLandsViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var lands = await _context.Lands
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new { l.Name, l.Slug })
            .ToListAsync();

        return View(lands);
    }
}
