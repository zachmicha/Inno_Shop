using Microsoft.AspNetCore.Identity;
using UserManagement.Common;

namespace UserManagement.Models
{
    public class User : IdentityUser
    {
        public string Role { get; set; } = StaticRoleTypes.User;
        public bool IsDeleted { get; set; } = false;
       
    }
}
