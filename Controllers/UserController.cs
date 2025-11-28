using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    AreaInteresse = u.AreaInteresse,
                    InepCode = u.InepCode
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound(new { Message = "Usuário não encontrado." });

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                AreaInteresse = user.AreaInteresse,
                InepCode = user.InepCode
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest(new { Message = "E-mail já cadastrado." });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = model.Role,
                AreaInteresse = model.AreaInteresse,
                InepCode = model.InepCode
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Usuário criado com sucesso.", UserId = newUser.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto model)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound(new { Message = "Usuário não encontrado." });

            if (model.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest(new { Message = "Este e-mail já está em uso por outro usuário." });
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.Role = model.Role;
            user.AreaInteresse = model.AreaInteresse;
            user.InepCode = model.InepCode;
            user.Bio = model.Bio;

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Usuário atualizado com sucesso." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound(new { Message = "Usuário não encontrado." });

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (id == currentUserId)
            {
                return BadRequest(new { Message = "Você não pode excluir sua própria conta de administrador por aqui." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Usuário excluído com sucesso." });
        }

        [HttpDelete("reset-database")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetDatabase()
        {

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM StudyActivities");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM StudyTracks");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM MentoringSessions");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Mentorships");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RecommendationLogs");

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");

            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Users', RESEED, 0)");

            return Ok(new { Message = "Banco de dados resetado com sucesso (exceto dados do INEP)." });
        }
    }
}