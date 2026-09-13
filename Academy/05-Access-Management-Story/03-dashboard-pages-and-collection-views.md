# 5.3 — Dashboard'daki Sayfalar: List, Details, Edit, Collection

Tentacle'da `SystemUser`, `Role`, `Permission` için toplam 12 `.razor` dosyası
var (`Pages/Crud/{SystemUser,Role,Permission}/{Index,Details,Update,Create}.razor`)
— ve her biri gerçekten sadece birkaç satır. Bu bölümün amacı, o birkaç
satırın arkasında ne olduğunu göstermek, ve bugün eksik olan tek parçayı
(collection view bağlantısı) somut bir tarifle kapatmak.

## Index → `GenericListView`

```razor
@* Pages/Crud/Role/Index.razor *@
@page "/Role"
<GenericListView T="Role" Client="Client" Title="Roles" />

@code {
    [Inject] public ApiClient<Role> Client { get; set; } = default!;
}
```

Bu üç satır, [1.3](../01-Architecture/03-hydra-razorclasslibrary.md)'te
anlatılan `GenericListView<T>`'i çalıştırıyor:

1. `OnInitializedAsync`'te `Client.SelectAsync(new TableDTO { Name = "Role", ViewType = ListView })`
   çağrılıyor → sunucuda `RoleController.Select` (aslında `MainController<Role>.Select`,
   `Role`'e özel bir override yok) → `ViewDTOTypeResolver.Resolve(null, "Role")`
   → `RoleDTO` bulunuyor → `RoleDTO.LoadConfigurations()`'taki `Name`/`Description`
   `ListViewConfiguration`'ları kolonlara dönüşüyor.
2. Dönen `TableDTO`, `HydraGrid`'e veriliyor — sayfalama, sıralama, filtre barı
   (`ShowFilters` açıksa `FilterBarComponent`) hepsi bu tek `TableDTO`'nun
   üzerinde çalışıyor (bkz. [2.1 — TableDTO ve Statelessness](../02-Core-Systems/01-table-dto-and-statelessness.md)).
3. Satır aksiyonları (`Edit`/`Delete` ikonları) ve üstteki "New Record"
   butonu `BasePath` (`Client.Controller`, yani `"Role"`) üzerinden
   `/Role/Update/{id}`, `/Role/Create` rotalarına yönleniyor.

`Permission` ve `SystemUser`'ın `Index.razor`'ı birebir aynı kalıp, sadece
`T` ve `Title` değişiyor. `SystemUser` için tek fark: `RoleDTO`/`PermissionDTO`
Tentacle'da yaşarken, `SystemUserViewDTO` Hydra core'da yaşıyor (bkz.
[5.2](02-viewdto-service-controller-layer.md)) — ama `Index.razor`'dan bakınca
bu fark **görünmüyor bile**, `GenericListView<SystemUser>` ikisini de aynı
şekilde tüketiyor.

## Create / Update → `GenericFormView` (tek motor, iki mod)

```razor
@* Pages/Crud/Role/Create.razor *@
<GenericCreateView T="Role" Client="Client" Title="New Role" />

@* Pages/Crud/Role/Edit.razor *@
<GenericEditView T="Role" Client="Client" Id="@Id" Title="Edit Role" />
```

İkisi de aynı `GenericFormView<T>`'in ince sarmalayıcısı — `Id` verilirse
Edit, verilmezse Create modu. Form alanları `Table.MetaColumns`'tan
(`CreateViewConfiguration`/`EditViewConfiguration` taşıyan kolonlar) üretiliyor,
her alan `ColumnFieldComponent`'e (Katman-2 dağıtıcı: `HtmlElementType`'a göre
input/textarea/select/checkbox çizer) devrediliyor. `PermissionDTO.Type` enum
alanı burada somutlaşıyor: `SetConfigurationsViaEnumPropertyInfo` sayesinde
`GenericFormView` bu alan için otomatik bir dropdown üretiyor, seçenekler
`LookupService.GetEnumOptions(field.PropertyTypeName)` ile enum değerlerinden
geliyor — hiçbir yerde elle "ViewBased, ControllerActionBased, ..." listesi
yazılmıyor.

## Details → `GenericDetailsView` + (eksik) `CollectionViewSection`

```razor
@* Pages/Crud/Role/Details.razor — BUGÜNKÜ HALİ *@
<GenericDetailsView T="Role" Client="Client" Id="@Id" Title="Role Details" />
```

