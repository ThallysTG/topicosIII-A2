using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ActivityStatus newStatus)
        {
            var activity = await _context.StudyActivities.FindAsync(id);

            if (activity == null) return NotFound("Atividade não encontrada.");
            
            activity.ActivityStatus = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Status atualizado com sucesso!", Status = newStatus.ToString() });
        }
    }
}