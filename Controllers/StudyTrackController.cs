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
    public class StudyTrackController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetMyTracks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdParam = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdParam, out int userId)) return Unauthorized();

            var query = _context.StudyTracks
                .Where(t => t.StudentUserId == userId)
                .OrderByDescending(t => t.Id);

            var totalCount = await query.CountAsync();

            var tracks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    Source = t.Source.ToString(),
                    TotalActivities = t.StudyActivities.Count,
                    CompletedActivities = t.StudyActivities.Count(a => a.ActivityStatus == ActivityStatus.Concluida),
                    
                    Activities = t.StudyActivities.OrderBy(a => a.Order).Select(a => new
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

            var result = new PagedResult<object>
            {
                Items = tracks.Cast<object>().ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return Ok(result);
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