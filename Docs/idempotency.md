# Idempotency — çalışma notu

Bu belge, Hydra tabanlı API'lerde `Create` gibi yan etkisi olan komutların aynı istemci isteği
tekrarlandığında birden fazla kez çalışmasını engellemek için planlanan idempotency altyapısını
tanımlar. Bu bir uygulama değil; uygulanmadan önce alınmış tasarım kararlarının notudur.

## Mevcut durum

`Hydra.WebApi` içindeki ortak `MainController<T>.Create` endpoint'i `Service<T>.CreateAsync`
çağırır ve sonucu `ResponseObject` içinde döner. `ResponseObject` kullanımı sorun değildir: tüm
istemcilerin başarılı/başarısız iş sonucunu aynı sözleşmeyle okumasını sağlar.

Ancak `JsonResult` için bir HTTP durum kodu belirtilmediğinde cevap pratikte `200 OK` olur. Bu,
validation, duplicate kayıt ve beklenmeyen sunucu hatalarının HTTP katmanında birbirinden ayırt
edilememesine neden olur. Idempotency eklendiğinde de ilk cevabın HTTP kodu ve JSON gövdesi
tekrar üretilebilmelidir.

Hydra'daki `MemoryCacheService` ve diğer cache servisleri entity/query cache amaçlıdır. Bir create
komutunun tekrarını güvenli biçimde engelleyen idempotency kayıtları değildir.

## Hedef davranış

İstemci, yan etkisi olan bir istekle benzersiz bir `Idempotency-Key` header'ı gönderir.

```http
POST /api/Employee/Create
Idempotency-Key: 5d20d3d4-2c91-4a16-9d72-dac8d962a5cc
Content-Type: application/json
```

Sistem anahtarı kullanıcı/oturum ve endpoint ile birlikte değerlendirir. Aynı header değerini
başka kullanıcı veya başka endpoint için kullanmak aynı işlem olarak sayılmamalıdır.

```text
scope = authenticated-user-or-session + HTTP method + normalized route
record key = scope + Idempotency-Key
request fingerprint = SHA-256(request body)
```

| Durum | HTTP | Davranış |
|---|---:|---|
| İlk başarılı create | 201 Created | İşlem tamamlanır; cevap kaydedilir. |
| Tekrarlanan, aynı key ve aynı body | İlk cevabın kodu | Controller çalışmadan kaydedilen cevap döner. |
| Aynı key hâlâ işleniyor | 409 Conflict | İstemci kısa süre sonra tekrar deneyebilir (`Retry-After`). |
| Aynı key, farklı request body | 422 Unprocessable Entity | Anahtarın hatalı yeniden kullanımı bildirilir. |
| Model/iş girdisi validation hatası | 400 Bad Request | `ResponseObject` hata gövdesi döner. |
| İş kuralı çakışması (örn. benzersiz e-posta) | 409 Conflict | `ResponseObject` hata gövdesi döner. |
| Beklenmeyen DB/sunucu hatası | 500 Internal Server Error | Cevap idempotency kaydı olarak tamamlanmış sayılmaz; tekrar denenebilir. |

## Kalıcı kayıt gerekli

Production çözümü `IMemoryCache` kullanmamalıdır. Tek sunucuda bile process yeniden başlatılabilir;
birden çok instance/container olduğunda ise her instance kendi belleğini görür.

İlk production sürüm için en uygun çözüm, uygulamanın ana veritabanında aşağıdaki nitelikleri
taşıyan bir `IdempotencyRecord` tablosudur:

```text
Id
Scope
Key
RequestHash
State              // Processing, Completed
ResponseStatusCode
ResponseContentType
ResponseBodyJson
CreatedAtUtc
CompletedAtUtc
ExpiresAtUtc
```

Veritabanında `(Scope, Key)` üzerinde **unique index** zorunludur. Bu index iki eşzamanlı isteğin
aynı anda kaydı boş görüp iki create yapmasını engelleyen nihai güvencedir. Redis gibi dağıtık cache
daha sonra performans için eklenebilir; fakat kritik create işlemlerinde kalıcı DB kaydı esas kaynak
olmalıdır.

