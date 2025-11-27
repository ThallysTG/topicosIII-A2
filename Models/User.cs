namespace Api.Models
{
    public enum UserRole
    {
        Aluno,
        Mentor,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string? AreaInteresse { get; set; }
        public string? InepCode { get; set; }
        public string? Bio { get; set; }
        public ICollection<StudyTrack> StudyTracks { get; set; } = new List<StudyTrack>();
    }
}