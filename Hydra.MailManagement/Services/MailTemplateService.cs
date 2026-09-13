using Hydra.DI;
using Hydra.MailManagement.Entities;
using Hydra.Services.Core;

namespace Hydra.MailManagement.Services
{
    [RegisterAsService(typeof(IService<MailTemplate>))]
    public class MailTemplateService : Service<MailTemplate>
    {
        public MailTemplateService(ServiceInjector injector) : base(injector)
        {
        }

        public async Task<MailTemplate?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await GetAsync(t => t.Code == code && t.IsActive);
        }

        public async Task<(string Subject, string Body)> RenderAsync(string code, object? model)
        {
            var template = await GetByCodeAsync(code);
            if (template == null)
            {
                throw new KeyNotFoundException($"MailTemplate with code '{code}' was not found.");
            }

            var subject = template.RenderSubject(model);
            var body = template.RenderBody(model);

            return (subject, body);
        }
    }
}
