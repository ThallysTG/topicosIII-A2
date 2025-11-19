using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<StudyTrack> StudyTracks { get; set; }
        public DbSet<StudyActivity> StudyActivities { get; set; }
        public DbSet<MentoringSession> MentoringSessions { get; set; }
        public DbSet<RecommendationLog> RecommendationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<StudyTrack>()
                .HasOne<User>()
                .WithMany(u => u.StudyTracks)
                .HasForeignKey(st => st.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudyActivity>()
                .HasOne(sa => sa.StudyTrack)
                .WithMany(st => st.StudyActivities)
                .HasForeignKey(sa => sa.StudyTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MentoringSession>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ms => ms.StudentUserId)
                .HasConstraintName("FK_MentoringSession_Student")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentoringSession>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ms => ms.MentorUserId)
                .HasConstraintName("FK_MentoringSession_Mentor")
                .OnDelete(DeleteBehavior.Restrict); // evita cascade

            modelBuilder.Entity<RecommendationLog>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(rl => rl.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudyActivity>()
                .Property(sa => sa.ActivityStatus)
                .HasConversion<string>();

            modelBuilder.Entity<MentoringSession>()
                .Property(ms => ms.SessionStatus)
                .HasConversion<string>();

            modelBuilder.Entity<StudyTrack>()
                .Property(st => st.Source)
                .HasConversion<string>();
        }
    }
}
