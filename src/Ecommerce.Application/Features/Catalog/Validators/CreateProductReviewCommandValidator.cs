using Ecommerce.Application.Features.Catalog.Queries;
using FluentValidation;

namespace Ecommerce.Application.Features.Catalog.Validators;

public class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
{
    public CreateProductReviewCommandValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
    }
}
