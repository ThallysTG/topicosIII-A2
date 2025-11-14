using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController(IReportService reportService, ApplicationDbContext context) : ControllerBase
    {
        private readonly IReportService _reportService = reportService;
        private readonly ApplicationDbContext _context = context;

        [HttpGet("progress/{studentId}")]
        public async Task<IActionResult> GetProgress(int studentId)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            var currentUserId = int.Parse(idClaim?.Value ?? "0");
            
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserRole == nameof(UserRole.Aluno) && currentUserId != studentId)
            {
                return StatusCode(403, new { Message = "Você só pode visualizar seu próprio progresso." });
            }

            if (currentUserRole == nameof(UserRole.Mentor))
            {
                bool isLinked = await _context.Mentorships.AnyAsync(m => 
                    m.MentorId == currentUserId && 
                    m.StudentId == studentId && 
                    m.Status == MentorshipStatus.Ativa);

                if (!isLinked)
                {
                    return StatusCode(403, new { Message = "Este aluno não é seu mentorado ou a conexão não está ativa." });
                }
            }

            try
            {
                var progress = await _reportService.GetStudentProgressAsync(studentId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}