# 5.8 — Beş Katmanı Tamamlamak ve İzin Verisini Yönetilebilir Kılmak

[5.7](07-permission-enforcement-and-status-codes.md) 1. katmanı (uç güvenliği)
çalışır hâle getirdi ve zinciri uçtan uca bağladı. Ama ortaya şu çıktı: **sistem
çalışıyordu, sadece kullanılamıyordu.** Deny-by-default açıktı, admin `*` ile her
şeyi yapabiliyordu, ve admin dışında kimsenin var olmasının bir yolu yoktu.

Bu bölüm o boşluğu kapatıyor: kalan dört katmanı uyguluyor ve — asıl mesele —
izin verisini ekrandan yönetilebilir hâle getiriyor.

## Katman durumu

| Katman | Tip | Durum |
|---|---|---|
| 1 | `ControllerActionBased` | Tam — `PermissionAuthorizationFilter` ([5.7](07-permission-enforcement-and-status-codes.md)) |
| 2 | `NavMenuBased` | Tam — menü `hydra:endpoint` claim'ine göre çiziliyor |
| 3 | `ViewBased` | Tam — `HydraRouteGuard` + 48 sayfaya `[HydraRequirePermission]` |
| 4 | `ComponentBased` | Tam — `GenericListView`'da New/Edit/Delete izne bağlı |
| 5 | `EntityPropertyBased` | Tam — `PropertySecurity` kolonu meta-veriden **ve** değerden siliyor |

## Katman 3'ün sessiz açığı

5.7'de `HydraRouteGuard` her rotayı kapatmıştı — ama sadece "giriş yapmış mı"
diye bakıyordu. `[HydraRequirePermission]` attribute'unu yazmıştım ve **hiçbir
sayfaya koymamıştım.**

Sonucu şuydu: menüde Users linkini görmeyen bir kullanıcı, adres çubuğuna
`/SystemUser` yazınca sayfayı açabiliyordu. Liste boş gelirdi (API 403 döner),
ama ekran açılırdı — yani menüyü gizlemek tek başına 3. katman değil,
sadece nezaketti.

Artık 48 CRUD sayfasının hepsi attribute taşıyor:

```razor
@page "/Role"
@attribute [HydraRequirePermission("Role", "Select")]
```

Eşleme, sunucudaki action adlarıyla birebir: `Index → Select`,
`Details → Details`, `Create → Create`, `Edit → Update`. Böylece aynı izin hem
sayfayı hem arkasındaki ucu açıyor; ikisi ayrı ayrı yönetilmiyor.

## Katman 5: neden opt-in, katman 1 deny-by-default iken

Bu, bölümdeki tek asimetrik karar — ve bilinçli.

Uçlar **sayılabilir** bir küme: on beş controller, her biri sekiz action. Hepsini
varsayılan olarak kapatmak ve tek tek açmak yönetilebilir. Property'ler değil:
her entity'nin her kolonu için izin satırı yazmadan hiç kimse hiçbir şey
göremezdi. Deny-by-default orada sistemi kullanılmaz yapardı.

Bu yüzden kural şu: **bir kolon, ancak onu adıyla işaretleyen bir
`EntityPropertyBased` izin satırı varsa korumalıdır.** Satır yoksa kolon açıktır.

```
Name     : Employee.Salary
Type     : EntityPropertyBased
Entity   : Employee
Property : Salary
```

Bu satır kaydedildiği an `Salary` kolonu herkesten gizlenir; izni bağlananlar
görür. Tek bir kayıt hem "bu alan hassas" der hem "bu izne sahip olan görür" —
iki ayrı kavram tanımlamaya gerek kalmaz.

**Değer de siliniyor, sadece kolon tanımı değil.** Kolonu UI'da gizleyip değerini
JSON'da göndermek bir kostümdür; veri, tarayıcının ağ sekmesi kadar uzaktadır.
`PropertySecurity.Apply` hem `TableDTO.MetaColumns`'tan hem her satırın
değerlerinden siler; `Redact` ise `Details`/`Get` uçlarının döndürdüğü entity
nesnesini temizler — aksi hâlde aynı cevabın öbür yarısında aynı değer dururdu.

Ayrıca izinden bağımsız, **koşulsuz gizli** iki alan var:
`SystemUser.PasswordHash` ve `PasswordResetValidationToken`. Bir parola
hash'inin tarayıcıya gitmesi için hiçbir meşru sebep yok: görüntülenmiyor,
düzenlenmiyor (form yeni parolayı aynı alanın üzerine yazıyor), ve bir
saldırgan için tek kullanımı çevrimdışı kırma denemesi.

## İzin verisini yönetmek: asıl tıkanan yer

