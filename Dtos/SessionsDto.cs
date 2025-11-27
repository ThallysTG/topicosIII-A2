using Api.Models;

namespace Api.Dtos
{
    public class SessionResponseDto
    {
        public int Id { get; set; }
        public string OtherPartyName { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class CreateSessionDto
    {
        public int StudentId { get; set; }
        public DateTime ScheduledAt { get; set; }
    }

    public class UpdateSessionNotesDto
    {
        public string Notes { get; set; }
    }
}