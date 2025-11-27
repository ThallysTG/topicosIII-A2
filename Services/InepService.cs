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
        Task<List<InepCourse>> SearchCoursesAsync(string term, int page = 1, int pageSize = 50);
    }

    public class InepService(ApplicationDbContext context) : IInepService
    {
        private readonly ApplicationDbContext _context = context;

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

        public async Task<List<InepCourse>> SearchCoursesAsync(string term, int page = 1, int pageSize = 50)
        {
            if (string.IsNullOrWhiteSpace(term)) return [];

            return await _context.InepCourses
                .AsNoTracking()
                .Where(c => c.CourseName.Contains(term) && !string.IsNullOrEmpty(c.City))
                .OrderBy(c => c.State)
                .ThenBy(c => c.City)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}