namespace Hydra.MailManagement.DTOs
{
    public class QuickSendMailDTO
    {
        public string To { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string? Body { get; set; }

        public bool IsBodyHtml { get; set; } = true;

        public string? TemplateCode { get; set; }

        public Dictionary<string, string?>? TemplateParameters { get; set; }

        public Guid? EntityId { get; set; }

        public string? EntityType { get; set; }
    }
}
