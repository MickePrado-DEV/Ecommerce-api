using Ecommerce.Application.Abstractions;
using FluentResults;

namespace Ecommerce.Infrastructure.Storage;

/// <param name="uploadsRoot">Carpeta raíz de subidas (contiene la subcarpeta <c>covers</c>).</param>
public class CoverImageStorage(string uploadsRoot) : ICoverImageStorage
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private readonly string _coversDir = Path.Combine(uploadsRoot, "covers");

    public async Task<Result<string>> SaveAsync(
        Stream content,
        string originalFileName,
        long length,
        CancellationToken ct = default)
    {
        if (length == 0)
            return Result.Fail<string>("El archivo está vacío.");

        if (length > MaxBytes)
            return Result.Fail<string>("La imagen no puede superar 5 MB.");

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return Result.Fail<string>("Formato no permitido. Usa JPG, PNG o WebP.");

        Directory.CreateDirectory(_coversDir);
        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(_coversDir, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await content.CopyToAsync(stream, ct);
        }

        return Result.Ok($"/uploads/covers/{fileName}");
    }

    public void TryDeleteByUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;
        if (!imageUrl.StartsWith("/uploads/covers/", StringComparison.OrdinalIgnoreCase)) return;

        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrEmpty(fileName)) return;

        var fullPath = Path.Combine(_coversDir, fileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
