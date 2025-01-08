using FluentValidation;
using UserManagement.Application.ViewModels;

namespace UserManagement.API.Validators
{
    internal sealed class UserUpdateEmailAndPasswordVMValidator : AbstractValidator<UserUpdateEmailAndPasswordVM>
    {
        public UserUpdateEmailAndPasswordVMValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Provide email");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Provide new password");
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Provide current password");
        }
    }
}