Katmanlar çalışıyordu ama sistemde tek kullanılabilir hesap admin'di. Sebep
üçlüydü:

**1. UI'dan açılan kullanıcı giriş yapamıyordu.** Kullanıcı formu şifreyi
hash'lemiyordu, `PasswordHash` boş kalıyordu. `SystemUserController.Create` ve
`Update` artık override edilmiş: gelen değer zaten hash'liyse dokunulmuyor, düz
metinse hash'leniyor, boşsa mevcut hash korunuyor (form hash'i hiç almadığı için
alan zaten boş gelir).

**2. Uçlar için izin kaydı yoktu.** Deny-by-default'ta bir ucu role vermek, önce
o ucu tarif eden `Permission` satırının var olmasına bağlı. On beş controller ×
sekiz action'ı elle açmak sürdürülebilir değil — üstelik elle yazılan bir
`Action` değeri route'tan bir harf saparsa izin sessizce hiçbir şeyi açmaz.

`EndpointPermissionScanner` her açılışta controller'ları gezip eksik izinleri
oluşturuyor. İsimler route'un kendisinden okunduğu için `PermissionEvaluator`'ın
karşılaştırdığı değerlerle birebir aynı. Üç ince nokta:

- **`Async` soneki kırpılıyor** — ASP.NET route'a `LoginAsync` değil `Login`
  yazar; kırpılmasaydı üretilen izin hiçbir isteğe eşleşmezdi.
- **Miras alınan action'lar da taranıyor** — `DeclaredOnly` kullanılsaydı
  `MainController<T>`'den gelen generic CRUD uçlarının hiçbiri keşfedilmezdi.
- **`[AllowAnonymous]` olanlar atlanıyor** — Login ve ClientLog için izin kaydı
  üretmek anlamsız.

Idempotent: `Controller/Action` anahtarıyla karşılaştırıyor (Name'e bakmıyor,
çünkü elle değiştirilmiş olabilir), sadece eksikleri ekliyor.

**3. İzni role bağlayacak ekran yoktu.** İzin sistemi üç parçadan oluşur:
`Permission` (kilit), `Role`/`SystemUser` (kişiler), ve **köprü** (anahtarı
vermek). İlk ikisinin ekranı vardı, üçüncüsünün yoktu — yani izin sistemi tek
yönlü çalışıyordu: kısıtlayabiliyor, yetkilendiremiyordun.

Artık `Role` detayında `Permissions` + `Users`, `Permission` detayında `Roles` +
`Users`, `SystemUser` detayında `Roles` + `Permissions` sekmeleri var. Sekme
mekanizması Hydra'da zaten vardı: `GenericDetailsView`, `IDetailsTabsHost`
üzerinden içine konan her `CollectionViewSection`'ı sekmeye çeviriyor.

## Link / Unlink: GedenLines'tan ne alındı, ne düzeltildi

Köprü kaydını tek tek form doldurarak açmak, on izin bağlarken on kez form
demekti. GedenLines'ın çözümü daha iyiydi: **Link** butonu bir lookup modalı
açıyor, checkbox'la çoklu seçim yapılıyor, tek istekte bağlanıyor.

O kalıp Hydra'ya taşındı — dört noktada düzeltilerek:

| GedenLines'ta | Hydra'da |
|---|---|
| Duplicate kontrolü yok; aynı çift iki kez bağlanırsa mükerrer satır ya da unique index hatası | Zaten bağlı olanlar sunucuda atlanıyor: *"3 linked, 2 were already linked"* |
| "Hepsini seç" bozuk — `CheckedIdList` hesaplanan bir property, `Clear()`/`Add()` geçici listeyi değiştiriyor, etkisi yok | Seçim ayrı bir `HashSet`'te; arama yazıp listeyi daraltmak önceki işaretleri silmiyor |
| Unlink için istemci önce "köprüde bu Id'ler hangi satırlar" diye ekstra bir `Select` atıyor | Sağ tarafın Id'si gönderiliyor, köprü satırını sunucu kendisi buluyor — fazladan tur yok |
| Link sonrası `NavigateTo(forceLoad: true)` — Blazor Server'da SignalR circuit'i koparıp sayfayı baştan kurar | `CollectionViewSection.ReloadAsync()` — sadece o sekme tazeleniyor, sekme durumu korunuyor |
| Her yeni M2M ilişki için üç lookup view'daki `switch`'e case eklemek gerekiyor | Köprü tablo adı istekle geldiği için tek generic uç hepsini karşılıyor |

Sayfada tek yapılan şey sağ tarafı tarif etmek:

```razor
<CollectionViewSection Title="Permissions" Controller="RolePermission"
                       ForeignKeyName="RoleId" ParentId="@Id"
                       RightKeyName="PermissionId" RightTableName="Permission"
                       RightDisplayColumn="Name" />
```

