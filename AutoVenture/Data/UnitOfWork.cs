using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AutoVenture.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoVenture.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public ICarRepository Cars { get; }
        public IRepository<Rental> Rentals { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Cars = new CarRepository(context);
            Rentals = new Repository<Rental>(context);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            _context.SaveChangesAsync(ct);

        public Task<IDbContextTransaction> BeginSerializableTransactionAsync(CancellationToken ct = default) =>
            _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        public ValueTask DisposeAsync() => _context.DisposeAsync();
    }
}
