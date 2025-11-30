using System.Text.Json.Serialization;

namespace Api.Models
{
    public enum ActivityStatus
    {
        Pendente,
        EmAndamento,
        Concluida
    }

    public class StudyActivity
    {
        public int Id { get; set; }
        public int StudyTrackId { get; set; }

        [JsonIgnore]
        public StudyTrack? StudyTrack { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public int Order { get; set; }
        public ActivityStatus ActivityStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}