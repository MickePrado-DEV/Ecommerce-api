using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Shipments;
using FluentValidation;

namespace Ecommerce.Application.Validators;

public class SaveFamilyRequestValidator : AbstractValidator<SaveFamilyRequest>
{
    public SaveFamilyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class SaveSubcategoryRequestValidator : AbstractValidator<SaveSubcategoryRequest>
{
    public SaveSubcategoryRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.SubcategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
    }
}

public class SaveVariantRequestValidator : AbstractValidator<SaveVariantRequest>
{
    public SaveVariantRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();
    }
}

public class SetInventoryRequestValidator : AbstractValidator<SetInventoryRequest>
{
    public SetInventoryRequestValidator()
    {
        RuleFor(x => x.QuantityOnHand).GreaterThanOrEqualTo(0);
    }
}

public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
    }
}

public class SaveDriverRequestValidator : AbstractValidator<SaveDriverRequest>
{
    public SaveDriverRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
    }
}
