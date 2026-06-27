using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoVenture.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoVenture.Data
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        // Compiled query: the availability/overlap check runs on every booking,
        // so caching the translated SQL avoids re-compiling the expression tree.
        // Overlap rule: an existing rental conflicts when it starts on/before the
        // requested end AND ends on/after the requested start (open-ended rentals
        // with a null EndDate are treated as still active).
        private static readonly Func<ApplicationDbContext, int, DateTime, DateTime, Task<bool>> _hasOverlap =
            EF.CompileAsyncQuery((ApplicationDbContext ctx, int carId, DateTime start, DateTime end) =>
                ctx.Rentals.Any(r =>
                    r.CarId == carId &&
                    r.StartDate <= end &&
                    (r.EndDate == null || r.EndDate >= start)));

        public CarRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> IsAvailableAsync(int carId, DateTime start, DateTime end, CancellationToken ct = default) =>
            !await _hasOverlap(Context, carId, start, end);

        public Task<Car?> GetForUpdateAsync(int carId, CancellationToken ct = default) =>
            Context.Cars
                .FromSqlInterpolated($"SELECT * FROM Cars WITH (UPDLOCK, HOLDLOCK) WHERE CarId = {carId}")
                .FirstOrDefaultAsync(ct);
    }
}
