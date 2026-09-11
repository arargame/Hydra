# 4.1 — BaseObject Anatomisi ve İlk Entity'ler: `Product`, `ProductCategory`

## Neden `BaseObject`'ten başlıyoruz

[1.1](../01-Architecture/01-hydra-core.md#1-baseobjectt--her-şeyin-atası)'de
`BaseObject<T>`'in ne verdiğini gördük: `Id` (zaman sıralı GUID), `Name`,
`Description`, `AddedDate`, `ModifiedDate`, `IsActive`, `RowVersion`
(concurrency). Yeni bir entity yazarken bunların **hiçbirini elle eklemiyoruz**
— bu, DevExpress XAF'ın "her business object bir base class'tan türer" vaadinin
Hydra'daki karşılığı. `Product` yazmaya başladığımızda elimizde zaten şunlar
var, sıfırdan yazmamıza gerek yok:

```csharp
public class Product : BaseObject<Product>
{
    // Id, Name, Description, AddedDate, ModifiedDate, IsActive, RowVersion
    // hepsi burada, hiçbiri yazılmadı
}
```

## `Product` — tasarım kararları

```csharp
// Tentacle/Source/HydraTentacle.Core/Models/Product/Product.cs (önerilen konum —
// diğer entity'ler nasıl HydraTentacle.Core/Models/<Domain>/ altında gruplanmışsa,
// bu da aynı şekilde)
using Hydra.Core;

namespace HydraTentacle.Core.Models.Product
{
    public class Product : BaseObject<Product>
    {
        public string Sku { get; set; } = string.Empty;

        public decimal Price { get; set; }

        // Category navigation — RequestDTO'daki RequestCategory deseninin birebir aynısı
        public Guid ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; } = null!;

        // Owner navigation — Hydra çekirdeğindeki hazır SystemUser'a bağlanıyor.
        // Request.OwnerEmployeeId deseniyle aynı: nullable FK, "kimsenin sahiplenmediği" durum geçerli.
        public Guid? OwnerId { get; set; }
        public Hydra.AccessManagement.SystemUser? Owner { get; set; }

        // ProductImage bkz. 4.2 — bire-çok, CustomFile köprüsü
        public List<ProductImage> Images { get; set; } = new();
    }
}
```

Üç karar, üçü de bilinçli:

**`Name`/`Description` tekrar tanımlanmadı.** `Product`'ın adı ve açıklaması
zaten `BaseObject`'te var. Sık yapılan bir hata, yeni bir entity yazarken
"ProductName" gibi ayrı bir alan açmak — bu hem gereksiz hem de generic view'ların
(`GenericListView` başlığı otomatik `Name`'i basıyor) varsayımını bozuyor.

**`Sku` `string`, boş string varsayılan — `null` değil.** `BaseObject`'in
kendi `string?` alanları (`Name`, `Description`) nullable, ama yeni alanlarda
nullable olup olmayacağına biz karar veriyoruz. `Sku` hiç boş kalmamalı bir
alan olduğu için `string.Empty` varsayılanla non-nullable bıraktık — bu,
[2.2](../02-Core-Systems/02-components-and-theming.md)'de bahsi geçmeyen ama
`GenericFormView`'in "zorunlu alan" yıldızını (nullable olmayan value type'lardan
türetilen) etkileyecek bir karar; `Sku` bir `string` olduğu için o otomasyondan
zaten muaf — form tarafında zorunluluk `AttributeToFilter`/DTO seviyesinde ayrıca
belirtilmesi gerekecek (bkz. 4.4).

**İki FK, iki farklı "zorunluluk" seviyesinde.** `ProductCategoryId` non-nullable
`Guid` — her ürün bir kategoriye ait olmak zorunda. `OwnerId` nullable `Guid?` —
`Request.OwnerEmployeeId`'deki gibi, bir ürünün henüz sahiplenilmemiş olması
geçerli bir durum.

## `ProductCategory` — kasıtlı olarak küçük tutuldu

```csharp
// Tentacle/Source/HydraTentacle.Core/Models/Product/ProductCategory.cs
using Hydra.Core;

namespace HydraTentacle.Core.Models.Product
{
    public class ProductCategory : BaseObject<ProductCategory>
    {
        public List<Product> Products { get; set; } = new();
    }
}
```

`RequestCategory`'nin (`Hydra.Docs` örneğindeki) aynısı — `IsAssignable` gibi
domain'e özel tek bir alanı bile yok, çünkü bu kategori sadece bir sınıflandırma,
iş kuralı taşımıyor. Bilerek minimal bırakıldı; `IHierarchicalObject<ProductCategory>`
(alt kategoriler, bkz. [1.1](../01-Architecture/01-hydra-core.md)) eklemek
kolay olurdu ama bugünkü kapsamda gerek yok — eklenirse ayrı bir bölümde
gösterilecek.

## DTO'lar: `ProductCategoryDTO`

Gerçek `RequestCategoryDTO`'nun şablonunu birebir izliyoruz:

```csharp
// Tentacle/Source/HydraTentacle.Core/DTOs/ProductCategoryDTO.cs
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;

namespace HydraTentacle.Core.DTOs
{
    [RegisterAsViewDTO("ProductCategory")]
    public class ProductCategoryDTO : Hydra.DTOs.ViewDTOs.ViewDTO
    {
        public ProductCategoryDTO()
        {
            SetControllerName("ProductCategory");
        }

        public override Hydra.DTOs.DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ProductCategoryDTO>(x => x.Name),
                displayName: "Name",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ProductCategoryDTO>(x => x.Description),
                displayName: "Description",
                htmlElementTypeInCreationAndEdit: HtmlElementType.TextArea
            );

            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
```

`[RegisterAsViewDTO("ProductCategory")]`'i açıklamakta fayda var, çünkü bu
noktada küçük ama önemli bir ayrım netleşiyor: bu attribute **zorunlu değil**.
`MainController<T>.ResolveViewDTOType` bir DTO'yu sadece **isim konvansiyonuyla**
(`"{Entity}DTO"`) buluyor — `ViewDTOTypeResolver.FindType` yüklenmiş
assembly'lerde `t.Name == "ProductCategoryDTO"` eşleşmesi arıyor, attribute'a
bakmıyor bile (bkz. [1.2](../01-Architecture/02-hydra-webapi.md)). Attribute'un
gerçek işi `ViewDTORegistryLoader.LoadAllViewDTOs` — uygulama açılışında, henüz
hiçbir istek gelmeden, her view type için konfigürasyonu önceden ısıtıyor
(`ViewDTOConfigurationCacheManager.GetOrLoad`). Yani attribute'u unutsak bile
`ProductCategoryDTO` **çalışırdı** — sadece ilk isteğin reflection maliyetini
öne çekme fırsatını kaçırırdık. Küçük ama gerçek bir performans detayı, ilk
bakışta "zorunlu kayıt" gibi göründüğü için not etmeye değer.

## `ProductDTO` — iki farklı navigation, aynı desen

```csharp
// Tentacle/Source/HydraTentacle.Core/DTOs/ProductDTO.cs
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;

namespace HydraTentacle.Core.DTOs
{
    [RegisterAsViewDTO("Product")]
    public class ProductDTO : Hydra.DTOs.ViewDTOs.ViewDTO
    {
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public Guid ProductCategoryId { get; set; }
        public string ProductCategory_Name { get; set; } = string.Empty;
        public Guid ProductCategory_Id { get; set; }

        public Guid? OwnerId { get; set; }
        public string Owner_Name { get; set; } = string.Empty;
        public Guid Owner_Id { get; set; }

        public ProductDTO()
        {
            SetControllerName("Product");
        }

        public override Hydra.DTOs.DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ProductDTO>(x => x.Name),
                displayName: "Product Name",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ProductDTO>(x => x.Sku),
                displayName: "SKU",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // Price: özel bir "decimal helper" yok — RequestDTO.DueDate deseninde
            // olduğu gibi, elle konfigürasyon listesi yazılıyor.
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<ProductDTO>(x => x.Price),
                configurations: new List<IConfiguration>
                {
                    new CreateViewConfiguration(elementType: HtmlElementType.Input, inputType: HtmlInputType.number),
                    new EditViewConfiguration(elementType: HtmlElementType.Input, inputType: HtmlInputType.number),
                    new ListViewConfiguration(toOrder: new AttributeToOrder(isOrderable: true))
                                                .AlsoUseToCreateCollectionViewConfiguration(),
                    new DetailsViewConfiguration()
                },
                displayName: "Price");

            // Category — RequestDTO'daki tek-join örneğiyle birebir aynı desen
            SetConfigurationsForNavigations(
                leftTableKeyName: "ProductCategoryId",
                rightTableName: "ProductCategory",
                columnNameToDisplay: "Name",
                displayName: "Category"
            );

            // Owner — SystemUser'a bağlanan navigation. RequestDTO'daki
            // CreatedByEmployee/OwnerEmployee ile aynı alias mekanizması,
            // ama burada Hydra ÇEKİRDEĞİNDEKİ bir tabloya (SystemUser) bağlanıyoruz —
            // Tentacle'ın kendi entity'lerinden birine değil.
            SetConfigurationsForNavigations(
                leftTableKeyName: "OwnerId",
                rightTableName: "SystemUser",
                rightTableAlias: "Owner",
                rightTableKeyName: "Id",
                columnNameToDisplay: "Name",
                displayName: "Owner",
                leftTableName: "Product"
            );

            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
```

İki navigation da görünüşte aynı metodu çağırıyor
(`SetConfigurationsForNavigations`) ama aralarında bir fark var:
`SystemUser`'ın "görüntülenecek" kolonu da `Name` — çünkü `SystemUser`
`BaseObject<SystemUser>`'dan türüyor, o da `Name` taşıyor (bkz.
[1.1](../01-Architecture/01-hydra-core.md)). Bu, `BaseObject`'in verdiği
tutarlılığın somut faydası: Hydra çekirdeğindeki **herhangi bir** entity'ye
navigation kurarken "bu tabloda hangi kolonu göstereceğim" sorusunun cevabı
her zaman aynı ihtimalle başlıyor — `Name`.

