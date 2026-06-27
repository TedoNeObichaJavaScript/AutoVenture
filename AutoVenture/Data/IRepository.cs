using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVenture.Data
{
    /// <summary>
    /// Minimal generic repository abstraction over an aggregate root.
    /// Read methods are async; Add/Remove stage changes that the owning
    /// Unit of Work commits in a single SaveChanges.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListAsync(Expression<System.Func<T, bool>> predicate, CancellationToken ct = default);
        IQueryable<T> Query();
        Task AddAsync(T entity, CancellationToken ct = default);
        void Remove(T entity);
        void Update(T entity);
    }
}
