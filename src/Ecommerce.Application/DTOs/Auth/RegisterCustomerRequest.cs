namespace Ecommerce.Application.DTOs.Auth;

public record RegisterCustomerRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone = null);
