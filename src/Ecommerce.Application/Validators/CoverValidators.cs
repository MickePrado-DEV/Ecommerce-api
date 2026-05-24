using Ecommerce.Application.DTOs.Admin;
using FluentValidation;

namespace Ecommerce.Application.Validators;

public class SaveCoverRequestValidator : AbstractValidator<SaveCoverRequest>
{
    public SaveCoverRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.ImageUrl).NotEmpty();
    }
}
