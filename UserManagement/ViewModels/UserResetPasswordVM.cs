namespace UserManagement.ViewModels
{
    public class UserResetPasswordVM
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
