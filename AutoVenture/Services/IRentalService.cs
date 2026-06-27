using System.Threading;
using System.Threading.Tasks;

namespace AutoVenture.Services
{
    public interface IRentalService
    {
        /// <summary>
        /// Atomically books a car for the requested window. Caller is expected
        /// to have already passed structural validation; this method enforces
        /// the business invariant that a chassis is never double-booked.
        /// </summary>
        Task<BookingResult> BookAsync(BookingRequest request, CancellationToken ct = default);
    }
}
