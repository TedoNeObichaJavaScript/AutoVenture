using System.Threading;
using System.Threading.Tasks;
using AutoVenture.Data;
using AutoVenture.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoVenture.Services
{
    public class RentalService : IRentalService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<RentalService> _logger;

        public RentalService(IUnitOfWork uow, ILogger<RentalService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<BookingResult> BookAsync(BookingRequest request, CancellationToken ct = default)
        {
            // Serializable isolation + a pessimistic UPDLOCK on the car row make
            // the "is it free? -> book it" sequence a single atomic step. A second
            // concurrent request for the same chassis blocks on GetForUpdateAsync
            // until this transaction commits, then sees the overlap and is rejected.
            await using var tx = await _uow.BeginSerializableTransactionAsync(ct);
            try
            {
                var car = await _uow.Cars.GetForUpdateAsync(request.CarId, ct);
                if (car is null)
                    return new BookingResult(BookingOutcome.CarNotFound);

                if (!await _uow.Cars.IsAvailableAsync(request.CarId, request.StartDate, request.EndDate, ct))
                    return new BookingResult(BookingOutcome.NotAvailable);

                var rental = new Rental
                {
                    CarId = car.CarId,
                    CustomerName = request.FullName,
                    StartDate = request.StartDate.Date,
                    EndDate = request.EndDate.Date
                };

                await _uow.Rentals.AddAsync(rental, ct);
                await _uow.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var total = rental.RentalDays * car.DailyRentPrice;
                return BookingResult.Success(rental.RentalId, total);
            }
            catch (DbUpdateException ex)
            {
                // Lost an optimistic/serialization race — surface as a conflict.
                await tx.RollbackAsync(ct);
                _logger.LogWarning(ex, "Booking conflict for car {CarId}", request.CarId);
                return new BookingResult(BookingOutcome.Conflict);
            }
        }
    }
}
