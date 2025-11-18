using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public int? MentorUserId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public SessionStatus SessionStatus { get; set; }
        public string? NotesMentor { get; set; }
        [ForeignKey("StudentUserId")]
        [JsonIgnore]
        public User? StudentUser { get; set; }
        
        [ForeignKey("MentorUserId")]
        [JsonIgnore]
        public User? MentorUser { get; set; }
    }
}