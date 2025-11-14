namespace Api.Models
{
    public enum RecommendationSource
    {
        IA,
        Mentor,
        Misto
    }

    public class StudyTrack
    {
        public int Id { get; set; }
        public int StudentUserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public RecommendationSource Source { get; set; }
        public ICollection<StudyActivity> StudyActivities { get; set; } = new List<StudyActivity>();
    }
}