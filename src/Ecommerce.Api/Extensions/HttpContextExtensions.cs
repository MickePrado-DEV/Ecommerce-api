using System.Security.Claims;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext ctx)
    {
        var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static Guid? GetGuestToken(this HttpContext ctx)
    {
        var header = ctx.Request.Headers["X-Guest-Token"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : null;
    }
}