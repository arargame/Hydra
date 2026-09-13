using Hydra.DI;
using Hydra.Http;
using Hydra.MailManagement.DTOs;
using Hydra.MailManagement.Entities;
using Hydra.MailManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hydra.MailManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailController : MainController<Mail>
    {
        private MailService MailService => Service as MailService
            ?? throw new InvalidOperationException("Service is not registered as MailService");

        public MailController(IControllerInjector injector) : base(injector)
        {
        }

        /// <summary>
        /// Dispatches an existing persisted email by its identifier.
        /// </summary>
        [HttpPost]
        [Route("Send/{id:guid}")]
        public async Task<JsonResult> Send(Guid id, CancellationToken cancellationToken)
        {
            var response = await MailService.SendAsync(id, cancellationToken);
            return new JsonResult(response);
        }

        /// <summary>
        /// Creates and dispatches an email in a single request using the QuickSendMailDTO contract.
        /// </summary>
        [HttpPost]
        [Route("SendQuick")]
        public async Task<JsonResult> SendQuick([FromBody] QuickSendMailDTO dto, CancellationToken cancellationToken)
        {
            var response = await MailService.SendQuickAsync(dto, cancellationToken);
            return new JsonResult(response);
        }

        /// <summary>
        /// Retries dispatching all failed emails that have not exceeded the max retry limit.
        /// </summary>
        [HttpPost]
        [Route("RetryFailed")]
        public async Task<JsonResult> RetryFailed([FromQuery] int maxRetryCount = 3, CancellationToken cancellationToken = default)
        {
            var results = await MailService.RetryFailedMailsAsync(maxRetryCount, cancellationToken);
            var response = new ResponseObject()
                .SetActionName("RetryFailed")
                .UseDefaultMessages()
                .SetSuccess(true)
                .SetData(results);

            return new JsonResult(response);
        }
    }
}
