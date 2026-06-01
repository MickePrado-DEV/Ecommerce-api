namespace Ecommerce.Application.DTOs.Auth;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    Guid? DriverId = null,
    string? Phone = null,
    bool MustChangePassword = false);