using Microsoft.AspNetCore.Identity;
using UserManagement.Common;

namespace UserManagement.Models
{
    /// <summary>
    /// You can modify this class, making more properties as needed for your User properties, as needed
    /// </summary>
    public class User : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;
       
    }
}
