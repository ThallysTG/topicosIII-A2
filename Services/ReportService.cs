using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IReportService
    {
        Task<StudentProgressDto> GetStudentProgressAsync(int studentId, int? filterByMentorId = null);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentProgressDto> GetStudentProgressAsync(int studentId, int? filterByMentorId = null)
        {
            var student = await _context.Users.FindAsync(studentId) ?? throw new Exception("Aluno não encontrado");

            var tracksQuery = _context.StudyTracks
                .Include(t => t.StudyActivities)
                .Where(t => t.StudentUserId == studentId);

            if (filterByMentorId.HasValue)
            {
                tracksQuery = tracksQuery.Where(t => t.Source == RecommendationSource.Mentor);
            }

            var tracks = await tracksQuery.ToListAsync();

            var sessionsQuery = _context.MentoringSessions
                .Where(s => s.StudentUserId == studentId);

            if (filterByMentorId.HasValue)
            {
                sessionsQuery = sessionsQuery.Where(s => s.MentorUserId == filterByMentorId.Value);
            }

            var sessions = await sessionsQuery.ToListAsync();

            var allActivities = tracks.SelectMany(t => t.StudyActivities).ToList();
            int totalActs = allActivities.Count;
            int completedActs = allActivities.Count(a => a.ActivityStatus == ActivityStatus.Concluida);

            var completedWithDates = allActivities
                .Where(a => a.ActivityStatus == ActivityStatus.Concluida && a.CompletedAt.HasValue)
                .OrderBy(a => a.CompletedAt)
                .ToList();

            double avgDays = 0;
            int maxGap = 0;

            if (completedWithDates.Count > 1)
            {
                var gaps = new List<double>();
                
                for (int i = 1; i < completedWithDates.Count; i++)
                {
                    var diff = (completedWithDates[i].CompletedAt!.Value - completedWithDates[i - 1].CompletedAt!.Value).TotalDays;
                    gaps.Add(diff);
                    
                    if (diff > maxGap) maxGap = (int)diff;
                }
                
                if (gaps.Any()) avgDays = gaps.Average();
            }
            else if (completedWithDates.Count == 1)
            {
                avgDays = 0; 
            }

            return new StudentProgressDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                TotalActivities = totalActs,
                CompletedActivities = completedActs,
                GlobalCompletionRate = totalActs > 0 ? (double)completedActs / totalActs * 100 : 0,

                TotalMentoringSessions = sessions.Count,
                CompletedMentoringSessions = sessions.Count(s => s.SessionStatus == SessionStatus.Concluida),

                AverageDaysBetweenTasks = avgDays,
                MaxGapInDays = maxGap,
                LastActivityDate = completedWithDates.LastOrDefault()?.CompletedAt,

                Tracks = [.. tracks.Select(t =>
                {
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
                })]
            };
        }
    }
}