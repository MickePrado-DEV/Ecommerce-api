using FluentResults;

namespace Ecommerce.Application.Abstractions;

public interface ICoverImageStorage
{
    Task<Result<string>> SaveAsync(Stream content, string originalFileName, long length, CancellationToken ct = default);
    void TryDeleteByUrl(string? imageUrl);
}
