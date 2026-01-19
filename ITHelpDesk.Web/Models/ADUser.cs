namespace ITHelpDesk.Web.Models
{
    public class ADUser
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? MobilePhone { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public List<string> Groups { get; set; } = new List<string>();
    }
}
