using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using UserManagement.Domain.Entities;
using UserManagement.Application.ViewModels;
using UserManagement.Application.Interfaces;


namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("/create-user")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateVM userDto)
        {

            var (isSuccess, message) = await _userService.CreateUserAsync(userDto);
            return isSuccess ? Ok(new { Message = message }) : BadRequest(message);
        }
        [HttpPost("/email-verification")]
        public async Task<IActionResult> EmailVerification(string? email, string? code)
        {
            var (isSuccess, message) = await _userService.VerifyEmailAsync(email, code);
            return isSuccess ? Ok(message) : BadRequest(message);
        }
        /// <summary>
        /// Returns JWT token for authentication/authorization purpouses
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginVM userDto)
        {
            var (isSuccess, token, message) = await _userService.LoginAsync(userDto);
            return isSuccess ? Ok(new { Token = token }) : Unauthorized(message);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user != null ? Ok(user) : NotFound("User not found");
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
           var response = await _userService.UpdateEmailAndPasswordAsync(id, userDto);
            return response
                .IsSuccess ?
                Ok(new { Message = response.Message })
                : BadRequest(new { Message = response.Message });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("/updateEmail/{id}")]
        public async Task<IActionResult> UpdateEmailUser(string id, [FromBody] UserUpdateEmailVM userDto)
        {
            var response = await _userService.UpdateEmailAsync(id, userDto);
            return response.IsSuccess ?
                Ok(new {Message = response.Message})
                : BadRequest(new {Message=response.Message});
       
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("/update-password/{id}")]
        public async Task<IActionResult> UpdatePasswordUser(string id, [FromBody] UserUpdatePasswordVM userDto)
        {
            var response = await _userService.UpdatePasswordAsync(id,userDto);
            return response.IsSuccess ?
                Ok(new { Message = response.Message })
                : BadRequest(new { Message = response.Message });
           
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
           var response = await _userService.DeleteUserAsync(id);
            return response.IsSuccess ?
                Ok(new { Message = response.Message })
                : BadRequest(new { Message = response.Message });
        }

        /// <summary>
        /// Returns token in the respons body, cause what service admin is gonna use gonna depend on him, and token is provided in the response body for the testing purpouses
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
           var response = await _userService.ForgotPasswordAsync(email);
            return response.IsSuccess ?
               Ok(new { Message = response.Message,
               Token=response.Token})
               : BadRequest(new { Message = response.Message });
        }
        /// <summary>
        /// Requires token from ForgotPassword API endpoint.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(UserResetPasswordVM model)
        {
            var response = await _userService.ResetPasswordAsync(model);
            return response.IsSuccess ?
              Ok(new { Message = response.Message })
              : BadRequest(new { Message = response.Message });
        }




  

   
    }
}