## Dosya haritası (bu bölümde yazılanlar)

| Dosya | Durum |
|---|---|
| `Tentacle/Source/HydraTentacle.Core/Models/Product/Product.cs` | Bu bölümde tasarlandı, henüz repoya yazılmadı |
| `Tentacle/Source/HydraTentacle.Core/Models/Product/ProductCategory.cs` | Bu bölümde tasarlandı, henüz repoya yazılmadı |
| `Tentacle/Source/HydraTentacle.Core/DTOs/ProductDTO.cs` | Bu bölümde tasarlandı, henüz repoya yazılmadı |
| `Tentacle/Source/HydraTentacle.Core/DTOs/ProductCategoryDTO.cs` | Bu bölümde tasarlandı, henüz repoya yazılmadı |

Bilinçli bir ayrım: bu kitap bölümü **tasarım ve gerekçe** üretiyor, dosyaları
gerçek repoya yazmak (ve derleyip test etmek) 4.4/4.5'te, WebApi controller'ları
ve CRUD ekranlarıyla birlikte tek seferde yapılacak — yarım bir entity'yi
repoya yazıp derlemesi bozuk bırakmaktansa, bir modülü bütün halinde teslim
etmek tercih edildi.

---

## ⭐ İleride Yapılacaklar / Not Edilenler

