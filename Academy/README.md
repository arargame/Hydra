# Hydra Academy

Hydra ekosisteminin (Core → WebApi → RazorClassLibrary → referans uygulama Tentacle)
mimarisini, tasarım kararlarını ve şu anki olgunluk seviyesini belgeleyen, bir kitap
gibi ilerleyen doküman seti.

**Neden "Academy"?** Bu klasör bir API referansı değil. Amaç, ekibe yeni katılan
birinin (ya da altı ay sonra buraya dönen bizim) "bu neden böyle yapılmış" sorusuna
kod okumadan cevap bulabilmesi. Her bölüm gerçek koda referans verir, uydurma örnek
kullanmaz — bir iddia varsa yanında dosya yolu ve satırı vardır.

**Nasıl okunmalı?** Sırayla. Bölüm 1 "ne var" sorusunu cevaplıyor, Bölüm 2 "nasıl
çalışıyor" sorusunu, Bölüm 3 tek bir alt sistemi mercek altına alıp puanlıyor, Bölüm 4
ise öğrenilenleri sıfırdan bir örnek üzerinde uyguluyor.

Her bölümün sonunda ⭐ **İleride Yapılacaklar** başlığı var — bugün doğru olan ama
gelecekte gözden geçirilmesi gereken kararlar orada toplanıyor. Bu doküman seti canlı;
mimari değiştikçe güncellenecek, eskiyen iddialar silinecek.

---

## Bölüm 1 — Mimari (Ne var, hangi katman ne iş yapıyor)

| # | Doküman | İçerik |
|---|---|---|
| 1.1 | [01-Architecture/01-hydra-core.md](01-Architecture/01-hydra-core.md) | Çekirdek DLL: `BaseObject`, `Repository<T>`/`Service<T>`, DAL, DTO/`ViewDTO`/`TableDTO` sözleşmesi, Filter sistemi |
| 1.2 | [01-Architecture/02-hydra-webapi.md](01-Architecture/02-hydra-webapi.md) | Genel REST katmanı: `MainController<T>`, DI kayıtları, generic CRUD endpoint'leri |
| 1.3 | [01-Architecture/03-hydra-razorclasslibrary.md](01-Architecture/03-hydra-razorclasslibrary.md) | Blazor component kiti: generic CRUD view'ları, `ApiClient<T>`, `LookupService`, theming |
| 1.4 | [01-Architecture/04-tentacle-reference-app.md](01-Architecture/04-tentacle-reference-app.md) | Tentacle referans uygulaması: Hydra'yı gerçek bir problemde (iş takibi) nasıl kullandığımız |

## Bölüm 2 — Çekirdek Sistemler (Nasıl çalışıyor, derinlemesine)

| # | Doküman | İçerik |
|---|---|---|
| 2.1 | [02-Core-Systems/01-table-dto-and-statelessness.md](02-Core-Systems/01-table-dto-and-statelessness.md) | `TableDTO`'nun backend↔frontend arasında taşınma hikayesi; bu mimarinin adı, stateless doğası |
| 2.2 | [02-Core-Systems/02-components-and-theming.md](02-Core-Systems/02-components-and-theming.md) | Bir Blazor component'i nasıl çalışır; HTML/CSS'i dışarıdan enjekte etme yolları |
| 2.3 | [02-Core-Systems/03-journey-of-a-value.md](02-Core-Systems/03-journey-of-a-value.md) | *A Journey of a Value* — bir filtre kutusuna yazılan `"a"` harfinin SQL `WHERE`'e kadar giden ve geri dönen tam yolculuğu |

## Bölüm 3 — Alt Sistem İncelemesi: File Management

| # | Doküman | İçerik |
|---|---|---|
| 3.1 | [03-FileManagement-Review/01-current-state-audit.md](03-FileManagement-Review/01-current-state-audit.md) | Mevcut dosya işleme kodunun tam envanteri, 10 üzerinden puan, `Hydra.FileManagement` olarak ayrıştırma önerisi |

## Bölüm 4 — Örnek Proje Kitabı: Product Kataloğu

Product / ProductCategory / ProductImage (CustomFile) / ProductOwner (SystemUser) /
Role / Permission / Log — `BaseObject` üzerinden sıfırdan kurulan bir örnek modül.
Her bölüm yazılıp test edildikten sonra puanlanacak, kitabın sonunda ⭐ önerilerin
toplandığı bir "gelecek çalışmalar" bölümü olacak.

| # | Doküman | Durum |
|---|---|---|
| 4.0 | [04-Sample-Project-Book/00-outline.md](04-Sample-Project-Book/00-outline.md) | Kitabın tam bölüm planı |
| 4.1 | [04-Sample-Project-Book/01-baseobject-and-first-entities.md](04-Sample-Project-Book/01-baseobject-and-first-entities.md) | `BaseObject` anatomisi + `Product`/`ProductCategory` tanımı |
| 4.2+ | — | Sıradaki bölümler için bkz. outline — henüz yazılmadı |

---

## Kaynak repo haritası

Bu dokümanlar dört repoyu birlikte kapsıyor, hepsi `C:\Users\ararg\source\AIRepos\` altında kardeş klasörler:

- **Hydra** — çekirdek DLL + `Hydra.WebApi` + `Hydra.RazorClassLibrary` (bu üçü tek solution: `Hydra.sln`)
- **Tentacle** — Hydra'yı kullanan referans uygulama, kendi solution'ı (`HydraTentacle.sln`)

`Hydra/Docs/` klasörüyle karıştırmayın: `Docs/` geliştirici notları (belirli bir hatanın
neden olduğu, belirli bir invariant'ın ne olduğu) tutuyor — kısa, göreve odaklı.
`Academy/` ise "sıfırdan anlat" perspektifiyle yazılıyor, mimarinin bütününü hedefliyor.
İkisi çelişmemeli; bir gerçek ikisinde de aynı olmalı.
