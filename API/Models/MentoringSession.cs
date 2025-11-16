namespace Api.Models
{
    public enum SessionStatus
    {
        Agendada,
        Concluida,
        Cancelada
    }

    public class MentoringSession
    {
        public int Id { get; set; }
        public int StudentUserId { get; set; }
        public int MentorUserId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public SessionStatus SessionStatus { get; set; }
        public string? NotesMentor { get; set; }
    }
}