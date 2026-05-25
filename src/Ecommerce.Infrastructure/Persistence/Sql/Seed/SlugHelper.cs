using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Seed;

internal static partial class SlugHelper
{
    public static string ToSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "item";

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = NonAlphaNumeric().Replace(sb.ToString().ToLowerInvariant(), "-");
        slug = MultiDash().Replace(slug, "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    public static string UniqueSlug(string baseSlug, HashSet<string> used)
    {
        var slug = baseSlug;
        var i = 2;
        while (!used.Add(slug))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumeric();

    [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
    private static partial Regex MultiDash();
}