`RightKeyName` verilmezse bölüm eskisi gibi bire-çok davranıyor — mevcut
kullanımlar (Kategori → Talepler) hiç etkilenmedi.

Sunucu tarafında `CollectionLinkService`, köprü tipini adından çözüp
(`EntityTypeResolver`) normal `Service<T>` üzerinden yazıyor — doğrudan
`DbContext`'e inilmiyor, ki audit/log/cache davranışı elle açılan kayıtlarla
birebir aynı kalsın.

## Uçtan uca akış

1. Uygulama açılır → tarayıcı tüm uçları `Permission` olarak yazar
2. **Roles** → yeni rol ("Operator")
3. Rolün **Permissions** sekmesi → **Link** → modalda uçları işaretle → tek seferde bağla
4. Rolün **Users** sekmesi → kullanıcıyı bağla
5. O kullanıcı girdiğinde: menüde yalnızca izinli modüller, gridlerde yalnızca
   izinli butonlar, URL'den zorlanan sayfalarda `/status/403`

## Dürüst uyarılar

- **Bu bölümdeki hiçbir kod çalıştırılmadı.** NuGet ortamdan bloklu olduğu için
  derleme yapılamadı; yalnızca sözdizimi/imza taraması yapıldı. En kırılgan yer
  `CollectionLinkService`'teki reflection: `IService<T>`'yi çalışma zamanında
  açıp `CreateAsync`/`DeleteRangeAsync`/`SelectThenCache` çağırıyor.
- **Geçici admin bypass'ı hâlâ duruyor.** `LoginAsync` içinde
  `admin@tentacle.com` için şifresiz giriş var — yani sistem şu an fiilen
  herkese açık. UI'dan admin'e bir şifre set edildikten sonra silinmeli.
- **Katman 5 admin ile test edilemez.** Admin `*` taşıdığı için
  `GetDeniedPropertiesAsync` ilk kontrolde boş döner; farkı görmek için ikinci
  bir kullanıcı gerekiyor.
- **İzin değerlendirmesinde önbellek yok.** Alt servislerin LRU cache'leri var
  ama rol→izin zinciri her istekte yeniden yürünüyor.

## Dosya haritası

| Sorumluluk | Yol |
|---|---|
| Kolon/alan güvenliği | `Hydra/AccessManagement/Authorization/PropertySecurity.cs` |
| Denied property hesabı | `Hydra/AccessManagement/Authorization/PermissionEvaluator.cs` (`GetDeniedPropertiesAsync`) |
| Okuma uçlarında uygulama | `Hydra.WebApi/Controllers/Base/MainController.cs` (`ApplyPropertySecurityAsync`) |
| Köprü bağlama servisi | `Hydra/Services/CollectionLinkService.cs`, `Hydra/DTOs/CollectionLinkDTO.cs` |
| Entity tipi çözücü | `Hydra/Utils/EntityTypeResolver.cs` |
| Uç tarayıcı | `Hydra.WebApi/Services/EndpointPermissionScanner.cs`, `Extensions/PermissionSyncExtensions.cs` |
| Çoklu seçim modalı | `Hydra.RazorClassLibrary/.../Components/MultiSelectLookupModal.razor` |
| Cross-table collection | `Hydra.RazorClassLibrary/.../Components/CRUD/CollectionViewSection.razor` |
| Buton izinleri | `Hydra.RazorClassLibrary/.../Components/CRUD/GenericListView.razor` |
| Sayfa izinleri | `Tentacle/Source/HydraTentacle.Blazor/Pages/Crud/**/*.razor` |
| Köprü ViewDTO'ları | `Tentacle/Source/HydraTentacle.Core/DTOs/{RolePermission,RoleSystemUser,SystemUserPermission}DTO.cs` |
| Şifre hash'leme | `Hydra.WebApi/Controllers/SystemUserController.cs`, `Hydra/Services/PasswordHasher.cs` |

## ⭐ İleride Yapılacaklar

- Admin bypass'ını sil (önce UI'dan admin şifresi set edilmeli).
- İzin değerlendirmesi için kullanıcı bazlı kısa ömürlü cache.
- Dashboard widget'ları hâlâ koşulsuz çiziliyor — 2. katmanın son parçası.
- `GenericDetailsView` / `GenericFormView` üzerindeki Edit/Delete butonları da
  izne bağlanmalı; şu an sadece `GenericListView` kontrollü.
- Tarayıcı yalnızca `ControllerActionBased` üretiyor. `ViewBased`,
  `NavMenuBased`, `ComponentBased` izinleri hâlâ elle açılıyor — onlar için de
  keşif (route tablosu, menü tanımları) düşünülebilir.
