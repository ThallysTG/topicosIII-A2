using System.Text.Json.Serialization;

namespace Api.Models
{
    public class TrackInstitution
    {
        public int Id { get; set; }
        public int StudyTrackId { get; set; }
        
        [JsonIgnore]
        public StudyTrack? StudyTrack { get; set; }

        public string InstitutionName { get; set; }
        public string CourseName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}