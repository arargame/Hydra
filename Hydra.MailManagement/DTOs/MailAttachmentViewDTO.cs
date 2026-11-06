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
    /// MailAttachment'ın ekran sözleşmesi.
    ///
    /// İki şeyi bilinçli olarak DIŞARIDA bırakıyor:
    /// <list type="bullet">
    /// <item><c>Data</c> (byte[]) — bir ek 10 MB olabilir; liste ekranında 20 satır çekmek
    /// 200 MB'lık bir TableDTO demek olurdu. Dosya içeriği ancak indirme ucundan alınır.</item>
    /// <item><c>ContentType</c> — kullanıcı için anlamlı değil; uzantı zaten gösteriliyor.</item>
    /// </list>
    ///
    /// <c>MailId</c> navigation olarak yapılandırıldığı için grid'de çıplak Guid yerine
    /// e-postanın konusu görünür, Create/Edit ekranında ise Mail seçtiren bir dropdown olur.
    /// </summary>
    [RegisterAsViewDTO(nameof(MailAttachment))]
    public class MailAttachmentViewDTO : ViewDTO
    {
        public Guid? MailId { get; set; }

        public string Mail_Subject { get; set; } = string.Empty;

        public Guid Mail_Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public MailAttachmentViewDTO()
        {
            SetControllerName("MailAttachment");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailAttachmentViewDTO>(x => x.FileName),
                displayName: "File Name",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailAttachmentViewDTO>(x => x.Extension),
                displayName: "Extension",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<MailAttachmentViewDTO>(x => x.FileSize),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(toOrder: new AttributeToOrder(isOrderable: true)),
                    new DetailsViewConfiguration()
                },
                displayName: "Size (bytes)");

            //Ek → Mail bağı. Köprü değil basit FK olduğu için tek çağrı yetiyor.
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
