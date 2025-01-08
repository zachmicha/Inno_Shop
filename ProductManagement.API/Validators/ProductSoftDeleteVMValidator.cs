using FluentValidation;
using ProductManagement.Application.ViewModels;

namespace ProductManagement.API.Validators
{
    internal sealed class ProductSoftDeleteVMValidator : AbstractValidator<ProductSoftDeleteVM>
    {
        public ProductSoftDeleteVMValidator()
        {
            RuleFor(x=>x.Id).NotEmpty().WithMessage("Provide id");
        }
    }
}
