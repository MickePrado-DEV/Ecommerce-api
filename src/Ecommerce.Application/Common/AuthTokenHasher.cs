// Hash SHA-256 del refresh token antes de guardarlo en BD (nunca se almacena el token en claro).
namespace Ecommerce.Application.Common;

public static class AuthTokenHasher
{
    public static string Hash(string token) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token)));
}
