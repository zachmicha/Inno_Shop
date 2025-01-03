﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagement.ViewModels;
using UserManagement.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net.Http;
using System.Text.Json;


namespace UserManagement.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public UserController(UserManager<User> userManager, IConfiguration configuration, HttpClient httpClient)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpClient = httpClient;
        }
        [HttpPost("/create-user")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateVM userDto)
        {
            
            if (userDto == null)
                return BadRequest("User data is required.");

            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email                
            };

            var result = await _userManager.CreateAsync(user, userDto.Password); // Provide a default password


            if (result.Succeeded)
            {
                //Generate token for email validation
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var message = $"Please confirm your email using this token : {code}";

                return CreatedAtAction(nameof(GetUser),
                    new { id = user.Id  },
                    new {user = userDto, message= message});
            }

            return BadRequest(result.Errors);
        }
        [HttpPost("/email-verification")]
        public async Task<IActionResult> EmailVerification(string? email, string? code)
        {
            if (email == null || code==null)
            {
                return BadRequest("Invalid input");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user ==null || user.IsDeleted==true)
            {
                return BadRequest("User not found");
            }
            var isVerified = await _userManager.ConfirmEmailAsync(user,code);
            if (isVerified.Succeeded)
            {
                return Ok("Email confirmed");
            }
            return BadRequest("Something went wrong");
        }
        /// <summary>
        /// Returns JWT token for authentication/authorization purpouses
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginVM userDto)
        {
            if (userDto == null || string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
            {
                return BadRequest("Email and password are required.");
            }


            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            if (user.IsDeleted == true)
            {
                return NotFound("User not found (soft delete).");
            }
            if (user.EmailConfirmed==false)
            {
                return BadRequest("Email is not confirmerd");
            }
            var result = await _userManager.CheckPasswordAsync(user, userDto.Password);
                if (result==false)
                {
                    return Unauthorized("Invalid email or password.");
                }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID is required.");
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");


            if (user.IsDeleted == true)
            {
                return NotFound("User not found (soft delete).");
            }

            
            var userDto = new UserReadOnlyVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsDeleted = user.IsDeleted,
                EmailConfirmed=user.EmailConfirmed
            };

            return Ok(userDto);
        }
        /// <summary>
        /// You have to provide current not hashed password for the User in the API call, method will hash it, and compare it in database with saved hashed version of that password.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userDto"></param>
        /// <returns></returns>
        /// 
        [Authorize(AuthenticationSchemes = "Bearer")]        
        [HttpPut("/updateEmailAndPassword/{id}")]
        public async Task<IActionResult> UpdateEmailAndPasswordUser(string id, [FromBody] UserUpdateEmailAndPasswordVM userDto)
        {

            var validateDto = ValidateVmsForNulls<UserUpdateEmailAndPasswordVM>(userDto, id);
            if (validateDto != null)
                return validateDto;

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");

            user.Email = userDto.Email;
            

            var passwordResult = await _userManager.ChangePasswordAsync(user, userDto.CurrentPassword,userDto.Password);

            if (passwordResult.Succeeded == true)
            {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }

           
           return BadRequest("Something went wrong");         
          
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("/updateEmail/{id}")]
        public async Task<IActionResult> UpdateEmailUser(string id, [FromBody] UserUpdateEmailVM userDto)
        {

            var validateDto = ValidateVmsForNulls<UserUpdateEmailVM>(userDto, id);
            if (validateDto != null)
                return validateDto;

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");

            user.Email = userDto.Email;

           await _userManager.UpdateAsync(user);

            return Ok("Email updated");

        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("/update-password/{id}")]
        public async Task<IActionResult> UpdatePasswordUser(string id, [FromBody] UserUpdatePasswordVM userDto)
        {

            var validateDto = ValidateVmsForNulls<UserUpdatePasswordVM>(userDto, id);
            if (validateDto != null)
                return validateDto;

            var user = await _userManager.FindByIdAsync(id);

            var passwordResult = await _userManager.ChangePasswordAsync(user, userDto.CurrentPassword, userDto.Password);

            if (passwordResult.Succeeded == true)
            {
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Password updated successfully." });
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            return BadRequest("Something went wrong");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID is required.");
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted == true)
                return NotFound("User not found.");
            //soft delete user
            user.IsDeleted = true;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Returns token in the respons body, cause what service admin is gonna use gonna depend on him, and token is provided in the response body for the testing purpouses
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send token via email (use an email service here)
            return Ok($"Password reset email sent. \n {token}");
        }
        /// <summary>
        /// Requires token from ForgotPassword API endpoint.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(UserResetPasswordVM model)
        {
           var validateDto= ValidateVmsForNulls<UserResetPasswordVM>(model);
            if (validateDto != null)
                return validateDto;

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) return Ok("Password reset successfully.");

            return BadRequest(result.Errors);
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
            //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            //        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //        var claims = new[]
            //        {
            //    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            //    new Claim(JwtRegisteredClaimNames.Email, user.Email),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //};

            //        var token = new JwtSecurityToken(
            //            issuer: _configuration["JwtSettings:Issuer"],
            //            audience: _configuration["JwtSettings:Audience"],
            //            claims: claims,
            //            expires: DateTime.UtcNow.AddHours(1),
            //            signingCredentials: credentials
            //        );

            //        return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region HelperMethods
        private static IActionResult ValidateVmsForNulls<T>(T dto, string id)
        {
            if (dto == null)
                return new BadRequestObjectResult("Input data is required.");

            if (string.IsNullOrEmpty(id))
                return new BadRequestObjectResult("User ID is required.");

            return null;
        }
        private static IActionResult ValidateVmsForNulls<T>(T dto)
        {
            if (dto == null)
                return new BadRequestObjectResult("Input data is required.");
            
            return null;
        }

        #endregion
    }
}
