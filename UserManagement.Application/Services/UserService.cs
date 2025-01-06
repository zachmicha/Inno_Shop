using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UserManagement.Application.Interfaces;
using UserManagement.Application.ViewModels;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public UserService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<(bool IsSuccess, string Message)> CreateUserAsync(UserCreateVM userDto)
        {
            if (userDto == null)
                return (false, "User data is required.");

            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return (true, $"Please confirm your email using this token: {code}");
            }

            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        public async Task<(bool IsSuccess, string Message)> VerifyEmailAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return (false, "Invalid input");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted)
                return (false, "User not found");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result.Succeeded ? (true, "Email confirmed") : (false, "Email verification failed");
        }

        public async Task<(bool IsSuccess, string Token, string Message)> LoginAsync(UserLoginVM userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
                return (false, null, "Email and password are required.");

            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null || user.IsDeleted)
                return (false, null, "Invalid email or password.");

            if (!user.EmailConfirmed)
                return (false, null, "Email is not confirmed.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, userDto.Password);
            if (!isPasswordValid)
                return (false, null, "Invalid email or password.");
            
            var token = GenerateJwtToken(user); 
            return (true, token, "Login successful");
        }

        public async Task<UserReadOnlyVM?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.IsDeleted)
                return null;

            return new UserReadOnlyVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsDeleted = user.IsDeleted,
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<(bool IsSuccess, string Message)> UpdateEmailAndPasswordAsync(string id, UserUpdateEmailAndPasswordVM userDto)
        {
            var validateDto = IsVmNull<UserUpdateEmailAndPasswordVM>(userDto, id);
            if (validateDto ==true)
                return (false,"Incorrect information");

            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted==true)
                return (false, "User doesn't exist");

            user.Email = userDto.Email;


            var passwordResult = await _userManager.ChangePasswordAsync(user, userDto.CurrentPassword, userDto.Password);

            if (passwordResult.Succeeded == true)
            {
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return (true, "Updated email and password");
                }
                else
                {
                    return (false, "Something went wrong");
                }
            }
            return (false, "Something went wrong");
        }

        public async Task<(bool IsSuccess, string Message)> UpdateEmailAsync(string id, UserUpdateEmailVM userDto)
        {
            var validateDto = IsVmNull<UserUpdateEmailVM>(userDto, id);
            if (validateDto ==true)
                return (false, "Incorrect information");

            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted==true)
                return (false, "User not found");

            user.Email = userDto.Email;

            await _userManager.UpdateAsync(user);

            return (true,"Email updated");

        }

        public async Task<(bool IsSuccess, string Message)> UpdatePasswordAsync(string id, UserUpdatePasswordVM userDto)
        {
            var validateDto = IsVmNull<UserUpdatePasswordVM>(userDto, id);
            if (validateDto ==true)
                return (false, "Incorrect information");

            var user = await _userManager.FindByIdAsync(id);

            var passwordResult = await _userManager.ChangePasswordAsync(user, userDto.CurrentPassword, userDto.Password);

            if (passwordResult.Succeeded == true)
            {
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return (true, "Updated password");
                }
                else
                {
                    return (false, "Something went wrong");
                }
            }
            return (false, "Something went wrong");
        }

        public async Task<(bool IsSuccess, string Message)> DeleteUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Incorrect information");
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted == true)
                return (false, "User not found");
            //soft delete user
            user.IsDeleted = true;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return (true, "Deleted user");
            }

            return (false, "Something went wrong");
        }

        public async Task<(bool IsSuccess, string Token, string Message)> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return (false,"", "User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send token via email (use an email service here)
            return (true,$"{token}",$"\n\n Password reset email sent.");
        }

        public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(UserResetPasswordVM model)
        {
            var validateDto = IsVmNull<UserResetPasswordVM>(model);
            if (validateDto != null)
                return (false, "Incorrect information");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted==true) return (false, "User not found");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) return (true, "Password reseted successfully ");

            return (false, "Something went wrong");
        }

        #region JWTimplementation


        private string GenerateJwtToken(User user)
        {
            string secretKey = _configuration["JwtSettings:Key"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email,user.Email)
                    ]),
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:Time")),
                SigningCredentials = credentials,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
            };
            var handler = new JsonWebTokenHandler();
            string token = handler.CreateToken(tokenDescriptor);
            return token;         
        }
        #endregion

        #region HelperMethods
        private static bool IsVmNull<T>(T dto, string id)
        {
            if (dto == null)
                return true;

            if (string.IsNullOrEmpty(id))
                return true;

            return false;
        }
        private static bool IsVmNull<T>(T dto)
        {
            if (dto == null)
                return true;

            return false;
        }

        #endregion
    }
}

