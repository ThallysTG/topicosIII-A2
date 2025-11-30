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
    public class ReportsController(IReportService reportService, IGeminiService geminiService, ApplicationDbContext context) : ControllerBase
    {
        private readonly IReportService _reportService = reportService;
        private readonly IGeminiService _geminiService = geminiService;
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

        [HttpGet("ai-analysis")]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> GetAiAnalysis()
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var progress = await _reportService.GetStudentProgressAsync(studentId);

                var analysisRequest = new StudentProgressAnalysisRequest
                {
                    StudentName = progress.StudentName,
                    GlobalCompletionRate = progress.GlobalCompletionRate,
                    TotalTracks = progress.Tracks.Count,
                    CompletedTracks = progress.Tracks.Count(t => t.CompletionRate >= 100),
                    TotalMentoringSessions = progress.CompletedMentoringSessions,
                    TrackSummaries = progress.Tracks.Select(t => $"{t.Title}: {t.CompletionRate:F0}%").ToList()
                };

                var aiFeedback = await _geminiService.GetProgressAnalysisAsync(analysisRequest);

                return Ok(new { Feedback = aiFeedback });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao gerar análise IA: " + ex.Message });
            }
        }
    }
}