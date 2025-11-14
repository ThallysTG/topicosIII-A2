namespace Api.Dtos
{
    public class StudentProgressDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public double GlobalCompletionRate { get; set; } 
        public int TotalMentoringSessions { get; set; }
        public int CompletedMentoringSessions { get; set; }
        public List<TrackProgressDto> Tracks { get; set; } = new();
    }

    public class TrackProgressDto
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public double CompletionRate { get; set; }
        public string Status { get; set; }
    }
}