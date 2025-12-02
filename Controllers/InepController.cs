using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InepController(IInepService inepService, ApplicationDbContext context) : ControllerBase
    {
        private readonly IInepService _inepService = inepService;
        private readonly ApplicationDbContext _context = context;

        [HttpGet("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatus()
        {
            var existeDados = await _context.InepCourses.AnyAsync();
            var ultimaAtualizacao = existeDados ? DateTime.Now : (DateTime?)null;

            return Ok(new
            {
                JaExiste = existeDados,
                DataUltimaAtualizacao = ultimaAtualizacao,
                Mensagem = existeDados ? "Base de dados carregada." : "Aguardando importação."
            });
        }

        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Envie um arquivo CSV válido.");

            var existeDados = await _context.InepCourses.AnyAsync();

            if (existeDados)
            {
                return BadRequest("A base de dados já contém registros. Limpe os dados antes de importar novamente.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                await _inepService.ImportCoursesFromCsvAsync(stream);
                return Ok(new { message = "Dados do INEP importados com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro na importação: {ex.Message}");
            }
        }

        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv()
        {
            var courses = await _context.InepCourses.AsNoTracking().ToListAsync();

            if (courses == null || !courses.Any())
            {
                return NotFound("Nenhum dado encontrado para exportar.");
            }

            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(courses);
                await writer.FlushAsync();

                return File(memoryStream.ToArray(), "text/csv", "dados_inep_exportados.csv");
            }
        }

        [HttpDelete("clear")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearData()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE InepCourses");
                return Ok(new { message = "Base de dados limpa com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao limpar base: {ex.Message}");
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] string? locationFilter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var results = await _inepService.SearchCoursesAsync(query, locationFilter, page, pageSize);
            return Ok(results);
        }
    }
}