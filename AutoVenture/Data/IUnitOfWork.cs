using System;
using System.Threading;
using System.Threading.Tasks;
using AutoVenture.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoVenture.Data
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        ICarRepository Cars { get; }
        IRepository<Rental> Rentals { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>Begins a serializable transaction used to make the
        /// availability check and the rental insert a single atomic step.</summary>
        Task<IDbContextTransaction> BeginSerializableTransactionAsync(CancellationToken ct = default);
    }
}