## ResponseObject ve HTTP cevapları

`ResponseObject` korunacaktır. HTTP durum kodunun doğru olması için sonuç türü açıkça taşınmalıdır;
controller mesaj metninden hata türü tahmini yapmamalıdır.

Önerilen kavram:

```csharp
public enum ResponseType
{
    Success,
    ValidationError,
    Conflict,
    ServerError
}
```

`ResponseObject` bu türü taşıdığında ortak `Create` endpoint'i kendi gövde sözleşmesini bozmadan
uygun HTTP cevabını üretir:

```csharp
return response.ResponseType switch
{
    ResponseType.Success         => CreatedAtAction(nameof(Get), new { id = response.Id }, response),
    ResponseType.ValidationError => BadRequest(response),
    ResponseType.Conflict        => Conflict(response),
    _                            => StatusCode(StatusCodes.Status500InternalServerError, response)
};
```

Idempotency katmanının `409` (işlem sürüyor) ve `422` (key/body uyuşmazlığı) cevapları bundan
ayrıdır: filter controller action'ına hiç girmeden bu cevapları döner.

## Uygulama şekli

`ActionFilterAttribute` içine doğrudan servis çözmek yerine DI uyumlu bir async filter yazılmalı,
attribute yalnızca filtreyi bağlayan ince bir işaret olmalıdır:

```csharp
[HttpPost]
[Route("Create")]
[Idempotent]
public async Task<ActionResult<IResponseObject>> Create([FromBody] T entity)
```

Filter aşağıdaki akışı uygular:

1. `Idempotency-Key` header'ını doğrular; uzunluk/format için makul sınır koyar.
2. Scope ve request hash üretir.
3. Unique kayıt insert'i ile isteği `Processing` durumunda sahiplenmeye çalışır.
4. Kayıt zaten varsa hash ve state kontrolü yaparak replay, `409` veya `422` döner.
5. Action tamamlanınca yalnızca tekrar üretilebilir başarılı/deterministik istemci hatası cevaplarını
   `Completed` olarak kaydeder.
6. Sunucu hatasında kaydı tamamlanmış saymaz; isteğin güvenle yeniden denenebilmesine izin verir.

Response gövdesi JSON olarak, HTTP durum kodu ve gerekliyse seçilmiş response header'larıyla birlikte
saklanmalıdır. `okResult.Value.ToString()` yeterli değildir; karmaşık nesnelerde gerçek JSON gövdesi
kaydedilmelidir.

## Sınırlar ve ek korumalar

Idempotency yalnızca **aynı idempotency key ile gelen tekrarları** engeller. Aynı kullanıcının iki
farklı key ile mantıksal olarak aynı siparişi oluşturmasını engellemez. Kritik domain kuralları için
veritabanında ayrıca doğru unique constraint/index bulunmalıdır (ör. kullanıcı + dış sipariş referansı).

TTL ilk aşamada 24 saat olabilir. Süresi geçmiş kayıtların arka plan job'ı ile temizlenmesi gerekir.
Key'ler, body'ler ve özellikle response gövdeleri loglara açık biçimde yazılmamalıdır; hassas veriler
içerebilir.

## Önerilen uygulama sırası

1. `ResponseObject` için açık sonuç türü ve `MainController<T>.Create` için doğru HTTP durum kodları.
2. `IdempotencyRecord` entity/migration ve `(Scope, Key)` unique index.
3. DI uyumlu `[Idempotent]` async filter ve persistent store.
4. `MainController<T>.Create` üzerinde opt-in kullanım ve entegrasyon testleri.
5. TTL temizliği, observability/metric'ler ve gerekirse Redis optimizasyonu.

Bu altyapı önce create endpoint'lerinde kullanılmalıdır. Ödeme, dış servis çağrısı ve bulk create gibi
kritik komutlar daha sonra ayrı ayrı opt-in olarak eklenebilir.
