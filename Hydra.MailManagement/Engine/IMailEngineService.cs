using Hydra.MailManagement.Entities;

namespace Hydra.MailManagement.Engine
{
    public interface IMailEngineService
    {
        Task<(bool Success, string? ErrorMessage)> SendMailAsync(Mail mail, CancellationToken cancellationToken = default);
    }
}
