# 3.1 — File Management: Mevcut Durum Denetimi

**Amaç:** Bugün Hydra'da dosya işlemleriyle ilgili ne var, ne çalışıyor, ne
sadece niyet olarak duruyor — hepsini gerçek koddan çıkarıp, `Hydra.FileManagement`
adında ayrı, bağımsız kullanılabilir bir kütüphaneye çıkarmaya değip
değmeyeceğine (ve değerse neyin eksik olduğuna) karar verebilmek için.

**Yöntem:** Repo'da "file" geçen her `.cs`/`.razor` dosyası tek tek okundu; ayrıca
gerçek bir kullanım örneği (Tentacle'daki `RequestAttachment`) izlendi. Aşağıdaki
her iddianın yanında dosya yolu var — "muhtemelen şöyledir" yok.

## Envanter: bugün var olan her şey

| Dosya | Ne yapıyor |
|---|---|
| `Hydra/FileOperations/CustomFile.cs` | `BaseObject<CustomFile>`'dan türeyen dosya entity'si. `byte[] Data`, `Extension`, `Path`, `ContainerName`, hesaplanan `LengthAsKb/Mb/Gb`, `Category` (uzantıdan), `Base64String`, `DataAsString`, `GetMemoryStream()`. |
| `Hydra/Utils/FileExtensions.cs` | Uzantı → `FileCategory` (Image/Audio/Video/Document/Unsupported) eşleme tablosu. |
| `Hydra/Utils/FileHelper.cs` | `File.ReadAllBytes`/`File.WriteAllBytes` etrafında ince bir sarmalayıcı. |
| `Hydra/Services/FileService.cs` | `CreateCustomFileFromPath`, `SaveFile` — yerel dosya sistemine yazma/okuma. |
| `Hydra/DTOs/FileInfoDTO.cs` | `Id`/`Name`/`Extension` taşıyan minik bir DTO — nerede kullanıldığı bulunamadı. |
| `Hydra/DataModels/MetaColumn.cs` + `MetaColumnDTO.cs` | `IsFileColumn` — `byte[]` kolonları generic Select sorgusundan otomatik dışlıyor (bkz. aşağıda, bu iyi bir tasarım kararı). |

**Gerçek kullanım örneği:** Tentacle'daki `RequestAttachment`
(`Tentacle/Source/HydraTentacle.Core/Models/Request/RequestAttachment.cs`):

```csharp
public class RequestAttachment : BaseObject<RequestAttachment>
{
    public Guid RequestId { get; set; }
    public Request Request { get; set; } = null!;
    public Guid FileId { get; set; }
    public CustomFile File { get; set; } = null!;
}
```

Bu, bir entity'yi `CustomFile`'a bire-çok bağlamanın kanonik yolu — köprü
tablosu, iki foreign key. Bu kitabın Bölüm 4'ünde kuracağımız
`ProductImage` de tam olarak bu şablonu kullanacak.

## Ne ÇALIŞMIYOR — somut bulgular

**1. Yükleme (upload) hiçbir yerde uçtan uca bağlı değil.**
`Hydra.RazorClassLibrary/Components/CRUD/ColumnFieldComponent.razor`:

```razor
case HtmlElementType.InputToUploadFile:
    <input class="hydra-input" id="@FieldId" type="file" disabled="@IsDisabled" />
    break;
```

Karşılaştırın — kütüphanenin diğer her input tipinde `@onchange="OnRawChanged"`
var (dropdown, textarea, checkbox, metin kutusu). Dosya input'unda **yok**. Bu
element, tarayıcıda tıklanabilir bir dosya seçici açar ama seçilen dosya hiçbir
yere gitmez — component seçimi dinlemiyor.

**2. Sunucuda dosya yükleme/indirme endpoint'i yok.**
`Hydra.WebApi/Controllers/` altında `CustomFile` için bir controller aranmış,
bulunamamış. `MainController<T>` deseni `CustomFile` için de kullanılabilirdi
(`CustomFileController : MainController<CustomFile>`) ama böyle bir dosya yok.
Teorik olarak `GET /CustomFile/Get/{id}` genel `Get` endpoint'i üzerinden dosya
çekilebilir — ama:

- `Data` (`byte[]`) düz JSON gövdesinde base64 olarak döner; gerçek bir HTTP
  dosya yanıtı (`Content-Type`, `Content-Disposition: attachment`, stream)
  değil, tarayıcı bunu "indir" olarak değil "JSON metni" olarak görür.
- `multipart/form-data` ile yükleme için hiçbir endpoint yok — bugün bir dosyayı
  sunucuya koymanın API üzerinden **hiçbir yolu yok**, sadece sunucu tarafı
  kodun (`FileService.CreateCustomFileFromPath`) yerel diskten okuyup entity
  oluşturması var.

**3. Depolama stratejisi kararsız — şema, kod ile çelişiyor.**
Migration'da (`Tentacle/.../Migrations/20251221011840_RecreateRequestSchema.cs`):

```csharp
Data          = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
Path          = table.Column<string>(type: "nvarchar(max)", nullable: true),
ContainerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
```

`Data` sütunu var — yani dosya bugün **SQL Server'a blob olarak** yazılıyor
(çalışıyorsa). Ama `Path` ve `ContainerName` alanları da var, bunlar "dosya bir
blob storage container'ında, şu path'te yaşıyor" senaryosunun izleri. Repo
genelinde `ContainerName` **hiçbir yerde bir değere set edilmiyor**
(`grep -rn "ContainerName"` sonucu sadece tanım ve migration dosyaları) — yani
iki farklı depolama stratejisinin (DB blob vs. harici storage) izleri aynı
entity'de duruyor, hangisinin "gerçek" olduğu koddan belli değil.

**4. `Base64String`/`DataAsString` hesaplanıyor ama hiçbir yerde tüketilmiyor.**
`grep -rn "Base64String|DataAsString"` sonucu, bu property'lerin tanımlandığı
`CustomFile.cs` dışında hiçbir gerçek çağıran yok. Ölü kod değiller (public API
yüzeyinin parçası) ama bugün kimse kullanmıyor.

**5. Bellek modeli büyük dosyada sorun çıkarır.**
`FileService.SaveFile` ve `CustomFile.Data`, dosyanın **tamamını** `byte[]`
olarak belleğe alıyor — stream/chunk yok. Küçük eklerde (birkaç MB) sorun değil;
büyük dosyalarda (video, arşiv) hem bellek hem de `varbinary(max)` sütununa tek
seferde yazım performans sorunu yaratır.

**6. Doğrulama (validation) yok.**
Boyut sınırı, izin verilen uzantı listesi, içerik tipi doğrulaması (dosya
gerçekten uzantısının iddia ettiği tip mi) — hiçbiri yok. `FileCategory`
sınıflandırması sadece **uzantıya** bakıyor (`FileExtensions.GetFileCategory`),
dosyanın gerçek baytlarına (magic number) değil — kötü niyetli bir kullanıcı
`.exe` dosyasını `.png` olarak yeniden adlandırıp yükleyebilir (yükleme
çalışsaydı).

## İyi yapılmış olan tek şey: `IsFileColumn`

Adil olmak gerekirse, bir tasarım kararı gerçekten doğru: `MetaColumn.IsFileColumn`
(`ValueType == ByteArray && (IsNavigation || Table.Name == "CustomFile")`),
`QueryBuilder.PrepareSelectedColumnsString`'te generic listeleme sorgularından
otomatik dışlanıyor:

```csharp
var selectedColumns = _table.GetSelectedMetaColumnsIncludingJoins
                             .Where(sc => !sc.IsFileColumn)
                             .ToList();
```

Yani bir `Product` listesi çekerken, o ürüne bağlı `ProductImage.Data`'nın
megabaytlarca içeriği **yanlışlıkla** her satırla birlikte sürüklenmiyor. Bu,
sisteme baştan düşünülmüş, doğru bir sınır — dosya içeriği yalnızca özel olarak
istendiğinde gelmeli, listeleme sorgusuna bedavadan binmemeli.

## Puan: 10 üzerinden **3**

| Boyut | Puan /2 | Gerekçe |
|---|---|---|
| Veri modeli | 1.5 | `CustomFile` makul tasarlanmış (kategori, boyut hesapları, BaseObject uyumu) ama `Path`/`ContainerName` kullanılmayan alanlar taşıyor. |
| Depolama stratejisi | 0.5 | DB blob mu, harici storage mu belirsiz; şema iki farklı niyeti aynı anda taşıyor. |
| API yüzeyi | 0 | Upload/download için hiçbir dedicated endpoint yok. |
| UI entegrasyonu | 0 | `<input type="file">` bağlı değil, hiçbir ekranda dosya yükleme akışı çalışmıyor. |
| Doğrulama/güvenlik | 0.5 | Sadece uzantı bazlı kategori sınıflandırması var; boyut/tip/magic-number kontrolü yok. |
| Sorgu güvenliği (list'e blob sızmaması) | 1.5 | `IsFileColumn` mekanizması iyi düşünülmüş, bugün çalışıyor. |
| **Toplam** | **3 / 10** | Bir veri modeli ve birkaç yardımcı fonksiyon var; **çalışan bir dosya yönetim sistemi yok.** |

Bu düşük puanı olumsuz bir eleştiri olarak değil, net bir başlangıç noktası
olarak okuyun: bugün burada olan şey "iskelet" — üzerine inşa edilecek doğru
temeller (entity modeli, sorgudan dışlama) zaten doğru atılmış, eksik olan
transport/API/UI katmanı.

## `Hydra.FileManagement` olarak ayrıştırma önerisi

DevExpress XAF benzeri "amacına göre ayrı kütüphane" vizyonuna uygun bir
`Hydra.FileManagement` şu parçaları toplayabilir:

1. **Taşınacaklar (bugün var, sadece yer değiştirir):** `CustomFile`,
   `FileExtensions`, `FileHelper`, `FileService`, `FileInfoDTO`.
2. **Yeni yazılması gerekenler (bugün yok):**
   - `CustomFileController : MainController<CustomFile>` + gerçek bir
     `POST /CustomFile/Upload` (`multipart/form-data`, `IFormFile`) ve
     `GET /CustomFile/Download/{id}` (doğru `Content-Type`/`Content-Disposition`
     ile gerçek dosya akışı, JSON zarfı değil).
   - `ColumnFieldComponent`'teki dosya input'una gerçek bir `InputFile`
     (Blazor'ın kendi component'i) bağlanması + yükleme ilerleme göstergesi.
   - Boyut/uzantı/MIME doğrulaması — hem istemci hem sunucu tarafında.
   - Depolama stratejisi kararı: DB blob mu kalsın (küçük dosyalar, ek belge
     gibi kullanım için makul) yoksa bir `IFileStorageProvider` soyutlaması
     (yerel disk / Azure Blob / S3 uyumlu) eklenip `ContainerName`/`Path`
     alanları gerçek bir anlam mı kazansın — bu kitabın kapsamı dışında,
     ayrı bir karar dokümanını hak ediyor.
3. **Test edilecekler:** büyük dosya (>10MB) davranışı, eşzamanlı yükleme,
   silinen bir entity'nin dosyasının (orphan) temizlenip temizlenmediği —
   bugün hiçbiri test edilmemiş çünkü akış çalışmıyor.

---

## ⭐ İleride Yapılacaklar / Not Edilenler

- Bu kitabın Bölüm 4'ünde kurulacak `ProductImage` örneği, bugünkü `CustomFile`
  ile aynı sınırlamaları miras alacak — yani örnek projede "resim yükle" akışı
  da gerçek bir HTTP upload'a bağlanamayacak, sadece veri modeli/ilişki
  seviyesinde gösterilebilecek. Bu, Bölüm 4'te açıkça belirtilecek.
- Depolama stratejisi kararı (DB blob / harici storage) maliyet ve ölçek
  sorusu — kullanıcı bunun "yarın öbür gün" ele alınacağını belirtti, bu
  yüzden burada bir tercih dayatılmadı, sadece iki seçenek de netleştirildi.
