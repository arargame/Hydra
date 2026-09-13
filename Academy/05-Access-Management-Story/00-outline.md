# Bölüm 5 — Access Management Story

Bu bölüm, [1.4](../01-Architecture/04-tentacle-reference-app.md) ve
[1.3](../01-Architecture/03-hydra-razorclasslibrary.md)'ün ⭐ notlarında ertelenen
sözü tutuyor: **tek bir örnek üzerinden, Hydra core'daki bir entity tanımından
Tentacle'daki gerçek bir Dashboard ekranına kadar bütün zinciri, tek bir
nefeste anlatmak.** Örnek olarak elimizdeki en zengin üçlüyü seçtik —
`SystemUser`, `Role`, `Permission` — çünkü aralarında bire-çok, çoka-çok, hatta
kendi kendine referans (bir kullanıcının hem doğrudan hem rol üzerinden izni
olması) gibi bu kitapta başka yerde tam gösterilmemiş ilişki tipleri var.

**Neden ayrı bir bölüm, [Bölüm 1](../01-Architecture/)'e eklenmedi?**
Bölüm 1 katman katman ("WebApi ne yapar", "RCL ne yapar") anlatıyor — yatay bir
kesit. Bu bölüm ise **dikey**: tek bir özelliğin (yetkilendirme) her katmandan
nasıl geçtiğini uçtan uca izliyor, [2.3 — A Journey of a Value](../02-Core-Systems/03-journey-of-a-value.md)'un
yaptığı gibi ama bir SQL sorgusu yerine bütün bir CRUD özelliği ölçeğinde.

## Bölüm planı

| # | Doküman | İçerik |
|---|---|---|
| 5.1 | [01-core-entities-and-relationships.md](01-core-entities-and-relationships.md) | `SystemUser`/`Role`/`Permission` ve aralarındaki üç köprü tablo (`RoleSystemUser`, `RolePermission`, `SystemUserPermission`); neden doğrudan çoka-çoğa değil de ayrı köprü entity'leri var |
| 5.2 | [02-viewdto-service-controller-layer.md](02-viewdto-service-controller-layer.md) | `ViewDTOTypeResolver` konvansiyonu, `Configuration`/`ListViewConfiguration`/vb. sistemi, entity'nin Hydra core'da, ekran konfigürasyonunun tüketen uygulamada (Tentacle) yaşaması |
| 5.3 | [03-dashboard-pages-and-collection-views.md](03-dashboard-pages-and-collection-views.md) | Tentacle'daki üç sayfa dörtlüsü (Index/Create/Update/Details), `GenericListView`/`GenericFormView`/`GenericDetailsView`, ve bugün eksik olan `CollectionViewSection` bağlantısı |
| 5.4 | [04-login-to-dashboard-and-gaps.md](04-login-to-dashboard-and-gaps.md) | `SystemUser`'ın ikinci rolü: kimlik doğrulama. Login → JWT → Dashboard akışının Hydra tarafındaki mekaniği ve bugün neyin bağlı olmadığı |
| 5.5 | [05-permission-matrix-and-granular-security.md](05-permission-matrix-and-granular-security.md) | GedenLines'tan Hydra'ya Permission evrimi, 5 katmanlı yetkilendirme mimarisi (`ControllerActionBased`, `NavMenuBased`, `ViewBased`, `ComponentBased`, `EntityPropertyBased`) ve `TableDTO`/`MetaColumnDTO` ile kolon/property seviyesinde veri güvenliği |
| 5.6 | [05-default-admin-seed.md](05-default-admin-seed.md) | Boş bir veritabanıyla ilk açılışta gezinebilmek için Hydra core seviyesinde, config-driven bir Role+SystemUser+wildcard Permission seed'i (`DbInitializer.SeedDefaultAdmin`) — her Hydra tabanlı uygulamada yeniden kullanılabilir |
| 5.7 | [07-permission-enforcement-and-status-codes.md](07-permission-enforcement-and-status-codes.md) | 5.5'in 1. katmanının (`ControllerActionBased`) gerçekten uygulanması: deny-by-default `PermissionAuthorizationFilter`, JWT şemasının etkinleştirilmesi, login zincirindeki beş kırık halkanın onarımı, ve reddedilen bir isteğin kullanıcıya nasıl döndüğü (401/403 + `HydraStatusCatalog` + `/status/{code}` sayfası) |

Bu bölüm boyunca kullanılan tüm kod örnekleri gerçek dosyalardan alınmıştır;
bir iddia varsa yanında dosya yolu vardır. Proje-özel "şunu şöyle bağla"
adımları için (bu bölümün 5.4'ü genel mekaniği anlatıyor) bkz.
`Tentacle/docs/login-to-dashboard-flow.md`.

---

## ⭐ İleride Yapılacaklar / Not Edilenler

- `Role`/`Permission` detay ekranlarına `CollectionViewSection` eklenmedi —
  bkz. [5.3](03-dashboard-pages-and-collection-views.md).
- Backend kimlik doğrulama ve uç yetkilendirmesi [5.7](07-permission-enforcement-and-status-codes.md)'de
  bağlandı; **Blazor tarafı henüz bağlı değil** (Login.razor gerçek giriş
  yapmıyor, rota koruması yok) — bkz. [5.4](04-login-to-dashboard-and-gaps.md)
  ve `Tentacle/docs/login-to-dashboard-flow.md`.
- ~~`SystemUserController.LoginAsync` şifre doğrulaması yapmıyor~~ →
  [5.7](07-permission-enforcement-and-status-codes.md)'de düzeltildi
  (`PasswordHasher.Verify`).
- ~~`Default Admin` seed'inin yazdığı wildcard `Permission` bir veri iskeleti~~ →
  [5.7](07-permission-enforcement-and-status-codes.md) ile `PermissionEvaluator`
  tarafından gerçekten okunuyor.
- [5.5](05-permission-matrix-and-granular-security.md)'in 2–5. katmanları
  (`NavMenuBased`, `ViewBased`, `ComponentBased`, `EntityPropertyBased`) henüz
  uygulanmadı — sadece 1. katman çalışıyor.
