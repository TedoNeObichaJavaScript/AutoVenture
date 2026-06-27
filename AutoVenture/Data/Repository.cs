using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoVenture.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoVenture.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly DbSet<T> Set;

        public Repository(ApplicationDbContext context)
        {
            Context = context;
            Set = context.Set<T>();
        }

        public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
            Set.FindAsync(new object[] { id }, ct).AsTask();

        public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default) =>
            await Set.AsNoTracking().ToListAsync(ct);

        public virtual async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
            await Set.AsNoTracking().Where(predicate).ToListAsync(ct);

        public IQueryable<T> Query() => Set.AsQueryable();

        public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
            await Set.AddAsync(entity, ct);

        public virtual void Remove(T entity) => Set.Remove(entity);

        public virtual void Update(T entity) => Set.Update(entity);
    }
}
