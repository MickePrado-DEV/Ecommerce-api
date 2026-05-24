namespace Ecommerce.Application.DTOs.Admin;

public record DashboardStatsDto(
    int TotalOrders,
    int PendingPaymentOrders,
    int PaidOrders,
    int ReadyToDispatchOrders,
    int TotalProducts,
    int TotalUsers);
