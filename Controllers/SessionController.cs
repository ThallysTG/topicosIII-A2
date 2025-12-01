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
    public class SessionController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetMySessions()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.MentoringSessions.AsQueryable();

            if (role == nameof(UserRole.Mentor))
            {
                query = query.Where(s => s.MentorUserId == userId)
                             .Include(s => s.StudentUser);
            }
            else
            {
                query = query.Where(s => s.StudentUserId == userId)
                             .Include(s => s.MentorUser);
            }

            var sessions = await query.OrderByDescending(s => s.ScheduledAt).ToListAsync();

            var dtos = sessions.Select(s => new SessionResponseDto
            {
                Id = s.Id,
                OtherPartyName = role == nameof(UserRole.Mentor) 
                    ? (s.StudentUser?.Name ?? "Aluno") 
                    : (s.MentorUser?.Name ?? "Mentor"),
                ScheduledAt = s.ScheduledAt,
                Status = s.SessionStatus.ToString(),
                Notes = s.NotesMentor
            });

            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto dto)
        {
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            bool isLinked = await _context.Mentorships.AnyAsync(m => 
                m.MentorId == mentorId && 
                m.StudentId == dto.StudentId && 
                m.Status == MentorshipStatus.Ativa);

            if (!isLinked) return BadRequest("Você não tem conexão ativa com este aluno.");

            var session = new MentoringSession
            {
                MentorUserId = mentorId,
                StudentUserId = dto.StudentId,
                ScheduledAt = dto.ScheduledAt,
                SessionStatus = SessionStatus.Agendada
            };

            _context.MentoringSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok("Sessão agendada com sucesso.");
        }

        [HttpPatch("{id}/feedback")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> AddFeedback(int id, [FromBody] UpdateSessionNotesDto dto)
        {
            var session = await _context.MentoringSessions.FindAsync(id);
            if (session == null) return NotFound();

            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (session.MentorUserId != mentorId) return Forbid();

            session.NotesMentor = dto.Notes;
            session.SessionStatus = SessionStatus.Concluida;

            await _context.SaveChangesAsync();
            return Ok("Feedback registrado.");
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelSession(int id)
        {
            var session = await _context.MentoringSessions.FindAsync(id);
            if (session == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (session.MentorUserId != userId && session.StudentUserId != userId) return Forbid();

            session.SessionStatus = SessionStatus.Cancelada;
            await _context.SaveChangesAsync();
            return Ok("Sessão cancelada.");
        }
    }
}