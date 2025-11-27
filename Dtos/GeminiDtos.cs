using System.Text.Json.Serialization;

namespace Api.Dtos
{
    public class RecommendationRequestDto
    {
        public int StudentId { get; set; }
        public string SpecificGoal { get; set; }
    }

    public class AiActivity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }

    public class StudyPlanResponse
    {
        public string PlanTitle { get; set; }
        public string Motivation { get; set; }
        public string SuggestedCourse { get; set; }
        public List<AiActivity> Activities { get; set; }
    }

    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }
}