# Hydra.MailManagement — Integration Playbook

> **Standard Module Integration Playbook Specification**
> Bu kılavuz; `Hydra.MailManagement` modülünü yeni veya mevcut bir Hydra tabanlı .NET projesine (WebApi, Console Worker, Windows Service veya Web Uygulaması) sıfırdan entegre edip ayağa kaldırmak için **Developer** ve **AI Agent**'ların takip etmesi gereken standart adımları içerir.

---

## 1. Prerequisites (Gereksinimler & Bağımlılıklar)

* **Target Framework:** .NET 9.0 (veya üstü)
* **Hydra Çekirdek Bağımlılığı:** `Hydra` (Core Class Library)
* **Opsiyonel Web API Bağımlılığı:** `Hydra.WebApi` (Controller'lar kullanılacaksa `Hydra.MailManagement.WebApi` projesi de dahil edilir)
* **NuGet Paketleri:**
  * `FluentEmail.Core` (3.0.2)
  * `FluentEmail.Smtp` (3.0.2)
  * `Microsoft.Extensions.Configuration.Binder`
  * `Microsoft.Extensions.Options.ConfigurationExtensions`
  * `Microsoft.Extensions.DependencyInjection.Abstractions`

---

## 2. Configuration (`appsettings.json`)

Projelerinizin `appsettings.json` ve `appsettings.Development.json` dosyalarına aşağıdaki `MailSettings` bloğunu ekleyin:

```json
{
  "MailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "sistem@firmaniz.com",
    "Password": "uygulama-sifreniz",
    "EnableSsl": true,
    "UseDefaultCredentials": false,
    "DefaultFromEmail": "sistem@firmaniz.com",
    "DefaultFromName": "Hydra Kurumsal Sistemi"
  }
}
```

> [!TIP]
> Geliştirme ortamında (Development) gerçek e-posta göndermemek için Papercut, MailHog veya Mailtrap gibi yerel/test SMTP sunucuları kullanılabilir (`Host: "localhost"`, `Port: 25`).

---

## 3. Database & EF Core Setup

Modül 4 temel entity içerir: `Mail`, `MailTemplate`, `MailAttachment`, `BaseObjectMail`. Tümü `BaseObject<T>`'den türediği için Hydra'nın UUIDv7 (`Guid.CreateVersion7()`), `AddedDate`, `ModifiedDate`, `IsActive` ve `RowVersion` mekanizmalarını otomatik devralır.

Tüketen uygulamanızın `DbContext` sınıfına ilgili `DbSet`'leri ekleyin:

```csharp
using Hydra.MailManagement.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public DbSet<Mail> Mails { get; set; } = null!;
    public DbSet<MailTemplate> MailTemplates { get; set; } = null!;
    public DbSet<MailAttachment> MailAttachments { get; set; } = null!;
    public DbSet<BaseObjectMail> BaseObjectMails { get; set; } = null!;

    // ... diğer DbSet'ler ...
}
```

Migration oluşturup veritabanını güncelleyin:
```powershell
dotnet ef migrations add AddHydraMailManagement
dotnet ef database update
```

---

## 4. DI Registration (`Program.cs`)

`Hydra.MailManagement` kütüphanesini referans aldıktan sonra, `Program.cs` içine tek bir satır ekleyin:

```csharp
using Hydra.MailManagement.Extensions;

// Core Mail Servislerini, Konfigürasyonu ve FluentEmail Motorunu Kaydeder:
builder.Services.AddHydraMailManagement(builder.Configuration);
```

Bu metot arka planda:
1. `HydraMailOptions` nesnesini konfigürasyondan doldurur.
2. `FluentEmail` motorunu ve SMTP istemcisini ayağa kaldırır.
3. `IMailEngineService`, `MailService` ve `MailTemplateService` servislerini Scoped olarak DI konteynerine kaydeder.

---

## 5. API & Controller Registration (WebApi Projeleri İçin)

Eğer REST uç noktalarını (`/api/Mail`, `/api/MailTemplate`) doğrudan projenize dahil etmek istiyorsanız:
1. Projenize `Hydra.MailManagement.WebApi` referansını ekleyin.
2. `Program.cs` dosyasındaki Controller kaydına `AddHydraMailManagementControllers` ekleyin:

```csharp
using Hydra.MailManagement.WebApi.Extensions;

builder.Services.AddControllers()
    .AddHydraMailManagementControllers(); // ApplicationPart ile otomatik bağlar!
```

> [!NOTE]
> Bu sayede projenizde tek bir satır controller kodu kopyalamanıza gerek kalmaz. `/api/Mail/Create`, `/api/Mail/Send/{id}`, `/api/Mail/SendQuick`, `/api/MailTemplate/Preview/{code}` gibi tüm uç noktalar anında Swagger ve HTTP rotalarına dahil edilir.

---

## 6. Quick Start & Code Recipes (Hızlı Kullanım Örnekleri)

### Senaryo A: Kod İçinden Doğrudan E-Posta Gönderme (MailService)

```csharp
public class OrderBusinessService
{
    private readonly MailService _mailService;

    public OrderBusinessService(MailService mailService)
    {
        _mailService = mailService;
    }

    public async Task NotifyCustomerAsync(Order order)
    {
        var mail = new Mail(
            to: order.CustomerEmail,
            subject: $"Siparişiniz Alındı: #{order.OrderNumber}",
            body: $"<h3>Sayın {order.CustomerName},</h3><p>{order.OrderNumber} numaralı siparişiniz onaylandı.</p>"
        );

        // İsteğe bağlı: Polimorfik olarak Order entity'sine bağla (Audit)
        mail.AttachEntity(order.Id, nameof(Order));

        // Veritabanına kaydeder ve FluentEmail üzerinden asenkron iletir:
        var result = await _mailService.QueueAndSendAsync(mail);
        
        if (!result.Success)
        {
            // Hata loglandı ve Mail.Status = 'Failed' oldu
        }
    }
}
```

### Senaryo B: Şablon Kullanarak Parametrik E-Posta Gönderme

```csharp
// 1. Şablon Oluşturma (Genellikle seed veya admin panelinden yapılır):
var welcomeTemplate = new MailTemplate(
    code: "WELCOME_USER",
    subject: "Aramıza Hoş Geldiniz @UserName!",
    body: "<h1>Merhaba @UserName</h1><p>Hesabınız aktif edilmiştir. Giriş adresi: <a href='@LoginUrl'>Giriş Yap</a></p>",
    parameters: "UserName,LoginUrl"
);
await mailTemplateService.CreateAsync(welcomeTemplate);

// 2. Şablonu Kullanarak Gönderme:
var mail = new Mail("ahmet@ornek.com", welcomeTemplate, new 
{
    UserName = "Ahmet Yılmaz",
    LoginUrl = "https://app.sirket.com/login"
});

await _mailService.QueueAndSendAsync(mail);
```

### Senaryo C: REST API Üzerinden Hızlı Gönderim (HTTP Client / cURL)

```http
POST /api/Mail/SendQuick HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "to": "alici@ornek.com",
  "subject": "Şifre Sıfırlama Kodu",
  "templateCode": "PASSWORD_RESET",
  "templateParameters": {
    "UserName": "Ahmet",
    "ResetCode": "482910"
  },
  "entityId": "0191eb56-0a56-789a-bcde-f0123456789a",
  "entityType": "SystemUser"
}
```

---

## 7. Agent Directives (AI / LLM İçin Kritik Yönergeler)

AI Agent'lar veya asistanlar bu modülü projelere entegre ederken şu kurallara kesinlikle uymalıdır:

1. **Entity Mirası:** `Mail`, `MailTemplate`, `MailAttachment` sınıfları mutlaka `BaseObject<T>`'den türemiş olmalıdır. `Id` alanını manuel `Guid.NewGuid()` yapmayın; `BaseObject<T>`'in varsayılan UUIDv7 üreticisini koruyun.
2. **Status Takibi:** E-postalar doğrudan `Sent` olarak kaydedilmez. Akış her zaman `Draft` / `Queued` -> iletim -> `Sent` veya `Failed` şeklinde işletilmelidir.
3. **ApplicationPart Kullanımı:** WebApi projesinde yeni `MailController` veya `MailTemplateController` dosyaları **oluşturmayın**. `AddHydraMailManagementControllers()` extension metodunu çağırarak kütüphanedeki hazır controller'ları bağlayın.
4. **Hata Yönetimi:** Gönderim hatalarında exception fırlatılıp akış kesilmez; `Mail.Status = Failed` yapılır, `ErrorMessage` ve `RetryCount` doldurulur, `LogService` ile loglanır.
