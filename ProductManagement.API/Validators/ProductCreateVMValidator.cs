using FluentValidation;
using ProductManagement.Application.ViewModels;

namespace ProductManagement.API.Validators
{
    internal sealed class ProductCreateVMValidator : AbstractValidator<ProductCreateVM>
    {
        public ProductCreateVMValidator()
        {
             RuleFor(x=>x.Name).NotEmpty().WithMessage("Name is required").MinimumLength(3).WithMessage("Name must have at least 3 letters");
            RuleFor(x => x.CreatorUserId).NotEmpty().WithMessage("Provide creator's id");
            RuleFor(x => x.Price).NotEmpty().GreaterThanOrEqualTo(1).WithMessage("Price must be greater than 1");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
        }
    }
}
