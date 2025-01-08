using FluentValidation;
using UserManagement.Application.ViewModels;

namespace UserManagement.API.Validators
{
    internal sealed class UserCreateVMValidator : AbstractValidator<UserCreateVM>
    {
        public UserCreateVMValidator()
        {
            RuleFor(x=>x.Email).NotEmpty().WithMessage("Emails is required").EmailAddress().WithMessage("Email must be correct format");
            RuleFor(x => x.UserName).NotEmpty().WithMessage("User name is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");

        }
    }
}
