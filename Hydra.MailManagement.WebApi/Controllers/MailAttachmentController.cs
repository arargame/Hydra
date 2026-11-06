using Hydra.DI;
using Hydra.Http;
using Hydra.MailManagement.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hydra.MailManagement.WebApi.Controllers
{
    /// <summary>
    /// MailAttachment için standart CRUD ucu.
    ///
    /// Kendi <c>MailAttachmentService</c>'i YOK — ihtiyaç duyduğu her şeyi generic
    /// <c>Service&lt;MailAttachment&gt;</c> zaten yapıyor (DI'da <c>IService&lt;&gt;</c> açık
    /// generic olarak kayıtlı). Bir entity'ye özel servis, ancak o entity'nin iş kuralı varsa
    /// yazılır; ek dosyasının yok.
    ///
    /// Tek eklenen uç <c>Download</c>: <c>Data</c> (byte[]) kolonu bilinçli olarak
    /// <c>MailAttachmentViewDTO</c>'nun dışında bırakıldığı için dosyanın kendisine
    /// ulaşmanın başka yolu kalmıyordu.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MailAttachmentController : MainController<MailAttachment>
    {
        public MailAttachmentController(IControllerInjector injector) : base(injector)
        {
        }

        /// <summary>
        /// Ekin ikili içeriğini indirir. Kayıt yoksa ya da içerik boşsa ResponseObject zarfıyla
        /// 404 döner — dosya döndüren uçlarda bile hata yolu aynı sözleşmeyi kullanır.
        /// </summary>
        [HttpGet]
        [Route("Download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var attachment = await Service.GetByIdAsync(id);

            if (attachment?.Data == null || attachment.Data.Length == 0)
            {
                var response = new ResponseObject()
                    .SetActionName("Download")
                    .SetId(id)
                    .UseDefaultMessages()
                    .SetSuccess(false);

                response.AddExtraMessage(new ResponseObjectMessage(
                    "NotFound", $"Attachment '{id}' was not found or has no content.", false));

                return new JsonResult(response) { StatusCode = StatusCodes.Status404NotFound };
            }

            var contentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                ? "application/octet-stream"
                : attachment.ContentType;

            var fileName = string.IsNullOrWhiteSpace(attachment.Name)
                ? $"{attachment.FileName}{attachment.Extension}"
                : attachment.Name;

            return File(attachment.Data, contentType, fileName);
        }
    }
}
