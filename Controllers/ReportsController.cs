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

        [HttpGet("my-progress")]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> GetMyProgress()
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var progress = await _reportService.GetStudentProgressAsync(studentId, filterByMentorId: null);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet("mentor/student/{studentId}")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> GetStudentProgressForMentor(int studentId)
        {
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            bool isLinked = await _context.Mentorships.AnyAsync(m => 
                m.MentorId == mentorId && 
                m.StudentId == studentId && 
                m.Status == MentorshipStatus.Ativa);

            if (!isLinked)
            {
                return StatusCode(403, new { Message = "Você não tem permissão para ver este aluno (sem vínculo ativo)." });
            }

            try
            {
                var progress = await _reportService.GetStudentProgressAsync(studentId, filterByMentorId: mentorId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}