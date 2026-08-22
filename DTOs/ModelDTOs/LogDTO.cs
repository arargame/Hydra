using Hydra.Core;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;

namespace Hydra.DTOs.ModelDTOs
{
    /// <summary>
    /// Log tablosunun view metadata'sı.
    ///
    /// Log ile BaseObject türevleri arasında foreign key YOKTUR — bağ yalnızca
    /// (EntityType, EntityId) çifti üzerinden, select ile kurulur. GenericDetailsView her kaydın
    /// detayına otomatik bir "Loglar" sekmesi ekler ve bu iki kolonu filtre olarak kullanır.
    ///
    /// Kolonlar burada iki ayrı view için ayrı ayrı konfigüre edilir:
    ///   • ListView       — /Log tam liste ekranı (kaynak, metot dahil geniş kolon seti)
    ///   • CollectionView — bir kaydın detayındaki "Loglar" sekmesi (dar, okunabilir set:
    ///                      Tarih · İşlem · Tip · Mesaj)
    /// Ayrım ListViewConfiguration.AlsoUseToCreateCollectionViewConfiguration() ile yapılır.
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

        public LogDTO()
        {
            SetControllerName("Log");
        }

        public override DTO LoadConfigurations()
        {
            // --- Sekmede de listede de görünen kolonlar (öncelik = soldan sağa sıra) ---

            // Tarih — sekmenin ilk kolonu, varsayılan sıralama en yeniden eskiye.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(x => x.AddedDate),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                            toSelect: new AttributeToSelect(0),
                            toFilter: new AttributeToFilter(nameof(BetweenFilter)),
                            //SortingDirection verilince IsOrdered otomatik true olur → varsayılan sıralama.
                            toOrder: new AttributeToOrder(isOrderable: true,
                                                          sortingDirection: SortingDirection.Descending))
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Date");

            // İşlem — "Created / Updated / Deleted" bilgisini taşıyan kolon budur.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.ProcessType),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                            toSelect: new AttributeToSelect(1),
                            toFilter: new AttributeToFilter(nameof(EqualFilter)),
                            toOrder: new AttributeToOrder(isOrderable: true),
                            elementType: HtmlElementType.DropdownList)
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Operation");

            // Tip — Error / Info / Warning
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.Type),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                            toSelect: new AttributeToSelect(2),
                            toFilter: new AttributeToFilter(nameof(EqualFilter)),
                            toOrder: new AttributeToOrder(isOrderable: true),
                            elementType: HtmlElementType.DropdownList)
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Type");

            // Mesaj
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(x => x.Description),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                            toSelect: new AttributeToSelect(3),
                            toFilter: new AttributeToFilter(nameof(ContainsFilter)))
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Message");

            // --- Sadece tam liste ekranında görünen kolonlar (sekmede gürültü yaratır) ---

            // Kaynak — log'u yazan sınıfın adı.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.Category),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toSelect: new AttributeToSelect(4),
                        toFilter: new AttributeToFilter(nameof(ContainsFilter))),

                    new DetailsViewConfiguration()
                },
                displayName: "Source");

            // Metot adı.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(x => x.Name),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toSelect: new AttributeToSelect(5),
                        toFilter: new AttributeToFilter(nameof(ContainsFilter))),

                    new DetailsViewConfiguration()
                },
                displayName: "Method");

            // --- Gevşek bağ kolonları: filtrelenebilir ama SELECT'e girmez ve kullanıcıya
            //     filtre kutusu olarak da gösterilmez. "Loglar" sekmesi bunları arka planda
            //     doldurur (EntityType = nesne adı, EntityId = kaydın Id'si). ---

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.EntityType),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                            toSelect: null,
                            toFilter: new AttributeToFilter(nameof(EqualFilter),
                                                            createFilterComponentFromThis: false))
                        .AlsoUseToCreateCollectionViewConfiguration()
                },
                displayName: "Entity Type");

            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.EntityId),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                            toSelect: null,
                            toFilter: new AttributeToFilter(nameof(EqualFilter),
                                                            createFilterComponentFromThis: false))
                        .AlsoUseToCreateCollectionViewConfiguration()
                },
                displayName: "Entity Id");

            return this;
        }
    }
}
