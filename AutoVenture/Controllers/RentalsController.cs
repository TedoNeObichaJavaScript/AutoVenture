using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Models;

namespace AutoVenture.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RentalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rentals = _context.Rentals.Include(r => r.Car);
            return View(await rentals.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rental = await _context.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null) return NotFound();

            return View(rental);
        }

        public IActionResult Create()
        {
            ViewData["CarId"] = new SelectList(_context.Cars, "CarId", "Make");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RentalId,CarId,CustomerName,StartDate,EndDate")] Rental rental)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CarId"] = new SelectList(_context.Cars, "CarId", "Make", rental.CarId);
            return View(rental);
        }

        [HttpPost("Rentals/CreateJson")]
        public async Task<IActionResult> CreateJson([FromBody] RentalJsonModel model)
        {
            if (model == null) return BadRequest("Invalid data.");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rental = new Rental
            {
                CarId = model.CarId,
                CustomerName = $"{model.FirstName} {model.MiddleName} {model.LastName}".Trim(),
                StartDate = model.RentalDate,
                EndDate = null
            };

            try
            {
                _context.Rentals.Add(rental);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            ViewData["CarId"] = new SelectList(_context.Cars, "CarId", "Make", rental.CarId);
            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RentalId,CarId,CustomerName,StartDate,EndDate")] Rental rental)
        {
            if (id != rental.RentalId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rental);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.RentalId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CarId"] = new SelectList(_context.Cars, "CarId", "Make", rental.CarId);
            return View(rental);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var rental = await _context.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null) return NotFound();

            return View(rental);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rentals.Any(e => e.RentalId == id);
        }
    }

    public class RentalJsonModel
    {
        public int CarId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime RentalDate { get; set; }
        public string CreditCard { get; set; }
        public string ExpirationDate { get; set; }
        public string CCV { get; set; }
        public string EGN { get; set; }
        public string PickupSite { get; set; }
    }
}
