namespace Hydra.MailManagement.Configuration
{
    public class HydraMailOptions
    {
        public const string SectionName = "MailSettings";

        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 25;

        public string? User { get; set; }

        public string? Password { get; set; }

        public bool EnableSsl { get; set; } = true;

        public bool UseDefaultCredentials { get; set; } = false;

        public string DefaultFromEmail { get; set; } = "noreply@hydra.com";

        public string DefaultFromName { get; set; } = "Hydra System";
    }
}
