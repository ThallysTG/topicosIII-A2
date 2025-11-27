using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IReportService
    {
        Task<StudentProgressDto> GetStudentProgressAsync(int studentId);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentProgressDto> GetStudentProgressAsync(int studentId)
        {
            var student = await _context.Users.FindAsync(studentId) ?? throw new Exception("Aluno não encontrado");
            var tracks = await _context.StudyTracks
                .Include(t => t.StudyActivities)
                .Where(t => t.StudentUserId == studentId)
                .ToListAsync();

            var sessions = await _context.MentoringSessions
                .Where(s => s.StudentUserId == studentId)
                .ToListAsync();

            var allActivities = tracks.SelectMany(t => t.StudyActivities).ToList();
            int totalActs = allActivities.Count;
            int completedActs = allActivities.Count(a => a.ActivityStatus == ActivityStatus.Concluida);

            var report = new StudentProgressDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                TotalActivities = totalActs,
                CompletedActivities = completedActs,
                GlobalCompletionRate = totalActs > 0 ? (double)completedActs / totalActs * 100 : 0,
                
                TotalMentoringSessions = sessions.Count,
                CompletedMentoringSessions = sessions.Count(s => s.SessionStatus == SessionStatus.Concluida),
                
                Tracks = tracks.Select(t => {
                    int tTotal = t.StudyActivities.Count;
                    int tCompleted = t.StudyActivities.Count(a => a.ActivityStatus == ActivityStatus.Concluida);
                    double rate = tTotal > 0 ? (double)tCompleted / tTotal * 100 : 0;

                    return new TrackProgressDto
                    {
                        TrackId = t.Id,
                        Title = t.Title ?? "Sem título",
                        TotalActivities = tTotal,
                        CompletedActivities = tCompleted,
                        CompletionRate = rate,
                        Status = rate == 100 ? "Concluída" : (rate > 0 ? "Em Andamento" : "Não Iniciada")
                    };
                }).ToList()
            };

            return report;
        }
    }
}