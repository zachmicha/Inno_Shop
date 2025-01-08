using FluentValidation;
using UserManagement.Application.ViewModels;

namespace UserManagement.API.Validators
{
    internal sealed class UserResetPasswordVMValidator : AbstractValidator<UserResetPasswordVM>
    {
        public UserResetPasswordVMValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Provide email");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("Provide password");
        }
    }
}
