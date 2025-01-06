using Microsoft.AspNetCore.Identity;

namespace UserManagement.Domain.Entities
{
    /// <summary>
    /// You can modify this class, making more properties as needed for your User properties, as needed
    /// </summary>
    public class User : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;
       
    }
}
