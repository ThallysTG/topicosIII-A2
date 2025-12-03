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
                    City = u.City,
                    State = u.State
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
                City = user.City,
                State = user.State
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
                City = model.City,
                State = model.State
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Usuário criado com sucesso.", UserId = newUser.Id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ForceDeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (id == currentUserId)
            {
                return BadRequest(new { Message = "Você não pode excluir sua própria conta enquanto está logado." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { Message = "Usuário não encontrado." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var logs = _context.RecommendationLogs.Where(x => x.StudentUserId == id);
                _context.RecommendationLogs.RemoveRange(logs);

                var sessions = _context.MentoringSessions.Where(x => x.StudentUserId == id || x.MentorUserId == id);
                _context.MentoringSessions.RemoveRange(sessions);

                var mentorships = _context.Mentorships.Where(x => x.StudentId == id || x.MentorId == id);
                _context.Mentorships.RemoveRange(mentorships);

                var tracks = _context.StudyTracks.Where(x => x.StudentUserId == id);
                _context.StudyTracks.RemoveRange(tracks);

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Usuário e todos os seus dados vinculados foram excluídos." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = $"Erro ao excluir usuário: {ex.Message}" });
            }
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