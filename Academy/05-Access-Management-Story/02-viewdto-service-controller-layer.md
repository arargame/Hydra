# 5.2 — ViewDTO, Service, Controller: Konvansiyonla Bulunan Zincir

[1.1](../01-Architecture/01-hydra-core.md) `ViewDTO`'nun ne işe yaradığını
anlatmıştı: bir entity'nin hangi alanlarının Listede/Formda/Detayda
görüneceğini, nasıl filtreleneceğini, hangi HTML elementiyle çizileceğini
tarif eden meta-veri sınıfı. Bu bölümde asıl ilginç kısmı gösteriyoruz: **bu
üç entity, ViewDTO'larını nereden ve nasıl buluyor** — ve bu, Hydra'nın "core
kütüphane / tüketen uygulama" ayrımının en net göründüğü yer.

## Konvansiyonla bulma: `ViewDTOTypeResolver`

`MainController<T>`, bir istek geldiğinde `T` için hangi `ViewDTO` sınıfının
kullanılacağını **bilmiyor** — bunu her istekte konvansiyonla arıyor:

```csharp
// Hydra.WebApi/Controllers/Base/MainController.cs
protected Type? ResolveViewDTOType(string? viewDTOTypeName = null)
{
    return ViewDTOTypeResolver.Resolve(viewDTOTypeName, typeof(T).Name);
}
```

```csharp
// Hydra/DTOs/ViewDTOs/ViewDTOTypeResolver.cs
// Convention: "{Entity}DTO" first, then "{Entity}ViewDTO".
public static Type? Resolve(string? viewDTOTypeName, string? entityName = null)
{
    var candidates = new List<string>();
    if (!string.IsNullOrWhiteSpace(viewDTOTypeName)) candidates.Add(viewDTOTypeName!);
    if (!string.IsNullOrWhiteSpace(entityName))
    {
        candidates.Add($"{entityName}DTO");
        candidates.Add($"{entityName}ViewDTO");
    }
    // ...yüklü assembly'lerde bu isimde, ViewDTO'dan türeyen ilk sınıfı bulur, cache'ler.
}
```

Yani `Role` için önce `RoleDTO`, bulunamazsa `RoleViewDTO` adında bir sınıf
aranıyor — **hangi assembly'de olduğu önemli değil**, `AppDomain.CurrentDomain.GetAssemblies()`
üzerinden taranıyor. Bu tek satırlık kural, aşağıdaki mimari kararı mümkün
kılıyor.

## Entity core'da, ekran konfigürasyonu tüketen uygulamada

`SystemUser`, `Role`, `Permission` entity'leri `Hydra` DLL'inde yaşıyor —
her uygulama (Tentacle, ileride başka bir proje) aynı üç entity'yi tüketir.
Ama **"Role listesinde hangi kolonlar görünsün, Permission'da `Type` alanı
nasıl bir dropdown olsun"** sorusunun cevabı bir uygulamadan diğerine
değişebilir. Hydra bu ayrımı, ViewDTO sınıflarının konumuyla yapıyor:

| Entity | ViewDTO sınıfı | Nerede yaşıyor |
|---|---|---|
| `SystemUser` | `SystemUserViewDTO` | `Hydra/DTOs/ModelDTOs/SystemUserDTO/` — **core'da** |
| `Role` | `RoleDTO` | `Tentacle/Source/HydraTentacle.Core/DTOs/` — **tüketen uygulamada** |
| `Permission` | `PermissionDTO` | `Tentacle/Source/HydraTentacle.Core/DTOs/` — **tüketen uygulamada** |

`SystemUser` için core'da bir ViewDTO olmasının sebebi basit: `Email` alanı
(login için gerekli) her uygulamada aynı şekilde göstermek mantıklı, o yüzden
Hydra bunu **varsayılan** olarak sağlıyor. `Role`/`Permission` için ise Hydra
hiçbir varsayım yapmıyor — Tentacle kendi `RoleDTO`/`PermissionDTO`'sunu
yazıyor, ve `ViewDTOTypeResolver`'ın assembly-agnostik taraması bunu otomatik
buluyor. Bu, XAF benzeri RAD sistemlerdeki "model reusable, presentation
per-app" prensibinin Hydra'daki somut karşılığı.

`RoleDTO`'nun tamamı, bir önceki bölümdeki `SetConfigurationsViaStringPropertyInfo`
gibi yardımcıların gerçek kullanımını gösteriyor:

