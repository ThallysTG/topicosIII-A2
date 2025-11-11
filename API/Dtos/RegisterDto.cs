namespace Api.Models
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string? AreaInteresse { get; set; }
        public string? InepCode { get; set; }
    }
}