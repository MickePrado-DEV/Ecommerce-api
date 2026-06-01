namespace Ecommerce.Application.DTOs.Admin;

public record ListPaginationQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null,
    string SortDirection = "asc",
    string? Status = null);
