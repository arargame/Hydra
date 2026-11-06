# Bölüm 2.6 — Create Story: UI'dan Veritabanına Bir Kaydın Uçtan Uca Hikayesi

Bu doküman, Hydra ekosisteminde bir varlığın oluşturulması (`Create`) sürecinin Blazor UI katmanından başlayıp SQL Server veritabanına ve oradan geri bildirim olarak arayüze dönüşüne kadar olan tüm yaşam döngüsünü teknik detaylarıyla ele alır.

---

## 1. Yaşam Döngüsü Özeti (Sequence Diagram)

```mermaid
sequenceDiagram
    autonumber
    actor User as Kullanıcı (Tarayıcı)
    participant UI as GenericFormView (Blazor)
    participant Client as ApiClient / HttpClientService
    participant API as MainController<T> (WebApi)
    participant Svc as Service<T> (İş Mantığı)
    participant Repo as Repository<T> (DAL)
    participant UoW as UnitOfWork (EF Core)
    participant DB as SQL Server (Ana DB)
    participant LogDB as Log DB (TentacleLogDatabase)

    User->>UI: Form alanlarını doldurur & "Create" butonuna basar
    UI->>UI: ValidateForm() (Client-side validation & IValidatableObject)
    alt Client Doğrulaması Başarısız
        UI-->>User: İlgili alanlar kırmızı çerçevelenir
    else Client Doğrulaması Başarılı
        UI->>Client: Client.CreateAsync(Model)
        Client->>Client: JSON Serialize & Bearer Token ekle
        Client->>API: HTTP POST /api/{Entity}/Create
        API->>API: SessionContext & JWT / Origin doğrulaması
        API->>Svc: Service.CreateAsync(entity)
        Svc->>Svc: entity.IsValid(out validationResults)
        alt Server Domain Doğrulaması Başarısız
            Svc-->>API: ResponseObject(Success=false, ValidationMessages)
        else Server Doğrulaması Başarılı
            Svc->>Repo: Repository.CreateAsync(entity)
            Repo->>Repo: GetExistingEntityAsync (Tekillik kontrolü)
            Repo->>Repo: entity.Id = Guid.NewGuid(), AddedDate, ModifiedDate
            Repo->>Repo: ChangeEntityState(entity, Added)
            Repo-->>Svc: isDone = true
            Svc->>UoW: CommitAsync()
            UoW->>DB: EF Core SaveChangesAsync() -> SQL INSERT
            DB-->>UoW: Row count = 1 (Commit Success)
            UoW-->>Svc: isCommitted = true
            Svc->>Svc: response.SetSuccess(true).SetId(entity.Id).SetData(entity)
            Svc->>LogDB: SaveRepositoryLogsAsync() -> INSERT Log (Audit)
            Svc-->>API: response (IResponseObject with Data & Success=true)
        end
        API-->>Client: HTTP 200 OK (JsonResult with ResponseObject)
        Client->>Client: TryDeserializeEnvelope<T>
        alt envelope.Success == false
            Client->>Client: HydraStatusNotifier.Raise(...)
            Client-->>UI: return null
            UI-->>User: Error Banner (HydraStatusNotifier güvenli hata mesajı)
        else envelope.Success == true
            Client-->>UI: return envelope.Data (Kaydedilen Varlık)
            UI->>UI: OnSaved.InvokeAsync(result)
            UI-->>User: Navigation.NavigateTo("/{BasePath}") (List View'a Yönlendirme)
        end
    end
```

---

## 2. Mimari Katmanlar ve Sorumluluklar

| Katman | Dosya / Bileşen | Sorumluluk |
|---|---|---|
| **Arayüz (UI)** | `GenericFormView.razor` | Form meta-verisini render etme, istemci doğrulama, dinamik girdi bağlama ve yönlendirme. |
| **HTTP Taşıma** | `ApiClient<T>.cs` & `HttpClientService.cs` | JWT taşıma, `ResponseObject` zarfını açma, `HydraStatusNotifier` üzerinden hata yayınlama. |
| **REST API** | `MainController<T>.cs` | Model binding, HTTP durum kodları ile envelope durumunu eşleme, route koruması. |
| **İş Mantığı** | `Service<T>.Command.cs` | Domain kuralları (`IsValid`), UoW koordinasyonu, response payload (`SetData`) paketleme. |
| **Veri Erişim** | `Repository<T>.Command.cs` | DB tekillik kontrolü (`GetExistingEntityAsync`), Id ve Audit tarihlerinin atanması, ChangeTracker. |
| **Birim İş & SQL** | `UnitOfWork.cs` & `SqlExceptionHelper.cs` | `SaveChangesAsync()`, SQL hata kodlarının CWE-209 uyumlu güvenli mesajlara çevrilmesi. |
| **Denetim İzi** | `LogFactory.cs` & `LogService.cs` | Başarılı oluşturma veya teknik hataların izole log veritabanına kaydedilmesi. |

---

## 3. Sahte Hata (False Failure) Post-Mortem

Bir Create işlemi sırasında veritabanında işlem başarıyla commit edildiği halde arayüzde *"Save failed. Please check the required fields."* uyarısının çıkması şu iki eksiklikten kaynaklanmıştı:
1. `Service.Command.cs`: Başarılı oluşturma sonrasında `response.SetData(entity)` çağrılmadığı için JSON zarfı `data: null` dönüyordu.
2. `GenericFormView.razor`: `result == null` durumunu mutlak hata kabul edip sabit metin gösteriyordu.

**Çözüm:** `Service.Command.cs` içine `response.SetId(entity.Id).SetData(entity)` eklendi; `GenericFormView.razor` ise `HydraStatusNotifier` ile entegre edilerek sunucudan gelen gerçek hata mesajlarını (örneğin SQL mükerrer kayıt, validasyon vb.) gösterecek şekilde güçlendirildi.
