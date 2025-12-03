namespace Api.Dtos
{
    public class UpdateProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string AreaInteresse { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}