namespace Api.Models
{
    public class InepCourseData
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string> AreasFoco { get; set; } = [];
        public Dictionary<string, double> DesempenhoNacional { get; set; } = [];
    }
}