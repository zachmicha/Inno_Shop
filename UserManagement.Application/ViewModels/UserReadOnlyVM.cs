namespace UserManagement.Application.ViewModels
{
    public class UserReadOnlyVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsDeleted { get; set; }= false;
        public bool EmailConfirmed { get; set; }
    }
}
