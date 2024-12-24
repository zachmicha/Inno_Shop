using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagement.ViewModels;
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
        public async Task<IActionResult> CreateUser([FromBody] UserCreateVM userDto)
        {
            //!! I will have to change to different VM, so i don't input data into fields that are not necessary or auto configured, also using Identity i will have to after creating an user, seed this user ID
            // for Identity.UserRoles table, and combine it with default User Role. also i need to seed 2 Default roles, Admin and User

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


            if (user.IsDeleted == true)
            {
                return NotFound("User not found (soft delete).");
            }

            
            var userDto = new UserReadOnlyVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                IsDeleted = user.IsDeleted,
                EmailConfirmed=user.EmailConfirmed
            };

            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserReadOnlyVM userDto)
        {
            if (userDto == null)
                return BadRequest("User data is required.");

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");

            user.UserName = userDto.UserName;
            user.Email = userDto.Email;
            user.Role = userDto.Role;
           // user.IsDeleted = userDto.IsDeleted;

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
