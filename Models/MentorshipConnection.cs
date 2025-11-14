using System.Text.Json.Serialization;

namespace Api.Models
{
    public enum MentorshipStatus
    {
        Pendente,
        Ativa,
        Rejeitada,
        Concluida
    }

    public class MentorshipConnection
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        [JsonIgnore]
        public User? Student { get; set; }
        public int MentorId { get; set; }
        [JsonIgnore]
        public User? Mentor { get; set; }
        public MentorshipStatus Status { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
    
        public string? InitialMessage { get; set; } 
    }
}