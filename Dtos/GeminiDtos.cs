using System.Text.Json.Serialization;

namespace Api.Dtos
{
    public class RecommendationRequestDto
    {
        public int StudentId { get; set; }
        public string SpecificGoal { get; set; } = string.Empty;
    }

    public class AiActivity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }

    public class StudyPlanResponse
    {
        public string PlanTitle { get; set; } = string.Empty;
        public string Motivation { get; set; } = string.Empty;
        public string SuggestedCourse { get; set; } = string.Empty;
        public string? SuggestedLocation { get; set; }
        public List<AiActivity> Activities { get; set; } = [];
    }

    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = [];
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = [];
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; } = [];
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; } = new();
    }

    public class StudentProgressAnalysisRequest
    {
        public string StudentName { get; set; } = string.Empty;
        public double GlobalCompletionRate { get; set; }
        public int TotalTracks { get; set; }
        public int CompletedTracks { get; set; }
        public int TotalMentoringSessions { get; set; }
        public double AverageDaysBetweenTasks { get; set; } 
        public int MaxGapInDays { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public List<string> TrackSummaries { get; set; } = [];
    }

    public class ProgressAnalysisResponse
    {
        public string AnalysisText { get; set; } = string.Empty;
    }
}