namespace WebAPI.Companies
{
    public class Company
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string[] CcEmails { get; set; } = [];
        public string FromEmail { get; set; } = string.Empty;
    }
}
