using System.Globalization;
using System.Text;
using Api.Data;
using Api.Dtos;
using Api.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IInepService
    {
        Task ImportCoursesFromCsvAsync(Stream fileStream);
        Task<List<InepCourse>> SearchCoursesAsync(string term, string? locationFilter = null, int page = 1, int pageSize = 50);
    }

    public class InepService(ApplicationDbContext context) : IInepService
    {
        private readonly ApplicationDbContext _context = context;

        private readonly Dictionary<string, string> _estadosPorExtenso = new(StringComparer.OrdinalIgnoreCase)
        {
            {"Acre", "AC"}, {"Alagoas", "AL"}, {"Amapá", "AP"}, {"Amazonas", "AM"}, {"Bahia", "BA"},
            {"Ceará", "CE"}, {"Distrito Federal", "DF"}, {"Espírito Santo", "ES"}, {"Goiás", "GO"},
            {"Maranhão", "MA"}, {"Mato Grosso", "MT"}, {"Mato Grosso do Sul", "MS"}, {"Minas Gerais", "MG"},
            {"Pará", "PA"}, {"Paraíba", "PB"}, {"Paraná", "PR"}, {"Pernambuco", "PE"}, {"Piauí", "PI"},
            {"Rio de Janeiro", "RJ"}, {"Rio Grande do Norte", "RN"}, {"Rio Grande do Sul", "RS"},
            {"Rondônia", "RO"}, {"Roraima", "RR"}, {"Santa Catarina", "SC"}, {"São Paulo", "SP"},
            {"Sergipe", "SE"}, {"Tocantins", "TO"}
        };

        private readonly HashSet<string> _siglasValidas = new()
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
            "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        public async Task ImportCoursesFromCsvAsync(Stream fileStream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.ToUpper().Trim()
            };

            using var reader = new StreamReader(fileStream, Encoding.Latin1);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecordsAsync<InepCsvRow>();
            var coursesToAdd = new List<InepCourse>();
            int batchSize = 5000;

            await foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.NoCurso)) continue;

                coursesToAdd.Add(new InepCourse
                {
                    CourseName = record.NoCurso,
                    InstitutionName = string.IsNullOrWhiteSpace(record.NomeIes) ? $"Instituição (Cód. {record.CoIes})" : record.NomeIes,
                    City = record.Municipio,
                    State = record.Uf
                });

                if (coursesToAdd.Count >= batchSize)
                {
                    await _context.InepCourses.AddRangeAsync(coursesToAdd);
                    await _context.SaveChangesAsync();
                    coursesToAdd.Clear();
                }
            }

            if (coursesToAdd.Any())
            {
                await _context.InepCourses.AddRangeAsync(coursesToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<InepCourse>> SearchCoursesAsync(string term, string? locationFilter = null, int page = 1, int pageSize = 50)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<InepCourse>();

            var query = _context.InepCourses
                .AsNoTracking()
                .Where(c => c.CourseName.Contains(term) && !string.IsNullOrEmpty(c.City));

            if (!string.IsNullOrWhiteSpace(locationFilter))
            {
                var cleanFilter = locationFilter.Trim();
                string? detectedState = null;
                string citySearchPart = cleanFilter;

                var parts = cleanFilter.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
                var lastPart = parts.LastOrDefault()?.ToUpper();

                if (lastPart != null && lastPart.Length == 2 && _siglasValidas.Contains(lastPart))
                {
                    detectedState = lastPart;
                    citySearchPart = cleanFilter.Replace(lastPart, "", StringComparison.OrdinalIgnoreCase).Trim(new[] { ' ', ',', '-' });
                }

                if (detectedState == null)
                {
                    foreach (var estado in _estadosPorExtenso)
                    {
                        if (cleanFilter.Contains(estado.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            detectedState = estado.Value;
                            citySearchPart = cleanFilter.Replace(estado.Key, "", StringComparison.OrdinalIgnoreCase).Trim(new[] { ' ', ',', '-' });
                            break;
                        }
                    }
                }

                if (detectedState != null)
                {
                    query = query.Where(c => c.State == detectedState);

                    if (!string.IsNullOrWhiteSpace(citySearchPart))
                    {
                        query = query.Where(c => c.City.Contains(citySearchPart));
                    }
                }
                else
                {
                    query = query.Where(c => c.City.Contains(cleanFilter) || c.State.Contains(cleanFilter));
                }
            }

            return await query
                .OrderBy(c => c.State)
                .ThenBy(c => c.City)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}