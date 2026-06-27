using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoVenture.Models;
using AutoVenture.Services;
using FluentValidation;

namespace AutoVenture.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRentalService _rentalService;
        private readonly IValidator<BookingRequest> _bookingValidator;

        public RentalsController(
            ApplicationDbContext context,
            IRentalService rentalService,
            IValidator<BookingRequest> bookingValidator)
        {
            _context = context;
            _rentalService = rentalService;
            _bookingValidator = bookingValidator;
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

        // Real booking endpoint consumed by the catalog/home rental forms.
        [HttpPost("Rentals/Book")]
        [Produces("application/json")]
        public async Task<IActionResult> Book([FromBody] BookingRequest request, CancellationToken ct)
        {
            if (request is null) return BadRequest(new { message = "Invalid request body." });

            var validation = await _bookingValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                });
            }

            var result = await _rentalService.BookAsync(request, ct);
            return result.Outcome switch
            {
                BookingOutcome.Success => Ok(new
                {
                    rentalId = result.RentalId,
                    total = result.Total,
                    message = "Booking confirmed."
                }),
                BookingOutcome.CarNotFound => NotFound(new { message = "Car not found." }),
                BookingOutcome.NotAvailable => Conflict(new { message = "Car is already booked for those dates." }),
                _ => Conflict(new { message = "Booking could not be completed, please retry." })
            };
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
}
