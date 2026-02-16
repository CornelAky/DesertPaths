using System.Diagnostics;
using DesertPaths.Data;
using DesertPaths.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured journeys
            var featuredJourneys = await _context.Journeys
                .Include(j => j.Land)
                .Include(j => j.Reviews)
                .Where(j => j.IsActive && j.IsFeatured)
                .Take(3)
                .ToListAsync();

            // Get all lands
            var lands = await _context.Lands
                .Where(l => l.IsActive)
                .ToListAsync();

            // Get recent approved reviews
            var recentReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Journey)
                    .ThenInclude(j => j.Land)
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedJourneys = featuredJourneys;
            ViewBag.Lands = lands;
            ViewBag.Reviews = recentReviews;

            return View();
        }

        public IActionResult OurStory()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
