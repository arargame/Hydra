using Hydra.Core;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;

namespace Hydra.DTOs.ModelDTOs
{
    /// <summary>
    /// Log tablosunun liste/detay metadata'sı.
    ///
    /// Log ile BaseObject türevleri arasında foreign key YOKTUR — bağ yalnızca
    /// (EntityType, EntityId) çifti üzerinden, select ile kurulur. Bu yüzden bu iki kolon
    /// filtrelenebilir olarak işaretlenir ama listede gösterilmez: GenericDetailsView'ın
    /// otomatik "Loglar" sekmesi onları filtre olarak kullanır.
    /// </summary>
    [RegisterAsViewDTO("Log")]
    public class LogDTO : ViewDTO
    {
        // Name ve Description ViewDTO'dan miras gelir.
        public LogType Type { get; set; }
        public LogProcessType ProcessType { get; set; }
        public string? Category { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? Payload { get; set; }

        public LogDTO()
        {
            SetControllerName("Log");
        }

        public override DTO LoadConfigurations()
        {
            // Tip (Error / Info / Warning)
            SetConfigurationsViaEnumPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.Type),
                displayName: "Tip"
            );

            // İşlem tipi (Create / Update / Delete / Login ...)
            SetConfigurationsViaEnumPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.ProcessType),
                displayName: "İşlem"
            );

            // Kaynak (log'u yazan sınıf)
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.Category),
                displayName: "Kaynak",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // Metot / başlık
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.Name),
                displayName: "Metot",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // Açıklama / mesaj
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.Description),
                displayName: "Mesaj",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // --- Gevşek bağ kolonları: filtrelenebilir ama listede gösterilmez ---

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.EntityType),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(toFilter: new AttributeToFilter(nameof(EqualFilter)))
                },
                displayName: "Nesne Tipi");

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.EntityId),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(toFilter: new AttributeToFilter(nameof(EqualFilter)))
                },
                displayName: "Nesne Id");

            // Tarih vb. BaseObject alanları
            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
