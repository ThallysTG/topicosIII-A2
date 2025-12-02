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
    public class StudyTrackController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetMyTracks()
        {
            var userIdParam = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdParam, out int userId)) return Unauthorized();

            var tracks = await _context.StudyTracks
                .Where(t => t.StudentUserId == userId)
                .Include(t => t.StudyActivities.OrderBy(a => a.Order))
                .Include(t => t.Institutions)
                .OrderByDescending(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    Source = t.Source.ToString(),
                    TotalActivities = t.StudyActivities.Count,
                    CompletedActivities = t.StudyActivities.Count(a => a.ActivityStatus == ActivityStatus.Concluida),

                    Activities = t.StudyActivities.Select(a => new
                    {
                        a.Id,
                        a.Title,
                        a.Description,
                        a.Link,
                        IsCompleted = a.ActivityStatus == ActivityStatus.Concluida
                    }),

                    SavedInstitutions = t.Institutions.Select(i => new
                    {
                        i.InstitutionName,
                        i.CourseName,
                        i.City,
                        i.State
                    })
                })
                .ToListAsync();

            return Ok(tracks);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrack(int id)
        {
            var userIdParam = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdParam, out int userId)) return Unauthorized();

            var track = await _context.StudyTracks.FindAsync(id);

            if (track == null)
                return NotFound(new { Message = "Trilha não encontrada." });

            if (track.StudentUserId != userId)
                return StatusCode(403, new { Message = "Você não tem permissão para excluir esta trilha." });

            _context.StudyTracks.Remove(track);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Trilha excluída com sucesso." });
        }
    }
}