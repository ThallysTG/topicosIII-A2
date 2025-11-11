namespace Api.Models
{
    public class RecommendationLog
    {
        public int Id { get; set; }
        public int StudentUserId { get; set; }
        public string? PromptSent { get; set; }
        public string? ResponseSummary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}