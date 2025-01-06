using UserManagement.Application.ViewModels;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<(bool IsSuccess, string Message)> CreateUserAsync(UserCreateVM userDto);
        Task<(bool IsSuccess, string Message)> VerifyEmailAsync(string email, string code);
        Task<(bool IsSuccess, string Token, string Message)> LoginAsync(UserLoginVM userDto);
        Task<UserReadOnlyVM?> GetUserByIdAsync(string id);
        Task<(bool IsSuccess, string Message)> UpdateEmailAndPasswordAsync(string id, UserUpdateEmailAndPasswordVM userDto);
        Task<(bool IsSuccess, string Message)> UpdateEmailAsync(string id, UserUpdateEmailVM userDto);
        Task<(bool IsSuccess, string Message)> UpdatePasswordAsync(string id, UserUpdatePasswordVM userDto);
        Task<(bool IsSuccess, string Message)> DeleteUserAsync(string id);
        Task<(bool IsSuccess, string Token, string Message)> ForgotPasswordAsync(string email);
        Task<(bool IsSuccess, string Message)> ResetPasswordAsync(UserResetPasswordVM model);
    }
}
