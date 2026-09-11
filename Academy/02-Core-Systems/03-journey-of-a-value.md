# 2.3 — A Journey of a Value: Bir "a" Harfinin Yolculuğu

Bu bölümün amacı farklı: önceki bölümler katman katman anlattı, bu bölüm **tek
bir değeri** baştan sona takip ediyor. Sahne: bir kullanıcı, Product listesinin
filtre kutusuna `Name` alanı için `a` harfini yazıyor, Enter'a basıyor. O tek
karakter, ekrandan veritabanına, veritabanından ekrana dönene kadar kaç kez
şekil değiştiriyor? Hangi sınıflardan geçiyor? Nerede bir SQL parametresine
dönüşüyor?

```mermaid
sequenceDiagram
    autonumber
    participant Input as &lt;input&gt; (DOM)
    participant FBC as FilterBarComponent
    participant Col as MetaColumnDTO<br/>(Name kolonu)
    participant Client as ApiClient&lt;Product&gt;
    participant Ctrl as MainController&lt;Product&gt;
    participant Svc as Service&lt;Product&gt;
    participant Conv as MetaColumnDTO.ConvertToColumn
    participant QB as QueryBuilder
    participant Ado as AdoNetDatabaseService
    participant DB as SQL Server

    Input->>FBC: oninput → e.Value = "a"
    FBC->>FBC: SetRawValue(column, 0, "a")<br/>(_rawValues sözlüğünde bekliyor, henüz DTO'ya yazılmadı)
    Input->>FBC: Enter tuşu → OnKeyDown → ApplyFilters()
    FBC->>Col: column.FilterDTO.SetParameters(["a"])
    FBC->>Client: TableChanged → ApiClient.SelectAsync(Table)
    Client->>Ctrl: POST /Product/Select {MetaColumns:[...,{Name:"Name",FilterDTO:{TypeName:"ContainsFilter",Parameters:[{Value:"a"}]}}]}
    Ctrl->>Svc: SelectWithTableAsync(tableDTO)
    Svc->>Conv: her MetaColumnDTO için ConvertToColumn()
    Conv->>Conv: "ContainsFilter" string'i → new ContainsFilter("a")
    Svc->>QB: Table.SetTableFilter() → JoinedFiltersGroup ya da tekil Filter
    QB->>QB: PrepareFilteredColumnsString()<br/>→ "where p.Name like '%'+@0+'%'"
    QB->>QB: Table.SetQueryParameters()<br/>→ {"@0": "a"}
    QB->>Ado: ExecuteQuery(sql, {"@0":"a"}, connection)
    Ado->>DB: parametreli SqlCommand (SQL injection'a kapalı)
    DB-->>Ado: eşleşen satırlar
    Ado-->>QB: List&lt;Dictionary&lt;string,object?&gt;&gt;
    QB-->>Svc: Table.Rows dolu
    Svc-->>Ctrl: TableDTO.FromTableToDTO(table)
    Ctrl-->>Client: ResponseObject.Data = TableDTO
    Client-->>FBC: aynı TableDTO, Rows dolu
    FBC-->>Input: HydraGrid yeniden render, eşleşen satırlar görünür
```

Şimdi bu on yedi adımı, gerçek kodla, tek tek açalım.

## 1–2. Tuş vuruşu: DOM'dan component'e

`FilterBarComponent.razor`, `Name` gibi metin filtreleri için düz bir `<input>`
basıyor:

```razor
<input class="hydra-input" type="@GetInputType(column)"
       value="@GetRawValue(column, 0)"
       @onchange="e => SetRawValue(column, 0, e.Value)"
       @onkeydown="@(e => OnKeyDown(e))" />
```

`@onchange` — her tuşta değil, input odağı kaybettiğinde (blur) ya da Enter'da
tetiklenen Blazor event'i. `SetRawValue` bu anda **henüz `TableDTO`'ya
dokunmuyor** — kendi private `_rawValues` sözlüğüne yazıyor:

```csharp
private void SetRawValue(MetaColumnDTO column, int index, object? value)
{
    var key = FieldKey(column);          // "Product.Name"
    if (!_rawValues.ContainsKey(key)) _rawValues[key] = new string?[2];
    _rawValues[key][index] = value?.ToString();   // "a"
}
```

Bu ayrım kasıtlı: kullanıcı yazarken her karakterde sunucuya gitmek istemiyoruz.
`"a"` şu an sadece component'in kendi hafızasında, henüz kimseye gönderilmedi.

## 3–4. Enter: taahhüt anı

```csharp
private async Task OnKeyDown(KeyboardEventArgs e)
{
    if (e.Key == "Enter") await ApplyFilters();
}

private async Task ApplyFilters()
{
    foreach (var column in FilterColumns)
    {
        var values = _rawValues.TryGetValue(FieldKey(column), out var raw) ? raw : new string?[2];
        if (!string.IsNullOrEmpty(values[0]))
            column.FilterDTO.SetParameters(new List<object?> { values[0] });   // "a" artık DTO'da
        else
            column.FilterDTO.ClearParameters();
    }
    Table.PageNumber = 1;                 // yeni bir filtre = ilk sayfaya dön
    await TableChanged.InvokeAsync(Table);
}
```

