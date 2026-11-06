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
    /// MailTemplate'in ekran sözleşmesi.
    ///
    /// <c>Code</c> bu entity'nin gerçek anahtarıdır: kod içinden şablon Id ile değil
    /// <c>GetByCodeAsync("WELCOME_USER")</c> ile çağrılır. Bu yüzden listede ilk kolon,
    /// filtrelenebilir ve veritabanında tekil (bkz. TentacleDbContext.OnModelCreating).
    ///
    /// <c>Parameters</c> virgülle ayrılmış bir placeholder listesidir ("UserName,LoginUrl").
    /// Şablon gövdesindeki <c>@@UserName</c> / <c>{{UserName}}</c> yer tutucularının hangileri
    /// olduğunu söyler — doğrulama için değil, şablonu dolduran geliştiriciye/kullanıcıya
    /// belge olması için.
    /// </summary>
    [RegisterAsViewDTO(nameof(MailTemplate))]
    public class MailTemplateViewDTO : ViewDTO
    {
        public string Code { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? Parameters { get; set; }

        public MailTemplateViewDTO()
        {
            SetControllerName("MailTemplate");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailTemplateViewDTO>(x => x.Code),
                displayName: "Code",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailTemplateViewDTO>(x => x.Subject),
                displayName: "Subject",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailTemplateViewDTO>(x => x.Parameters),
                displayName: "Parameters");

            //Gövde HTML: listede değil, form ekranlarında TextArea olarak.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailTemplateViewDTO>(x => x.Body),
                configurations: new List<IConfiguration>
                {
                    new CreateViewConfiguration(elementType: HtmlElementType.TextArea),
                    new EditViewConfiguration(elementType: HtmlElementType.TextArea),
                    new DetailsViewConfiguration()
                },
                displayName: "Body");

            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
