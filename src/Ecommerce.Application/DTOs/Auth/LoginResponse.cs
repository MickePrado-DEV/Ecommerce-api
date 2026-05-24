namespace Ecommerce.Application.DTOs.Auth;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User,
    IReadOnlyList<string> Permissions);