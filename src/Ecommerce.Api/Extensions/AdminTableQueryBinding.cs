namespace Ecommerce.Api.Extensions;

public static class AdminTableQueryBinding
{
    public static Guid[]? ParseGuids(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? null
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Guid.Parse)
                .ToArray();

    public static string[]? ParseStrings(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? null
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
