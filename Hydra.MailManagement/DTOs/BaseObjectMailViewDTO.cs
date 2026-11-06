using Hydra.DTOs;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.MailManagement.Entities;
using Hydra.Utils;

namespace Hydra.MailManagement.DTOs
{
    /// <summary>
    /// BaseObjectMail, bir e-postayı sistemdeki HERHANGİ bir kayda bağlayan polimorfik köprüdür:
    /// <c>EntityType</c> + <c>EntityId</c> ikilisi "bu mail hangi Request/SystemUser/Order için
    /// gönderildi" sorusunu cevaplar. FK değil, çünkü hedef tablo çalışma anında belli olur —
    /// bu yüzden navigation yapılandırması YOK, iki kolon da düz metin/Guid olarak gösteriliyor.
    ///
    /// Ekranı okunur tutan tek bağ <c>MailId</c>; o gerçek bir FK olduğu için navigation.
    /// </summary>
    [RegisterAsViewDTO(nameof(BaseObjectMail))]
    public class BaseObjectMailViewDTO : ViewDTO
    {
        public Guid MailId { get; set; }

        public string Mail_Subject { get; set; } = string.Empty;

        public Guid Mail_Id { get; set; }

        public Guid EntityId { get; set; }

        public string EntityType { get; set; } = string.Empty;

        public BaseObjectMailViewDTO()
        {
            SetControllerName("BaseObjectMail");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<BaseObjectMailViewDTO>(x => x.EntityType),
                displayName: "Entity Type",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<BaseObjectMailViewDTO>(x => x.EntityId),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(toFilter: new AttributeToFilter(nameof(EqualFilter))),
                    new DetailsViewConfiguration()
                },
                displayName: "Entity Id");

            SetConfigurationsForNavigations(
                leftTableKeyName: "MailId",
                rightTableName: "Mail",
                columnNameToDisplay: "Subject",
                displayName: "Mail");

            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
