# Hydra.MailManagement — Architecture & Design

## Mimari Yaklaşım

`Hydra.MailManagement`, kurumsal e-posta yönetimini modern .NET 9 standartlarında, **SOLID**, **OOP**, **DRY** ve **Strategy / Adapter** tasarım desenlerine uygun olarak sunan tak-çıkar bir modüldür.

```mermaid
classDiagram
    class BaseObject~T~ {
        +Guid Id
        +string Name
        +string Description
        +DateTime AddedDate
        +DateTime? ModifiedDate
        +bool IsActive
    }

    class Mail {
        +MailStatus Status
        +string From
        +string FromName
        +string To
        +string Cc
        +string Bcc
        +string Subject
        +string Body
        +bool IsBodyHtml
        +Guid? MailTemplateId
        +DateTime? SentDate
        +DateTime? FailedDate
        +string ErrorMessage
        +int RetryCount
        +ApplyTemplate()
        +ChangeStatus()
        +AddAttachment()
        +AttachEntity()
    }

    class MailTemplate {
        +string Code
        +string Subject
        +string Body
        +string Parameters
        +RenderSubject()
        +RenderBody()
    }

    class MailAttachment {
        +Guid? MailId
        +string FileName
        +string Extension
        +string ContentType
        +byte[] Data
        +long FileSize
    }

    class BaseObjectMail {
        +Guid MailId
        +Guid EntityId
        +string EntityType
    }

    BaseObject <|-- Mail
    BaseObject <|-- MailTemplate
    BaseObject <|-- MailAttachment
    BaseObject <|-- BaseObjectMail

    Mail "1" *-- "0..*" MailAttachment : Attachments
    Mail "1" *-- "0..*" BaseObjectMail : LinkedEntities
    Mail "0..*" o-- "0..1" MailTemplate : Template
```

---

## Yaşam Döngüsü (State Machine)

Bir e-postanın veritabanındaki durum geçişleri:

```mermaid
stateDiagram-v2
    [*] --> Draft : Yeni oluşturuldu
    Draft --> Queued : Kuyruğa alındı (Queue / SendAsync)
    Queued --> Sent : SMTP/Sağlayıcı iletimi başarılı
    Queued --> Failed : SMTP/Sağlayıcı hatası (Timeout, Auth, vb.)
    Failed --> Queued : Yeniden Deneme (RetryFailedMailsAsync)
    Sent --> Delivered : Webhook/Teslim bildirimi (Opsiyonel)
    Sent --> [*]
    Failed --> [*] : Maksimum deneme aşıldı
```

---

## Tasarım Desenleri (Design Patterns)

1. **Adapter Pattern (`IMailEngineService` -> `FluentEmailEngineService`):**
   * Hydra'nın domain varlığı olan `Mail` nesnesini üçüncü taraf iletim sağlayıcısı olan `IFluentEmail` API'sine dönüştürür.
   * İleride `MailKit`, `SendGrid` veya `Amazon SES` API'sine geçilmek istendiğinde yalnızca yeni bir `IMailEngineService` implementasyonu yazmak yeterlidir (Open/Closed Principle).

2. **Application Part Pattern (`Hydra.MailManagement.WebApi`):**
   * ASP.NET Core `ApplicationPart` mekanizması kullanılarak controller sınıfları tüketici API'nin routing tablosuna çalışma zamanında eklenir.

3. **Polymorphic Link Pattern (`BaseObjectMail`):**
   * E-postaları tek bir nesne tipine hapsetmez (`OrderMail`, `UserMail`, `InvoiceMail` gibi tablolar türetilmez).
   * `EntityType` ve `EntityId` ikilisi ile Hydra ekosistemindeki **herhangi bir entity** ile ilişki kurulabilir.
