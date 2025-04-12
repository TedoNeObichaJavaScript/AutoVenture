using System.Diagnostics;
using AutoVenture.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Models;

namespace AutoVenture.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            try
            {
                var cars = await _context.Cars.ToListAsync();
                return View(cars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve cars from the database.");
                return RedirectToAction("Error");
            }
        }

        // GET: /Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Handle form logic here (e.g., send email, save to DB)
                ViewBag.Message = "Съобщението беше изпратено успешно!";
                ModelState.Clear(); // Reset form fields
                return View();
            }

            return View(model); // Return with validation errors
        }

        // GET: /Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