Burada `"a"`, geçici `_rawValues`'tan kalıcı `MetaColumnDTO.FilterDTO.Parameters`'a
taşınıyor — artık `TableDTO`'nun bir parçası, bir sonraki istekte gidecek olan
gerçek "state" bu.

## 5–6. Yukarı akış: `HydraGrid` → `GenericListView` → `ApiClient`

`TableChanged.InvokeAsync(Table)`, [2.2](02-components-and-theming.md)'de
anlatılan "parametre aşağı, event yukarı" akışını tetikliyor — `GenericListView`
`OnTableChanged`'ini çağırıyor, o da `Client.SelectAsync(Table)`'a gidiyor.
`ApiClient.SelectAsync` bu `TableDTO`'yu olduğu gibi HTTP gövdesine koyup
`POST /Product/Select?viewType=ListView`'a yolluyor. O anda tel üzerinde giden
JSON'da `"a"` şöyle görünür:

```json
{
  "name": "Product",
  "viewType": "ListView",
  "metaColumns": [
    { "typeName": "FilteredColumn", "name": "Name",
      "filterDTO": { "typeName": "ContainsFilter", "parameters": [ { "value": "a" } ] } }
  ]
}
```

## 7–9. Sunucuda: string'den gerçek bir `Filter` nesnesine

`MainController.Select`, `Service<Product>.SelectWithTableAsync`'e devrediyor.
Orada `TableDTO.ConvertToTable(tableDTO)` her `MetaColumnDTO`'yu
`MetaColumnDTO.ConvertToColumn`'dan geçiriyor — burası JSON'daki `"ContainsFilter"`
**string'inin** gerçek bir `.NET` nesnesine dönüştüğü an:

```csharp
switch (filterTypeName)
{
    case nameof(ContainsFilter):
        column = new FilteredColumn(columnDTO.Name, new ContainsFilter(values!.First()!));
        break;
    // EqualFilter, BetweenFilter, InFilter... aynı desende, her biri ayrı case
}
```

`values.First()` — yani `"a"` artık gerçek bir `ContainsFilter(object value)`
kurucusuna geçen bir parametre. Bu switch, tip adını (reflection/`Activator`
değil, elle yazılmış bir `case` listesi) somut sınıfa eşliyor — yeni bir filtre
tipi eklerseniz bu switch'e de bir `case` eklemeniz gerekiyor, aksi halde o
filtre tipi hiç tanınmaz (bkz. ⭐ aşağıda).

## 10–11. `QueryBuilder`: SQL metnine dönüşüm

`Table.SetTableFilter()` (join yoksa direkt, varsa [`Hydra/Docs/filters.md`](../../Docs/filters.md)'deki
`JoinedFiltersGroup` ağacı üzerinden) filtreyi `_table.Filter`'a koyuyor.
`QueryBuilder.PrepareFilteredColumnsString()`:

```csharp
FilteredColumnsString = _table.Filter != null ? $"where {_table.Filter.PrepareQueryString()}" : null;
```

`ContainsFilter.PrepareQueryString()` (`Hydra/DataModels/Filter/ContainsFilter.cs`):

```csharp
public override string PrepareQueryString()
{
    return $"{Column?.Table?.Alias}.{Column?.Name} like \'%\'+@{StartParameterIndex}+\'%\'";
}
```

Üretilen metin: `p.Name like '%'+@0+'%'`. Dikkat — `%` işaretleri SQL **string
literal'i**, `@0` ise gerçek bir SQL parametresi. `"a"` değerinin kendisi hiçbir
zaman SQL metnine string olarak yapıştırılmıyor; sadece parametrenin **adı**
(`@0`) metne giriyor. Bu, `LIKE '%...%'` deseninin klasik bir SQL-injection
tuzağı gibi görünüp aslında güvenli olmasının sebebi.

## 12. Parametre numaralama: `"a"`'nın `@0`'a bağlanması

`Table.SetQueryParameters()` (`Hydra/DataModels/Table.cs`):

```csharp
public ITable SetQueryParameters()
{
    for (int i = 0; i < Filter.Parameters.Count; i++)
        QueryParameters.Add($"@{i}", Filter.Parameters[i].Value);   // {"@0": "a"}
    return this;
}
```

Tek filtre olduğu için burada basit — `i=0`, `@0` → `"a"`. Birden fazla filtre
aktifse (`JoinedFiltersGroup` devreye girer) bu numaralamanın **kesin bir
invariant'a** uyması gerekiyor; o hikâye [`Hydra/Docs/filters.md`](../../Docs/filters.md)'de
ayrıca anlatılıyor — burada tekrar etmiyoruz, ama "neden `@0` diye sabit
yazmadılar da pozisyona göre üretiyorlar" sorusunun cevabı tam olarak orada.