- `Product.Price`'ın `decimal` için Hydra'da özel bir `SetConfigurationsViaDecimalPropertyInfo`
  helper'ı yok — sadece `string`/`bool`/`enum` için var (bkz.
  [1.1 §5](../01-Architecture/01-hydra-core.md)). `Price` gibi sayısal alanlar
  için elle yazılan konfigürasyon listesi (yukarıdaki gibi) her seferinde
  tekrarlanıyor; bir `SetConfigurationsViaNumericPropertyInfo` helper'ı bu
  tekrarı ortadan kaldırabilir. Bu örnek, o boşluğu somut olarak gösteriyor.
- `SystemUser`'a navigation kurmak çalıştı (tasarım seviyesinde) ama
  `SystemUser` `Hydra.AccessManagement` içinde, `Product` `HydraTentacle.Core`
  içinde — iki farklı assembly. Bunun `QueryBuilder`'ın join ürettiği SQL'de
  (iki farklı şemadaki tablo) sorunsuz çalışıp çalışmayacağı 4.3'te,
  gerçek bir sorgu denenerek doğrulanacak.
- `ProductCategoryId` non-nullable bırakıldı — yani "kategorisiz ürün" repo
  seviyesinde imkansız. Bu bilinçli bir kısıtlama; gerekirse 4.2'de
  gözden geçirilebilir.
