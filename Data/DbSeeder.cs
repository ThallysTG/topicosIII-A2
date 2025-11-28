using Api.Models;
using BCrypt.Net;

namespace Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedUsers(ApplicationDbContext context)
        {
            if (context.Users.Any()) return;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("12345678");

            var users = new List<User>
            {
                new() { 
                    Name = "Administrador", 
                    Email = "admin@edumentor.com", 
                    PasswordHash = passwordHash, 
                    Role = UserRole.Admin,
                    AreaInteresse = "Gestão",
                    Bio = "Gestor do Sistema"
                },
                new() { 
                    Name = "Prof. Xavier", 
                    Email = "mentor@edumentor.com", 
                    PasswordHash = passwordHash, 
                    Role = UserRole.Mentor,
                    AreaInteresse = "Tecnologia e Inovação",
                    Bio = "Especialista em IA e Desenvolvimento de Software."
                },
                new() { 
                    Name = "Aluno Teste", 
                    Email = "aluno@edumentor.com", 
                    PasswordHash = passwordHash, 
                    Role = UserRole.Aluno,
                    AreaInteresse = "Programação",
                    Bio = "Estudante dedicado."
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}