## 13–15. Veritabanına gidiş: gerçek bir `SqlParameter`

`AdoNetDatabaseService.ExecuteQuery`:

```csharp
var command = connection.CreateCommand();
command.CommandText = query;                 // "...where p.Name like '%'+@0+'%'..."
if (parameters != null) command.AddParameters(parameters);   // {"@0":"a"} → SqlParameter
var dataReader = command.ExecuteReader();
```

Bu noktada `"a"` artık bir C# `string`'den bir ADO.NET `SqlParameter.Value`'ya
dönüştü — SQL Server'a asla "metnin bir parçası" olarak değil, "bağlanmış bir
değer" olarak gidiyor. Sunucu tarafında çalışan gerçek sorgu (basitleştirilmiş):

```sql
select p.* from (
    select p.Id, p.Name, ..., ROW_NUMBER() OVER (ORDER BY p.Id) AS RowNumber
    from Product p
    where p.Name like '%'+@0+'%'
) p where p.RowNumber between 1 and 10
```

## 16–17. Dönüş yolu: satırlardan tekrar `"a"` içeren metne

`QueryBuilder.SetTableRows()` her satırı `Row`/`DataColumn` nesnelerine çeviriyor,
`Table.AddRow`. `Service<Product>`, `TableDTO.FromTableToDTO(table)` ile bunu
tekrar DTO'ya çeviriyor — artık `Rows` dolu bir `TableDTO`. Bu, `Details`
endpoint'inin aksine **hiçbir sarmalayıcı olmadan** doğrudan
`ResponseObject.Data` oluyor (bkz. [1.2](../01-Architecture/02-hydra-webapi.md)),
çünkü `ApiClient.SelectAsync` `ResponseObject.Data`'yı doğrudan `TableDTO`'ya
deserialize ediyor.

`ApiClient.SelectAsync` bu `TableDTO`'yu `GenericListView`'e, o da `HydraGrid`'e
geri veriyor. `HydraGrid` her hücreyi `CellText(row, column)` ile basıyor —
`Lookup.GetDisplayText` üzerinden (bkz. [2.2](02-components-and-theming.md)) —
ve artık ekranda, adında `"a"` geçen `Product` satırları görünüyor. `HydraGrid`'in
**kendi arama kutusu** (`ShowSearchBox`) bu noktadan sonra devreye girerse, o
zaten sunucudan gelen bu satırlar üzerinde, tamamen istemci tarafında, ikinci
bir süzme yapıyor — sunucuya tekrar gitmiyor. İki arama kutusunun (filtre barı
vs. grid'in kendi arama kutusu) neden farklı katmanlarda çalıştığı
[`Hydra/Docs/enum-and-value-display.md`](../../Docs/enum-and-value-display.md#search-follows-the-displayed-text)'te
not edilmişti.

## Yolculuğun özeti — tek tabloda

| Adım | `"a"`'nın o andaki hâli | Tip |
|---|---|---|
| Kullanıcı yazıyor | `"a"` | DOM `<input>` değeri |
| `SetRawValue` | `"a"` | `string?[]` (component içi geçici state) |
| `ApplyFilters` | `"a"` | `FilterParameterDTO.Value` (`object?`) |
| HTTP gövdesi | `"a"` | JSON string |
| `ConvertToColumn` | `"a"` | `ContainsFilter` kurucu parametresi |
| `PrepareQueryString` | *(yok — sadece `@0` adı SQL metnine giriyor)* | SQL metni |
| `SetQueryParameters` | `"a"` | `Dictionary<string,object?>["@0"]` |
| `AddParameters` | `"a"` | `SqlParameter.Value` |
| Sonuç satırları | *(eşleşen `Name` değerleri, `"a"` içeren)* | `DataColumn` → `RowDTO` |
| Ekran | eşleşen satırlar | `HydraGrid` hücre metni |

---

## ⭐ İleride Yapılacaklar / Not Edilenler

- `MetaColumnDTO.ConvertToColumn`'daki filtre-tipi switch'i, yeni bir `Filter`
  alt sınıfı eklendiğinde elle güncellenmesi gereken bir yer. Reflection tabanlı
  bir kayıt (`FilterTypeRegistry`, tıpkı `ViewDTORegistryLoader` gibi) bu riski
  ortadan kaldırabilir — bugün 15 filtre tipi var, unutulan bir `case` sessizce
  o filtrenin hiç uygulanmamasına yol açar (`column` `null` kalır, `break` ile
  çıkılır, hata fırlatılmaz).
- Bu dokümanın "tekil filtre" versiyonu; aynı yolculuğun **iki filtre birden
  aktifken** (`JoinedFiltersGroup` devredeyken) nasıl değiştiğini gösteren bir
  ek şema faydalı olurdu — bugün bu bilgi `Hydra/Docs/filters.md`'de metin
  olarak var ama görselleştirilmedi.
