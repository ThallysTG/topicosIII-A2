using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InepController(IInepService inepService) : ControllerBase
    {
        private readonly IInepService _inepService = inepService;

        [HttpPost("import")]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Envie um arquivo CSV válido.");

            try
            {
                using var stream = file.OpenReadStream();
                await _inepService.ImportCoursesFromCsvAsync(stream);
                return Ok("Dados do INEP importados com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro na importação: {ex.Message}");
            }
        }

        [HttpDelete("clear")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearData([FromServices] Api.Data.ApplicationDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE InepCourses");
            return Ok("Base limpa.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] string? locationFilter, // <--- ADICIONADO AQUI
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            // Agora a chamada corresponde à assinatura do serviço: (termo, local, pagina, tamanho)
            var results = await _inepService.SearchCoursesAsync(query, locationFilter, page, pageSize);
            return Ok(results);
        }
    }
}