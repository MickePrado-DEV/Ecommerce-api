// Helpers para leer datos del usuario desde JWT o headers de la petición actual.
using System.Security.Claims;

public static class HttpContextExtensions
{
    /// <summary>Id del usuario logueado (claim NameIdentifier del JWT). Null si no hay token.</summary>
    public static Guid? GetUserId(this HttpContext ctx)
    {
        var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>Token de carrito invitado enviado en header X-Guest-Token.</summary>
    public static Guid? GetGuestToken(this HttpContext ctx)
    {
        var header = ctx.Request.Headers["X-Guest-Token"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : null;
    }
}
