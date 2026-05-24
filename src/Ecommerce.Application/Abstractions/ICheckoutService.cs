using Ecommerce.Application.DTOs.Checkout;

namespace Ecommerce.Application.Abstractions;

public interface ICheckoutService
{
    Task<CheckoutResultDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken ct = default);
}
