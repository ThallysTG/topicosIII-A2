using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MentorshipController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet("mentors")]
        public async Task<IActionResult> GetMentors([FromQuery] string? area)
        {
            var query = _context.Users
                .Where(u => u.Role == UserRole.Mentor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(area))
            {
                query = query.Where(u => u.AreaInteresse.Contains(area));
            }

            var mentors = await query
                .Select(u => new { u.Id, u.Name, u.AreaInteresse, u.Bio, u.InepCode })
                .ToListAsync();

            return Ok(mentors);
        }

        [HttpPost("request/{mentorId}")]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> RequestMentorship(int mentorId, [FromBody] string message)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var exists = await _context.Mentorships.AnyAsync(m => m.StudentId == studentId && m.MentorId == mentorId && (m.Status == MentorshipStatus.Pendente || m.Status == MentorshipStatus.Ativa));

            if (exists) return BadRequest("Você já tem uma conexão ativa ou pendente com este mentor.");

            var connection = new MentorshipConnection
            {
                StudentId = studentId,
                MentorId = mentorId,
                Status = MentorshipStatus.Pendente,
                InitialMessage = message
            };

            _context.Mentorships.Add(connection);
            await _context.SaveChangesAsync();

            return Ok("Solicitação enviada com sucesso.");
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> GetMyRequests()
        {
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var requests = await _context.Mentorships
                .Include(m => m.Student)
                .Where(m => m.MentorId == mentorId)
                .Select(m => new
                {
                    m.Id,
                    StudentId = m.StudentId,
                    StudentName = m.Student.Name,
                    StudentArea = m.Student.AreaInteresse,
                    m.InitialMessage,
                    m.Status,
                    m.RequestDate
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPatch("{requestId}/status")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] MentorshipStatus status)
        {
            var connection = await _context.Mentorships.FindAsync(requestId);
            if (connection == null) return NotFound();

            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (connection.MentorId != mentorId) return Forbid();

            connection.Status = status;
            if (status == MentorshipStatus.Ativa) connection.StartDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok($"Status atualizado para {status}");
        }
    }
}