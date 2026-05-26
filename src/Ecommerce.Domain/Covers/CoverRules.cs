using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Covers;

public static class CoverRules
{
    public const int MaxPrincipalActive = 5;

    public static bool IsEffectivelyActive(Cover cover, DateTime utcNow) =>
        cover.IsActive
        && (!cover.EndsAt.HasValue || cover.EndsAt.Value >= utcNow)
        && (!cover.StartsAt.HasValue || cover.StartsAt.Value <= utcNow);

    public static bool IsInPrincipalSlot(Cover cover, DateTime utcNow) =>
        IsEffectivelyActive(cover, utcNow) && cover.SortOrder is >= 1 and <= MaxPrincipalActive;
}
