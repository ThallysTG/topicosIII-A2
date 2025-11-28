namespace EduMentorClient.Services
{
    public class UserState
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public event Action? OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}