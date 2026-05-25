namespace Ecommerce.Application.Common;

public static class AuthTokenHasher
{
    public static string Hash(string token) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token)));
}
