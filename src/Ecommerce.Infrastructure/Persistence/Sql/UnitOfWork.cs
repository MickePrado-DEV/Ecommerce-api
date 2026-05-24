using Ecommerce.Application.Abstractions.Persistence;

namespace Ecommerce.Infrastructure.Persistence.Sql
{
    public class UnitOfWork(EcommerceDbContext db) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

        public Task BeginTransactionAsync(CancellationToken ct = default)
            => db.Database.BeginTransactionAsync(ct);

        public async Task CommitAsync(CancellationToken ct = default)
        {
            await db.SaveChangesAsync(ct);
            await db.Database.CommitTransactionAsync(ct);
        }

        public Task RollbackAsync(CancellationToken ct = default)
            => db.Database.RollbackTransactionAsync(ct);
    }
}