`GenericDetailsView<T>` üç bölümden oluşuyor (bkz. [1.3](../01-Architecture/03-hydra-razorclasslibrary.md)):
başlık kartı, `DetailsViewConfiguration` taşıyan alanların özet kartı, ve
sekmeli bir koleksiyon kartı. Üçüncüsü `ChildContent` olarak verilen
`CollectionViewSection`'lardan besleniyor — [1.4](../01-Architecture/04-tentacle-reference-app.md)'te
gösterilen `Position/Details.razor` örneği tam da bunu yapıyor:

```razor
<GenericDetailsView T="Position" Client="Client" Id="@Id" Title="Position Details">
    <CollectionViewSection Title="Employees" Controller="Employee" ForeignKeyName="PositionId" ParentId="@Id" />
</GenericDetailsView>
```

**Bugün `Role/Details.razor` ve `Permission/Details.razor`'ın `ChildContent`'i
yok** — yani Role'ün hangi kullanıcılara atandığını, hangi izinleri taşıdığını
görmek için Details ekranından hiçbir yol yok, oysa tam bunun için yazılmış
`RoleController.GetUsers`/`GetPermissions` endpoint'leri zaten sunucuda duruyor
(bkz. [5.2](02-viewdto-service-controller-layer.md)). Otomatik "Logs" sekmesi
(her `GenericDetailsView`'e `EntityType`/`EntityId` üzerinden select ile
eklenen, FK gerektirmeyen sekme) hâlâ çalışıyor — eksik olan yalnızca
uygulamaya özel sekmeler.

### Bunu eklemenin önündeki gerçek engel: çoka-çoğa

`CollectionViewSection`, tek bir eşitlik filtresi (`ForeignKeyName = ParentId`)
kuruyor — `Position/Details`'teki `Employee.PositionId == Id` gibi **doğrudan
bir FK** varsayıyor. Ama `Role` ile `Permission` arasında doğrudan FK yok,
aradaki `RolePermission` köprü tablosu var. Yani teknik olarak eklenebilecek
tek şey:

```razor
<GenericDetailsView T="Role" Client="Client" Id="@Id" Title="Role Details">
    <CollectionViewSection Title="Users" Controller="RoleSystemUser" ForeignKeyName="RoleId" ParentId="@Id" />
    <CollectionViewSection Title="Permissions" Controller="RolePermission" ForeignKeyName="RoleId" ParentId="@Id" />
</GenericDetailsView>
```

Bu **çalışır** (`RoleSystemUserController`/`RolePermissionController` zaten
`MainController<T>.Select`'i miras alıyor), ama gösterdiği tablo `Permission`
değil, **köprü satırı** — yani kolonlar `PermissionId` (çıplak Guid), `RoleId`,
`AddedDate` olur, "Admin Panel Access" gibi okunabilir bir isim değil. Bunun
sebebi [5.2](02-viewdto-service-controller-layer.md)'de gördüğümüz şey:
`RolePermission`'ın kendi `RolePermissionDTO`'su yok, dolayısıyla
`ViewDTOTypeResolver` onun için bir konfigürasyon bulamıyor ve ham kolonlar
görünüyor.

Düzgün bir sonuç için iki yoldan biri gerekir:

1. **`RolePermissionDTO` yaz**, `ViewDTO.SetConfigurationsForNavigations`
   kullanarak `PermissionId`'yi `Permission.Name`'e flatten et — tam olarak
   `RequestDTO`'nun `CreatedByEmployee` için yaptığı desen
   (`Documentation/Hydra/Worklog-2026-07-19.md §4`'te anlatılıyor).
2. **`CollectionViewSection`'ı `RolePermissionService.GetPermissionsResponseAsync`
   gibi özel bir endpoint'e bağlayacak şekilde genişlet** — bugünkü component
   yalnızca generic `Controller/Select`'e POST atıyor, özel bir aksiyona değil.

İkisi de bu kitabın kapsamında **henüz yapılmadı** — bilinçli olarak: hangi
yönün seçileceği (genel flatten deseni mi, özel "ilişki endpoint'i" deseni mi)
Hydra'nın çoka-çoğa ilişkileri nasıl standartlaştıracağına dair ayrı bir karar
gerektiriyor, tek bir entity için çözülüp unutulacak bir şey değil.

## Dosya haritası

| Sorumluluk | Yol |
|---|---|
| CRUD sayfaları | `Tentacle/Source/HydraTentacle.Blazor/Pages/Crud/{SystemUser,Role,Permission}/*.razor` |
| Generic view motorları | `Hydra.RazorClassLibrary/.../Components/CRUD/Generic{ListView,FormView,CreateView,EditView,DetailsView}.razor` |
| Koleksiyon sekmesi | `Hydra.RazorClassLibrary/.../Components/CRUD/CollectionViewSection.razor` |
| Örnek (çalışan) collection view kullanımı | `Tentacle/Source/HydraTentacle.Blazor/Pages/Crud/Position/Details.razor` |
