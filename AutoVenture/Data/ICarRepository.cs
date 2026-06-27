using System;
using System.Threading;
using System.Threading.Tasks;
using AutoVenture.Models;

namespace AutoVenture.Data
{
    public interface ICarRepository : IRepository<Car>
    {
        /// <summary>True if no existing rental overlaps [start, end] for this car.</summary>
        Task<bool> IsAvailableAsync(int carId, DateTime start, DateTime end, CancellationToken ct = default);

        /// <summary>
        /// Loads a car taking a pessimistic UPDLOCK/HOLDLOCK so a concurrent
        /// booking transaction blocks until this one commits — the primary
        /// guard against double-booking the same chassis.
        /// </summary>
        Task<Car?> GetForUpdateAsync(int carId, CancellationToken ct = default);
    }
}
