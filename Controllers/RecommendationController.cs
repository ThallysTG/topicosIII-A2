using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly IInepService _inepService;
        private readonly ApplicationDbContext _context;

        public RecommendationController(IGeminiService geminiService, IInepService inepService, ApplicationDbContext context)
        {
            _geminiService = geminiService;
            _inepService = inepService;
            _context = context;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRecommendation([FromBody] RecommendationRequestDto request)
        {
            var student = await _context.Users.FindAsync(request.StudentId);
            if (student == null) return NotFound("Estudante não encontrado.");

            try
            {
                var aiResponse = await _geminiService.GetStudyRecommendationAsync(student, request.SpecificGoal);

                var coursesFound = new List<InepCourse>();
                if (!string.IsNullOrEmpty(aiResponse.SuggestedCourse))
                {
                    coursesFound = await _inepService.SearchCoursesAsync(
                        aiResponse.SuggestedCourse,
                        aiResponse.SuggestedLocation
                    );
                }

                var newTrack = new StudyTrack
                {
                    StudentUserId = student.Id,
                    Title = aiResponse.PlanTitle,
                    Description = aiResponse.Motivation,
                    Source = RecommendationSource.IA
                };

                int order = 1;
                foreach (var act in aiResponse.Activities)
                {
                    newTrack.StudyActivities.Add(new StudyActivity
                    {
                        Title = act.Title,
                        Description = act.Description,
                        Link = act.Link,
                        Order = order++,
                        ActivityStatus = ActivityStatus.Pendente
                    });
                }

                foreach (var course in coursesFound.Take(5))
                {
                    newTrack.Institutions.Add(new TrackInstitution
                    {
                        InstitutionName = course.InstitutionName,
                        CourseName = course.CourseName,
                        City = course.City,
                        State = course.State
                    });
                }

                _context.StudyTracks.Add(newTrack);
                await _context.SaveChangesAsync();

                var log = new RecommendationLog
                {
                    StudentUserId = student.Id,
                    PromptSent = $"Objetivo: {request.SpecificGoal}",
                    ResponseSummary = JsonSerializer.Serialize(aiResponse),
                    CreatedAt = DateTime.UtcNow
                };
                _context.RecommendationLogs.Add(log);
                await _context.SaveChangesAsync();

                var responseActivities = newTrack.StudyActivities.Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Link = a.Link,
                    IsCompleted = a.ActivityStatus == ActivityStatus.Concluida
                }).ToList();

                return Ok(new
                {
                    AiPlan = new
                    {
                        PlanTitle = newTrack.Title,
                        Motivation = newTrack.Description,
                        SuggestedCourse = aiResponse.SuggestedCourse,
                        SuggestedLocation = aiResponse.SuggestedLocation,
                        Activities = responseActivities
                    },
                    InepOptions = coursesFound
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpPost("create-manual/{studentId}")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> CreateManualTrack(int studentId, [FromBody] CreateManualTrackDto dto)
        {
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            bool isLinked = await _context.Mentorships.AnyAsync(m =>
                m.MentorId == mentorId &&
                m.StudentId == studentId &&
                m.Status == MentorshipStatus.Ativa);

            if (!isLinked)
            {
                return StatusCode(403, new { Message = "Você não tem permissão para criar trilhas para este aluno." });
            }

            var newTrack = new StudyTrack
            {
                StudentUserId = studentId,
                Title = dto.Title,
                Description = dto.Description,
                Source = RecommendationSource.Mentor
            };

            int order = 1;
            foreach (var act in dto.Activities)
            {
                newTrack.StudyActivities.Add(new StudyActivity
                {
                    Title = act.Title,
                    Description = act.Description,
                    Link = act.Link,
                    Order = order++,
                    ActivityStatus = ActivityStatus.Pendente
                });
            }

            _context.StudyTracks.Add(newTrack);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Trilha manual criada com sucesso!", TrackId = newTrack.Id });
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetPromptSuggestions()
        {
            var userIdParam = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdParam, out int userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var area = user.AreaInteresse?.ToLower() ?? "";
            var suggestions = new List<string>();

            if (area.Contains("tec") || area.Contains("prog") || area.Contains("dev") || area.Contains("software") || area.Contains("comput"))
            {
                suggestions.Add("Quero aprender C# e .NET do zero para criar APIs.");
                suggestions.Add("Como me tornar um desenvolvedor Fullstack começando hoje?");
                suggestions.Add("Quero estudar Inteligência Artificial e Python.");
                suggestions.Add("Roteiro de estudos para Engenharia de Software.");
            }
            else if (area.Contains("saúde") || area.Contains("med") || area.Contains("enf") || area.Contains("bio"))
            {
                suggestions.Add("Quero me especializar em Anatomia Humana.");
                suggestions.Add("Quais os primeiros passos para estudar Enfermagem?");
                suggestions.Add("Roteiro de estudos para passar em Medicina.");
            }
            else if (area.Contains("dir") || area.Contains("jur") || area.Contains("adv"))
            {
                suggestions.Add("Quero aprender Direito Civil e Processual.");
                suggestions.Add("Como me preparar para o exame da OAB?");
                suggestions.Add("Roteiro de estudos sobre Direito Digital.");
            }
            else if (area.Contains("eng") || area.Contains("civil") || area.Contains("elet"))
            {
                suggestions.Add("Quero dominar Cálculo e Física para Engenharia.");
                suggestions.Add("Melhores cursos para Engenharia Civil na minha região.");
            }
            else
            {
                var areaFormatada = string.IsNullOrEmpty(user.AreaInteresse) ? "minha carreira" : user.AreaInteresse;
                suggestions.Add($"Quero me tornar um especialista em {areaFormatada}.");
                suggestions.Add($"Quais os fundamentos para começar em {areaFormatada}?");
                suggestions.Add($"Crie uma trilha de estudos intensiva sobre {areaFormatada}.");
            }

            return Ok(suggestions);
        }
    }

    public class ActivityDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class CreateManualTrackDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ManualActivityDto> Activities { get; set; } = new();
    }

    public class ManualActivityDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }
}