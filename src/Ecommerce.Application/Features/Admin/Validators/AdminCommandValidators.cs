using Ecommerce.Application.Features.Admin;
using FluentValidation;

namespace Ecommerce.Application.Features.Admin.Validators;

public class SaveFamilyCommandValidator : AbstractValidator<SaveFamilyCommand>
{
    public SaveFamilyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class SaveCategoryCommandValidator : AbstractValidator<SaveCategoryCommand>
{
    public SaveCategoryCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class SaveSubcategoryCommandValidator : AbstractValidator<SaveSubcategoryCommand>
{
    public SaveSubcategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class SaveProductCommandValidator : AbstractValidator<SaveProductCommand>
{
    public SaveProductCommandValidator()
    {
        RuleFor(x => x.SubcategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
    }
}

public class SaveVariantCommandValidator : AbstractValidator<SaveVariantCommand>
{
    public SaveVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();
    }
}

public class SetInventoryCommandValidator : AbstractValidator<SetInventoryCommand>
{
    public SetInventoryCommandValidator()
    {
        RuleFor(x => x.QuantityOnHand).GreaterThanOrEqualTo(0);
    }
}

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
    }
}

public class SaveDriverCommandValidator : AbstractValidator<SaveDriverCommand>
{
    public SaveDriverCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
    }
}

public class SaveCoverCommandValidator : AbstractValidator<SaveCoverCommand>
{
    public SaveCoverCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.ImageUrl).NotEmpty();
    }
}

public class ReorderCoversCommandValidator : AbstractValidator<ReorderCoversCommand>
{
    public ReorderCoversCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty();
        RuleFor(x => x.Ids).Must(ids => ids.Count <= 5)
            .WithMessage("Máximo 5 portadas en el orden principal.");
    }
}

public class SaveProductOptionCommandValidator : AbstractValidator<SaveProductOptionCommand>
{
    public SaveProductOptionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class SaveOptionValueCommandValidator : AbstractValidator<SaveOptionValueCommand>
{
    public SaveOptionValueCommandValidator()
    {
        RuleFor(x => x.Value).NotEmpty();
    }
}
