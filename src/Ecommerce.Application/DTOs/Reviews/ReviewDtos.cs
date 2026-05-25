namespace Ecommerce.Application.DTOs.Reviews;

public record ProductReviewDto(
    Guid Id,
    string AuthorName,
    int Rating,
    string? Title,
    string Comment,
    DateTime CreatedAt);

public record ProductReviewSummaryDto(double AverageRating, int TotalCount);

public record ProductReviewsPageDto(
    ProductReviewSummaryDto Summary,
    IReadOnlyList<ProductReviewDto> Items);

public record CreateProductReviewRequest(int Rating, string? Title, string Comment);
