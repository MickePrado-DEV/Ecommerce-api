namespace Ecommerce.Application.DTOs.Admin;

public record ProductOptionDto(
    Guid Id,
    string Name,
    int OptionType,
    int SortOrder,
    IReadOnlyList<OptionValueDto> Values);

public record OptionValueDto(Guid Id, string Value, string? Description, int SortOrder);

public record SaveProductOptionRequest(string Name, int OptionType, int SortOrder);

public record SaveOptionValueRequest(string Value, string? Description, int SortOrder);

public record ProductOptionAssignmentDto(
    Guid OptionId,
    string Name,
    int OptionType,
    IReadOnlyList<OptionValueDto> SelectedValues);

public record AttachProductOptionRequest(Guid OptionId, IReadOnlyList<Guid> ValueIds);

public record GenerateVariantsResultDto(int VariantsCreated, int ExpectedCount);
