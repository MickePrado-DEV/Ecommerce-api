namespace Ecommerce.Application.Abstractions.Persistence;

public interface IDashboardRepository
{
    Task<(int Orders, int PendingPayment, int Paid, int ReadyToDispatch, int Products, int Users)> GetStatsAsync(CancellationToken ct = default);
}
