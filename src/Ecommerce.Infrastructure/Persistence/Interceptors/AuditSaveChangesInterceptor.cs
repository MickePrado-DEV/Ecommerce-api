using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ecommerce.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Placeholder para auditoría automática cuando exista tabla audit_logs.
/// Registrar en AddDbContext: options.AddInterceptors(sp.GetRequiredService&lt;AuditSaveChangesInterceptor&gt;());
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // TODO: capturar Added/Modified/Deleted y escribir en audit_logs
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
