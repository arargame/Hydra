# 1.2 — Hydra.WebApi: Genel REST Katmanı

**Proje:** `Hydra.WebApi` (ASP.NET Core Web API, `Hydra.sln`'nin bir parçası)
**Rolü:** Hydra Core'daki `Service<T>`'i HTTP üzerinden dışarı açan, tek bir generic
controller'la 20+ entity'yi aynı anda servis eden ince bir katman.

## Neden "generic controller"?

Klasik yaklaşımda her entity için ayrı bir `ProductController : ControllerBase`
yazılır, her biri kendi Create/Update/Delete/Select metodunu tekrar tekrar
implemente eder. Hydra bunun yerine tek bir soyut sınıfa yazıyor,
`MainController<T>`, ve her entity için sadece **boş bir alt sınıf** açıyor:

```csharp
// Hydra.WebApi/Controllers/PermissionController.cs — muhtemelen bundan ibaret:
public class PermissionController : MainController<Permission>
{
    public PermissionController(IControllerInjector injector) : base(injector) { }
}
```

Yeni bir entity için REST API'nin bedeli bu kadar — bir dosya, birkaç satır. Bunun
neden mümkün olduğunu (yani `MainController<T>`'in ne yaptığını) açalım.

## `MainController<T>` — tek dosyada CRUD + Select

`Hydra.WebApi/Controllers/Base/MainController.cs`. Kurucusu, `T` için doğru
`IService<T>`'i reflection ile bulup DI container'dan çekiyor:

```csharp
protected MainController(IControllerInjector injector)
{
    var serviceType = typeof(IService<>).MakeGenericType(typeof(T));
    Service = _serviceFactory.GetService(serviceType) as IService<T>
        ?? throw new InvalidOperationException($"Service for {typeof(T).Name} not found");
}
```

Sonra klasik REST fiilleri, hepsi `Service`'e devrediyor:

| Route | Fiil | Ne yapar |
|---|---|---|
| `POST /{Entity}/Create` | `Service.CreateAsync` | Yeni kayıt |
| `PUT /{Entity}/Update` | `Service.UpdateAsync` | Güncelleme |
| `DELETE /{Entity}/Delete/{id}` | `Service.DeleteAsync` | Tekil silme |
| `POST /{Entity}/DeleteBulk` | `Service.DeleteRangeAsync` | Toplu silme |
| `GET /{Entity}/Get/{id}` | `Service.GetByIdAsync` | Tekil kayıt (include'larla) |
| `GET /{Entity}/Details/{id}` | `Service.GetDetailsAsync<T>` | `DetailsView` konfigürasyonuna göre hazırlanmış `TableDTO` + entity |
| `POST /{Entity}/Select` | `Service.SelectWithTableAsync<T>` | Liste/koleksiyon/filtre/sayfalama — bkz. [2.1](../02-Core-Systems/01-table-dto-and-statelessness.md) |
| `POST /{Entity}/Seed/{count}` *(yalnız DEBUG)* | `Service.SeedAsync` | Rastgele örnek veri |

`Select` en ilginç olanı, çünkü tek bir endpoint dört farklı view type'ı
(`ListView`/`CollectionView`/`CreateView`/`EditView`/`LookupView`) aynı sözleşmeyle
karşılıyor — hangi view olduğu gövdedeki `TableDTO.ViewType` alanından ya da
`?viewType=` query string'inden geliyor:

```csharp
[HttpPost, Route("Select")]
public virtual async Task<JsonResult> Select([FromBody] TableDTO? tableDTO = null,
                                              [FromQuery] ViewType? viewType = ViewType.ListView)
{
    tableDTO ??= new TableDTO();
    if (string.IsNullOrEmpty(tableDTO.Name))
        tableDTO.Name = typeof(T).Name;

    var viewDTOType = ResolveViewDTOType(tableDTO.ViewDTOTypeName);
    var (finalDTO, results) = await Service.SelectWithTableAsync<T>(tableDTO, viewType, viewDTOType);

    // Not: TableDTO doğrudan Data olarak dönüyor — { Table = ... } gibi bir
    // sarmalayıcı YOK, çünkü Blazor tarafındaki ApiClient.SelectAsync
    // ResponseObject.Data'yı doğrudan TableDTO'ya deserialize ediyor.
    return new JsonResult(response.SetSuccess(results?.Any() ?? false).SetData(finalDTO));
}
```

`ResolveViewDTOType` bir **konvansiyon** kullanıyor: `T`'nin adına göre
`"{T}DTO"` veya `"{T}ViewDTO"` isimli bir tip arıyor (`ViewDTOTypeResolver.Resolve`).
Yani `Product` için `ProductDTO` sınıfını otomatik buluyor — DTO tipini elle
bağlamanıza gerek yok, isimlendirmeye uyduğunuz sürece.

## DI: attribute ile taranan, elle kaydedilmeyen servisler

`Hydra.WebApi/Extensions/ServiceCollectionExtensions.cs`, `AddHydraDependencies`
altında. Elle `services.AddScoped<IService<Product>, ProductService>()` yazmak
yerine, assembly'ler reflection ile taranıyor:

```csharp
public static void AddCustomServicesByAttribute<TAttribute>(IServiceCollection services, params Assembly[] assemblies)
{
    var candidates = assemblies.SelectMany(a => a.GetTypes())
                                .Where(t => t.IsClass && !t.IsAbstract)
                                .Where(t => t.GetCustomAttribute<TAttribute>() != null);

    foreach (var implType in candidates)
    {
        var interfaceType = ((RegisterAsServiceAttribute)implType.GetCustomAttribute<TAttribute>())
                                .ServiceInterface ?? implType.GetInterfaces().FirstOrDefault();

        if (services.Any(s => s.ServiceType == interfaceType))
            throw new InvalidOperationException($"Duplicate service registration for {interfaceType.Name}");

        services.AddScoped(interfaceType, implType);
    }
}
```

Bu yüzden Tentacle'daki `RequestAttachmentService` şöyle görünüyor —
`ServiceCollectionExtensions.cs`'e hiç dokunmadan otomatik kaydolur:

```csharp
[RegisterAsService(typeof(IService<RequestAttachment>))]
public class RequestAttachmentService : Service<RequestAttachment>
{
    public RequestAttachmentService(ServiceInjector injector) : base(injector) { }
}
```

Kayıt kendini tekrarlarsa (`services.Any(s => s.ServiceType == interfaceType)`)
**başlangıçta** (uygulama açılırken) exception fırlatıyor — yani iki servis aynı
arayüzü kaydetmeye çalışırsa hata runtime'ın ortasında bir çağrıda değil,
`Program.cs` açılırken patlıyor. Erken başarısızlık, doğru tercih.

Aynı tarama deseni repository katmanı için de var
(`AddCustomServicesByAttribute<RegisterAsRepositoryAttribute>`), ve `Service<>`/
`Repository<>` açık generic'leri de ayrıca `AddScoped(typeof(IService<>), typeof(Service<>))`
ile kayıtlı — yani `[RegisterAsService]` **atlanan** entity'ler için bile
`IService<Product>` çözülebiliyor, generic fallback her zaman orada.

## `ViewDTORegistryLoader` — DTO'ların önceden yüklenmesi

```csharp
ViewDTORegistryLoader.LoadAllViewDTOs(typeof(ViewDTO).Assembly, Assembly.GetExecutingAssembly());
```

`ViewDTO.LoadConfigurations()` (bkz. [1.1 — Hydra Core §5](01-hydra-core.md#5-viewdto--ekrana-göre-kolon-sözleşmesi))
her istek için tekrar tekrar reflection yapmasın diye, uygulama açılırken bir kere
taranıp cache'leniyor (`ViewDTOConfigurationCacheManager`). Bunun pratik sonucu:
yeni bir `XDTO` eklediğinizde uygulamayı yeniden başlatmadan görünmez.

## Bağlantı yönlendirmesi de burada kayıtlı

`TableService`'in per-table veritabanı yönlendirmesi (bkz.
[`Hydra/Docs/database-routing.md`](../../Docs/database-routing.md)) DI kaydında
kuruluyor — `ServiceCollectionExtensions.cs` içinde `connectionNameForTable`
delegate'i `appsettings.json`'daki `Hydra:TableConnections` bölümünü okuyor.
WebApi katmanının burada tek görevi "hangi config'ten hangi connection factory'nin
besleneceğini" bağlamak; asıl yönlendirme mantığı Hydra Core'da.

## Dosya haritası

| Sorumluluk | Yol |
|---|---|
| Generic CRUD + Select | `Hydra.WebApi/Controllers/Base/MainController.cs` |
| Somut controller'lar | `Hydra.WebApi/Controllers/*.cs` (her biri birkaç satır) |
| DI kayıtları, attribute tarama | `Hydra.WebApi/Extensions/ServiceCollectionExtensions.cs` |

---

## ⭐ İleride Yapılacaklar / Not Edilenler

- `MainController<T>.Select` `virtual` — yani bir entity'nin özel bir Select
  davranışı gerekiyorsa (ör. yetkiye göre satır filtreleme) override edilebilir,
  ama bugün ekosistemde bunun hiç kullanıldığı bir örnek yok. Bölüm 4'te
  `Product` üzerinde bir override örneği denenebilir.
- `AddCustomServicesByAttribute`'daki "duplicate service" kontrolü sadece
  **aynı interface**'i yakalıyor; iki farklı implementasyon farklı interface'lerle
  kaydolursa (kasıtsız da olsa) sessizce ikisi de kayıtlı kalır. Küçük bir risk,
  not edildi.
- `Details` endpoint'i `dataPackage` için anonim tip (`new { Table, Item }`)
  kullanıyor — `Select`'in aksine burada bir sarmalayıcı var. `ApiClient.GetDetailsViewAsync`
  bunu özel bir `DetailsContainer` sınıfıyla karşılıyor; iki endpoint'in farklı
  zarflama biçimi tutarsız ama şu an ikisi de doğru çalışıyor. Uzun vadede
  `Select`'in zarflama biçimine (doğrudan `TableDTO`) hizalanabilir.
