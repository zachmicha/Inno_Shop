using FluentValidation;
using UserManagement.Application.ViewModels;

namespace UserManagement.API.Validators
{
 internal sealed class UserUpdatePasswordVMValidator : AbstractValidator<UserUpdatePasswordVM>
    {
        public UserUpdatePasswordVMValidator()
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage("Provide new password");
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Provide current password");
        }
    }
}
