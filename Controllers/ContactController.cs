using Microsoft.AspNetCore.Mvc;

namespace DesertPaths.Controllers;

public class ContactController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(string name, string email, string subject, string message)
    {
        // TODO: Implement email sending
        TempData["Success"] = "Thank you for your message! We'll get back to you soon.";
        return RedirectToAction(nameof(Index));
    }
}
