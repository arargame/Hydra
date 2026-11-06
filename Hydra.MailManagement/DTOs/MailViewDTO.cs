using Hydra.DTOs;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.MailManagement.Entities;
using Hydra.MailManagement.Enums;
using Hydra.Utils;

namespace Hydra.MailManagement.DTOs
{
    /// <summary>
    /// Mail'in ekran sözleşmesi.
    ///
    /// Entity'nin her alanı buraya taşınmaz — <c>Body</c> hariç. Body HTML olabildiği için
    /// liste ekranında değil yalnızca Create/Edit/Details'te, TextArea olarak görünür;
    /// grid'de bir satırı onlarca satır yüksekliğe çıkarırdı.
    ///
    /// <c>Status</c> kasıtlı olarak Create ekranında da düzenlenebilir bırakıldı ama varsayılanı
    /// <c>Draft</c>: modülün sözleşmesi "bir mail asla doğrudan Sent yazılmaz, Draft/Queued →
    /// gönderim → Sent/Failed akar" (bkz. INTEGRATION_PLAYBOOK bölüm 7). Gerçek gönderim
    /// <c>POST api/Mail/Send/{id}</c> ile olur, statüyü oraya bırakmak doğru olan.
    /// </summary>
    [RegisterAsViewDTO(nameof(Mail))]
    public class MailViewDTO : ViewDTO
    {
        public MailStatus Status { get; set; } = MailStatus.Draft;

        public string? From { get; set; }

        public string To { get; set; } = string.Empty;

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public Guid? MailTemplateId { get; set; }

        public string MailTemplate_Code { get; set; } = string.Empty;

        public Guid MailTemplate_Id { get; set; }

        public DateTime? SentDate { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; }

        public MailViewDTO()
        {
            SetControllerName("Mail");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.To),
                displayName: "To",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.Subject),
                displayName: "Subject",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.From),
                displayName: "From");

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.Cc),
                displayName: "Cc");

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.Bcc),
                displayName: "Bcc");

            //Body listede YOK: Create/Edit'te TextArea, Details'te düz metin.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.Body),
                configurations: new List<IConfiguration>
                {
                    new CreateViewConfiguration(elementType: HtmlElementType.TextArea),
                    new EditViewConfiguration(elementType: HtmlElementType.TextArea),
                    new DetailsViewConfiguration()
                },
                displayName: "Body");

            SetConfigurationsViaEnumPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.Status),
                displayName: "Status");

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.SentDate),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(toFilter: new AttributeToFilter(nameof(BetweenFilter)),
                                              toOrder: new AttributeToOrder(isOrderable: true)),
                    new DetailsViewConfiguration()
                },
                displayName: "Sent Date");

            //ErrorMessage ve RetryCount yalnızca teşhis içindir: form ekranlarında görünmezler,
            //çünkü bunları kullanıcı yazmaz — gönderim motoru doldurur.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.ErrorMessage),
                configurations: new List<IConfiguration> { new DetailsViewConfiguration() },
                displayName: "Error Message");

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailViewDTO>(x => x.RetryCount),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(toOrder: new AttributeToOrder(isOrderable: true)),
                    new DetailsViewConfiguration()
                },
                displayName: "Retry Count");

            //Şablon bağı: grid'de Guid yerine şablonun Code'u, formda şablon seçtiren dropdown.
            SetConfigurationsForNavigations(
                leftTableKeyName: "MailTemplateId",
                rightTableName: "MailTemplate",
                columnNameToDisplay: "Code",
                displayName: "Template");

            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
