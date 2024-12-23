using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
      
        public UserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDTO userDto)
        {
            if (userDto == null)
                return BadRequest("User data is required.");

            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                Role = userDto.Role,
                IsDeleted = userDto.IsDeleted
            };

            var result = await _userManager.CreateAsync(user, "DefaultPassword123!"); // Provide a default password

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");

            var userDto = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                IsDeleted = user.IsDeleted
            };

            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDTO userDto)
        {
            if (userDto == null)
                return BadRequest("User data is required.");

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");

            user.UserName = userDto.UserName;
            user.Email = userDto.Email;
            user.Role = userDto.Role;
            user.IsDeleted = userDto.IsDeleted;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
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
    }
}
