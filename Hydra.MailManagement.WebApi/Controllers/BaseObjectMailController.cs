using Hydra.DI;
using Hydra.MailManagement.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hydra.MailManagement.WebApi.Controllers
{
    /// <summary>
    /// BaseObjectMail (mail ↔ herhangi bir kayıt polimorfik bağı) için CRUD ucu.
    ///
    /// Bu uç olmadan Mail detay ekranındaki "Linked Records" sekmesi veri çekemezdi:
    /// <c>CollectionViewSection</c> bir controller adı üzerinden konuşur, doğrudan tabloya
    /// değil. Kendi iş kuralı olmadığı için generic <c>Service&lt;BaseObjectMail&gt;</c>
    /// yeterli — ek bir servis sınıfı yazmak boş soyutlama olurdu.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BaseObjectMailController : MainController<BaseObjectMail>
    {
        public BaseObjectMailController(IControllerInjector injector) : base(injector)
        {
        }
    }
}
