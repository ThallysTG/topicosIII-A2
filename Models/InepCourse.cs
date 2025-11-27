namespace Api.Models
{
    public class InepCourse
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string InstitutionName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public double? GeneralScore { get; set; }
    }
}