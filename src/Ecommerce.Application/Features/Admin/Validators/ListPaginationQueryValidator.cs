using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using FluentValidation;

namespace Ecommerce.Application.Features.Admin.Validators;

public class ListPaginationQueryValidator : AbstractValidator<ListPaginationQuery>
{
    public ListPaginationQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRules.MaxPageSize);
        RuleFor(x => x.SortDirection)
            .Must(d => d is "asc" or "desc")
            .WithMessage("sortDirection debe ser asc o desc.");
    }
}
