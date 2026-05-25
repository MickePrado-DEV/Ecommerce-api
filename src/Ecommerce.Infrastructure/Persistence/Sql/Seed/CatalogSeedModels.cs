namespace Ecommerce.Infrastructure.Persistence.Sql.Seed;

internal sealed record OptionValueTemplate(string Value, string? Description = null);

internal sealed record OptionTemplate(string Name, IReadOnlyList<OptionValueTemplate> Values);

internal sealed record SubcategorySeedContext(
    Guid Id,
    string Name,
    string FamilyName,
    string CategoryName);