```csharp
// Tentacle/Source/HydraTentacle.Core/DTOs/RoleDTO.cs
[RegisterAsViewDTO("Role")]
public class RoleDTO : Hydra.DTOs.ViewDTOs.ViewDTO
{
    public RoleDTO() => SetControllerName("Role");

    public override Hydra.DTOs.DTO LoadConfigurations()
    {
        SetConfigurationsViaStringPropertyInfo(
            propertyInfo: ReflectionHelper.GetPropertyOf<RoleDTO>(x => x.Name),
            displayName: "Name",
            attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)));

        SetConfigurationsViaStringPropertyInfo(
            propertyInfo: ReflectionHelper.GetPropertyOf<RoleDTO>(x => x.Description),
            displayName: "Description",
            htmlElementTypeInCreationAndEdit: HtmlElementType.TextArea);

        SetConfigurationsForBaseObjectMembers(); // AddedDate, ModifiedDate, IsActive, Description
        return this;
    }
}
```

`PermissionDTO` biraz daha zengin — kendi `Type` (enum), `Controller`,
`Action`, `Entity`, `AllowAnonymous`, `Enabled` alanlarını tanımlıyor ve her
biri için doğru yardımcıyı çağırıyor (`SetConfigurationsViaEnumPropertyInfo`
enum için, `SetConfigurationsViaBooleanPropertyInfo` boolean için). Sonuç: bu
alanlar Listede filtrelenebilir, Formda doğru input tipiyle (`Type` için
dropdown, `Enabled`/`AllowAnonymous` için switch) çizilir — **hiçbir Razor
kodu yazılmadan**, sadece bu tek dosyadaki bildirimle.

## `[RegisterAsViewDTO]` ve önyükleme

`RegisterAsViewDTO` attribute'u, `ViewDTORegistryLoader.LoadAllViewDTOs`
tarafından uygulama açılışında taranıp `ViewDTOConfigurationCacheManager`'a
önceden yüklenebiliyor — yani her istek yeniden `Activator.CreateInstance` +
`LoadConfigurations()` çalıştırmıyor, ilk çözümden sonra sonuç cache'leniyor
(`ConcurrentDictionary<(dtoName, tableName, viewType), ConfigurationCacheGroup>`).
`ViewDTOTypeResolver`'ın kendi cache'i (isim → `Type`) ile bu cache ayrı
katmanlar: biri "hangi sınıf", diğeri "o sınıfın hangi view için hangi
konfigürasyonu var".

## Controller katmanı: üçü de aynı kalıp, ikisi ekstra endpoint ekliyor

`RolePermissionController`, `RoleSystemUserController`,
`SystemUserPermissionController` — üç köprü tablonun controller'ı da **tek
satır**, hiçbir özel endpoint yok:

```csharp
// Hydra.WebApi/Controllers/RolePermissionController.cs
[ApiController]
[Route("api/[controller]")]
public class RolePermissionController : MainController<RolePermission>
{
    public RolePermissionController(IControllerInjector injector) : base(injector) { }
}
```

`RoleController` ve `PermissionController` ise `MainController<T>`'ın
verdiği Create/Update/Delete/Select/Details'e ek olarak, bir önceki bölümde
gördüğümüz servis metodlarını dışarı açan birer ince sarmalayıcı ekliyor:

```csharp
// Hydra.WebApi/Controllers/RoleController.cs
[HttpGet("GetUsers/{roleId}")]
public async Task<JsonResult> GetUsers(Guid roleId)
    => new JsonResult(await RoleService.GetUsersResponseAsync(roleId));

[HttpGet("GetPermissions/{roleId}")]
public async Task<JsonResult> GetPermissions(Guid roleId)
    => new JsonResult(await RoleService.GetPermissionsResponseAsync(roleId));
```

Bu iki endpoint bugün **hiçbir istemci tarafından çağrılmıyor** — RCL
tarafında `RoleClient`/`PermissionClient` gibi entity-özel bir `ApiClient<T>`
alt sınıfı yok (bkz. [1.3](../01-Architecture/03-hydra-razorclasslibrary.md)'te
`CustomFileClient` örneğiyle anlatılan desen). Bu, [5.3](03-dashboard-pages-and-collection-views.md)'te
göreceğimiz "CollectionView bağlı değil" boşluğunun API tarafındaki ikizi:
sunucu tarafı hazır, istemci tarafı henüz onu kullanmıyor.

## Dosya haritası

| Sorumluluk | Yol |
|---|---|
| Konvansiyon çözücü | `Hydra/DTOs/ViewDTOs/ViewDTOTypeResolver.cs` |
| Konfigürasyon cache'i | `Hydra/DTOs/ViewDTOs/ViewDTOConfigurationCacheManager.cs`, `ViewDTORegisteryLoader.cs` |
| `SystemUser` ViewDTO (core) | `Hydra/DTOs/ModelDTOs/SystemUserDTO/SystemUserViewDTO.cs` |
| `Role`/`Permission` ViewDTO (uygulama) | `Tentacle/Source/HydraTentacle.Core/DTOs/RoleDTO.cs`, `PermissionDTO.cs` |
| Controller'lar | `Hydra.WebApi/Controllers/{SystemUser,Role,Permission,RolePermission,RoleSystemUser,SystemUserPermission}Controller.cs` |
