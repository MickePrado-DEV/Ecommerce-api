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
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.LicenseNumber).MaximumLength(80);
        RuleFor(x => x.VehicleType).MaximumLength(80);
        RuleFor(x => x.VehiclePlate).MaximumLength(20);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().When(x => x.CreateLoginAccount);
    }
}

public class SetDriverTemporaryPasswordCommandValidator : AbstractValidator<SetDriverTemporaryPasswordCommand>
{
    public SetDriverTemporaryPasswordCommandValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
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

public class SaveGlobalOptionCommandValidator : AbstractValidator<SaveGlobalOptionCommand>
{
    public SaveGlobalOptionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class SaveGlobalOptionValueCommandValidator : AbstractValidator<SaveGlobalOptionValueCommand>
{
    public SaveGlobalOptionValueCommandValidator()
    {
        RuleFor(x => x.Value).NotEmpty();
    }
}

public class AttachProductOptionCommandValidator : AbstractValidator<AttachProductOptionCommand>
{
    public AttachProductOptionCommandValidator()
    {
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.ValueIds).NotEmpty();
    }
}

public class CreateUserAdminCommandValidator : AbstractValidator<CreateUserAdminCommand>
{
    public CreateUserAdminCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.RoleCodes).NotEmpty();
    }
}

public class UpdateUserAdminCommandValidator : AbstractValidator<UpdateUserAdminCommand>
{
    public UpdateUserAdminCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x).Must(x => x.IsActive.HasValue || x.RoleCodes is not null)
            .WithMessage("Indica isActive y/o roleCodes para actualizar.");
        When(x => x.RoleCodes is not null, () =>
        {
            RuleFor(x => x.RoleCodes!).NotEmpty();
        });
    }
}

public class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionCodes).NotNull();
    }
}
