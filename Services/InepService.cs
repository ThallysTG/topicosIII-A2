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

        private readonly Dictionary<string, string> _estados = new(StringComparer.OrdinalIgnoreCase)
        {
            {"Acre", "AC"}, {"Alagoas", "AL"}, {"Amapá", "AP"}, {"Amazonas", "AM"}, {"Bahia", "BA"},
            {"Ceará", "CE"}, {"Distrito Federal", "DF"}, {"Espírito Santo", "ES"}, {"Goiás", "GO"},
            {"Maranhão", "MA"}, {"Mato Grosso", "MT"}, {"Mato Grosso do Sul", "MS"}, {"Minas Gerais", "MG"},
            {"Pará", "PA"}, {"Paraíba", "PB"}, {"Paraná", "PR"}, {"Pernambuco", "PE"}, {"Piauí", "PI"},
            {"Rio de Janeiro", "RJ"}, {"Rio Grande do Norte", "RN"}, {"Rio Grande do Sul", "RS"},
            {"Rondônia", "RO"}, {"Roraima", "RR"}, {"Santa Catarina", "SC"}, {"São Paulo", "SP"},
            {"Sergipe", "SE"}, {"Tocantins", "TO"}
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
            if (string.IsNullOrWhiteSpace(term)) return [];

            var query = _context.InepCourses
                .AsNoTracking()
                .Where(c => c.CourseName.Contains(term) && !string.IsNullOrEmpty(c.City));

            if (!string.IsNullOrWhiteSpace(locationFilter))
            {
                var cleanFilter = locationFilter.Trim();

                if (_estados.TryGetValue(cleanFilter, out string? uf))
                {
                    query = query.Where(c => c.State == uf);
                }
                else
                {
                    var parts = cleanFilter.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(p => p.Trim())
                                           .ToArray();

                    if (parts.Length > 1)
                    {
                        var cityPart = parts[0];
                        var statePart = parts.Last();

                        if (statePart.Length == 2)
                        {
                            query = query.Where(c => c.City.Contains(cityPart) && c.State == statePart.ToUpper());
                        }
                        else
                        {
                            query = query.Where(c => c.City.Contains(cityPart));
                        }
                    }
                    else
                    {
                        if (cleanFilter.Length == 2)
                        {
                            query = query.Where(c => c.State == cleanFilter.ToUpper());
                        }
                        else
                        {
                            query = query.Where(c => c.City.Contains(cleanFilter));
                        }
                    }
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