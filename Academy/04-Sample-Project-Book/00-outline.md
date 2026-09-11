# Bölüm 4 — Örnek Proje Kitabı: Product Kataloğu

## Neden bu örnek?

Önceki bölümler Hydra'yı **var olan** kodla (Log, Request, RequestCategory)
anlattı — okuyucu hep bitmiş bir şeye bakıyordu. Bu bölüm tam tersini yapıyor:
sıfırdan bir modül kuruyoruz, her adımı test edip puanlıyoruz, karşılaştığımız
her tuzağı olduğu gibi yazıyoruz. Amaç kusursuz bir örnek değil — **gerçekçi**
bir örnek: Hydra'yı yeni tanıyan birinin ilk modülünü kurarken yaşayacağı
şeyleri baştan yaşamak.

Seçilen entity seti kasıtlı olarak Hydra'nın belli başlı desenlerinin hepsini
tek modülde tetikliyor:

| Entity | Hangi deseni gösteriyor |
|---|---|
| `Product` | Temel `BaseObject`, iki farklı FK (`Category`, `Owner`) |
| `ProductCategory` | Basit lookup tablosu, `IHierarchicalObject<T>` adayı (alt kategoriler) |
| `ProductImage` | `CustomFile`'a köprü tablosu ile bire-çok bağlanma (bkz. [3.1](../03-FileManagement-Review/01-current-state-audit.md) — bugünkü sınırlamalarıyla birlikte) |
| `ProductOwner (SystemUser)` | Hydra'nın hazır kimlik/yetki entity'sine (`Hydra.AccessManagement.SystemUser`) dışarıdan bağlanma |
| `Role`, `Permission` | Yeniden tanımlanmıyor — mevcut Hydra çekirdek entity'leri; `Product` üzerinde yetkilendirme örneği için kullanılacak |
| `Log` | Yeniden tanımlanmıyor — her yeni entity gibi `Product` de otomatik Logs sekmesini miras alacak |

## Bölüm planı

| # | Başlık | Durum | Kapsam |
|---|---|---|---|
| 4.1 | [BaseObject anatomisi + ilk entity'ler](01-baseobject-and-first-entities.md) | ✅ yazıldı | `Product`, `ProductCategory` entity + DTO tanımı, `LoadConfigurations()` |
| 4.2 | `ProductImage` ve `CustomFile` köprüsü | ⏳ sırada | `ProductImage` bridge entity, Bölüm 3.1'deki sınırlamaların örnek üzerinde gösterilmesi |
| 4.3 | `ProductOwner` — `SystemUser`'a bağlanma | ⏳ sırada | Navigation config, iki farklı entity'nin ortak `Id` alanı paylaşma tuzakları |
| 4.4 | WebApi: controller'lar ve DI kaydı | ⏳ sırada | `ProductController`, `ProductCategoryController`, `[RegisterAsService]` |
| 4.5 | RazorClassLibrary: CRUD ekranları | ⏳ sırada | `GenericListView`/`GenericFormView`/`GenericDetailsView` ile List/Create/Edit/Details |
| 4.6 | Master-Detail: Category → Products, Product → Images/Logs | ⏳ sırada | `CollectionViewSection` kurulumu, ⭐ bulunan boşluklar |
| 4.7 | Role/Permission ile yetkilendirme denemesi | ⏳ sırada | Mevcut `Role`/`Permission` entity'lerinin `Product` üzerinde nasıl kullanılacağı |
| 4.8 | Uçtan uca test + puanlama | ⏳ sırada | Her bölümün 10 üzerinden puanı, ⭐ toplu liste |

**Not:** 4.2 ve sonrası bir sonraki oturumda yazılacak — bu, kapsamın
büyüklüğü nedeniyle bilinçli bir duraklama, unutkanlık değil. `README.md`
üzerinden bu dosya güncellenerek ilerleme takip edilecek.

## Test/puanlama metodolojisi (4.8'de uygulanacak)

Her bölüm şu beş soruya göre puanlanacak, cevaplar gerçek denemeye dayanacak
(mümkün olduğunca — bu cloud ortamında `dotnet build` çalıştırılamıyor, bkz.
[`Documentation/Hydra/COMMIT_PLAN.md §0.2`](../../../Documentation/Hydra/COMMIT_PLAN.md)):

1. Kod, mevcut Hydra konvansiyonlarına ne kadar sadık kaldı?
2. Kaç satırlık "boilerplate" gerekti — DevExpress XAF'ın vaadi olan "az kodla
   çok ekran" ne kadar gerçekleşti?
3. Karşılaşılan ilk tuzak neydi, dokümantasyonda zaten var mıydı yoksa yeni mi
   keşfedildi?
4. Üretilen ekranlar Tentacle'daki muadillerine (`Request`/`RequestCategory`)
   kalite olarak ne kadar yakın?
5. Bir sonraki geliştiricinin bu örnekten kopyalayabileceği kısım hangisi?

---

## ⭐ İleride Yapılacaklar / Not Edilenler

- Bu kitap "canlı" — her oturumda bir bölüm daha eklenecek şekilde tasarlandı.
  README.md'deki tabloyu her yeni bölümde güncellemek gerekiyor.
- 4.7 (Role/Permission ile yetkilendirme) muhtemelen bu kitabın en az
  belgelenmiş köşesi olacak — bugün Hydra'da entity-bazlı yetkilendirmenin
  (mesela "sadece Product Owner rolündekiler fiyatı değiştirebilir") hazır bir
  mekanizması var mı, yoksa elle mi yazılıyor, bu araştırılmadı.
