namespace Ecommerce.Application.DTOs.Admin;

public record ProductOptionDto(Guid Id, Guid ProductId, string Name, int SortOrder, IReadOnlyList<OptionValueDto> Values);

public record OptionValueDto(Guid Id, string Value, int SortOrder);

public record SaveProductOptionRequest(string Name, int SortOrder);

public record SaveOptionValueRequest(string Value, int SortOrder);
