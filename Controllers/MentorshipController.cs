using Api.Data;
using Api.Dtos;
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
        public async Task<IActionResult> GetMentors([FromQuery] string? area, [FromQuery] int page = 1, [FromQuery] int pageSize = 9)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var query = _context.Users
                .Where(u => u.Role == UserRole.Mentor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(area))
            {
                query = query.Where(u => u.AreaInteresse.Contains(area));
            }

            var totalCount = await query.CountAsync();

            var mentors = await query
                .OrderBy(u => u.Name) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new 
                { 
                    u.Id, 
                    u.Name, 
                    u.AreaInteresse, 
                    u.Bio, 
                    MentorshipId = _context.Mentorships
                        .Where(m => m.MentorId == u.Id && m.StudentId == studentId && (m.Status == MentorshipStatus.Pendente || m.Status == MentorshipStatus.Ativa))
                        .Select(m => m.Id)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var result = new PagedResult<object>
            {
                Items = mentors.Cast<object>().ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return Ok(result);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> UnlinkMentorship(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var mentorship = await _context.Mentorships.FindAsync(id);

            if (mentorship == null) return NotFound(new { Message = "Conexão não encontrada." });

            bool isAuthorized = (userRole == "Aluno" && mentorship.StudentId == userId) || (userRole == "Mentor" && mentorship.MentorId == userId);

            if (!isAuthorized)
            {
                return StatusCode(403, new { Message = "Você não tem permissão para desfazer esta conexão." });
            }

            _context.Mentorships.Remove(mentorship);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Vínculo de mentoria encerrado com sucesso." });
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

        [HttpGet("my-students")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> GetMyActiveStudents()
        {
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var students = await _context.Mentorships
                .Where(m => m.MentorId == mentorId && m.Status == MentorshipStatus.Ativa)
                .Select(m => new
                {
                    m.Student.Id,
                    m.Student.Name
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}