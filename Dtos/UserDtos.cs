using Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? AreaInteresse { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string? AreaInteresse { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class UpdateUserDto
    {
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string? Password { get; set; } 
        public UserRole Role { get; set; }
        public string? AreaInteresse { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}