using Hydra.DI;
using Hydra.Http;
using Hydra.MailManagement.Entities;
using Hydra.MailManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hydra.MailManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailTemplateController : MainController<MailTemplate>
    {
        private MailTemplateService MailTemplateService => Service as MailTemplateService
            ?? throw new InvalidOperationException("Service is not registered as MailTemplateService");

        public MailTemplateController(IControllerInjector injector) : base(injector)
        {
        }

        /// <summary>
        /// Previews the rendered subject and body of a template with provided model parameters.
        /// </summary>
        [HttpPost]
        [Route("Preview/{code}")]
        public async Task<JsonResult> Preview(string code, [FromBody] Dictionary<string, string?> parameters)
        {
            var response = new ResponseObject()
                .SetActionName("Preview")
                .UseDefaultMessages();

            try
            {
                var (subject, body) = await MailTemplateService.RenderAsync(code, parameters);
                var previewData = new
                {
                    Code = code,
                    Subject = subject,
                    Body = body
                };

                return new JsonResult(response.SetSuccess(true).SetData(previewData));
            }
            catch (Exception ex)
            {
                response.SetSuccess(false);
                response.AddExtraMessage(new ResponseObjectMessage("PreviewError", ex.Message, false));
                return new JsonResult(response);
            }
        }
    }
}
