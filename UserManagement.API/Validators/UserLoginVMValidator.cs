using FluentValidation;
using UserManagement.Application.ViewModels;

namespace UserManagement.API.Validators
{
    internal sealed class UserLoginVMValidator : AbstractValidator<UserLoginVM>
    {
        public UserLoginVMValidator()
        {
            RuleFor(x=>x.Email).NotEmpty().WithMessage("Provide email");
            RuleFor(x=>x.Password).NotEmpty().WithMessage("Provide password");
        }
    }
}
