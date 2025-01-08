using FluentValidation;
using UserManagement.Application.ViewModels;

namespace UserManagement.API.Validators
{
    internal sealed class UserUpdateEmailVMValidator : AbstractValidator<UserUpdateEmailVM>
    {
        public UserUpdateEmailVMValidator()
        {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Provide email");
        }
    }
}
