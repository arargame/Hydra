# View Configurations

How a column decides which screens it appears on, and in what shape.

## The view types

`ViewType` (declared in `DTOs/ViewDTOs/ViewDTO.cs`):

| View | Screen |
|---|---|
| `ListView` | the entity's own full list screen (`/Request`) |
| `CollectionView` | the same entity shown as a **child** grid inside another entity's details screen |
| `DetailsView` | the key/value summary on a details screen |
| `CreateView` / `EditView` | the form |
| `LookupView` | the option list behind a foreign-key picker |

A `ViewDTO.LoadConfigurations()` declares, per property, one configuration object per view the
property should appear in:

```csharp
SetConfigurationsViaPropertyInfo(
    propertyInfo: ReflectionHelper.GetPropertyOf<LogDTO>(x => x.ProcessType),
    configurations: new List<IConfiguration>
    {
        new ListViewConfiguration(
                toSelect: new AttributeToSelect(1),
                toFilter: new AttributeToFilter(nameof(EqualFilter)),
                toOrder:  new AttributeToOrder(isOrderable: true),
                elementType: HtmlElementType.DropdownList)
            .AlsoUseToCreateCollectionViewConfiguration(),

        new DetailsViewConfiguration()
    },
    displayName: "Operation");
```

## There is no fallback between view types

`TableDTO.PrepareUsingConfigurations` asks
`ViewDTOConfigurationCacheManager.GetOrLoad(viewDTOType, tableName, ViewType)` for the configurations
of **that view type only**. A column with no configuration for the requested view simply is not in
the result.

So if a collection tab renders an empty grid, the first thing to check is whether that entity's DTO
declares any CollectionView configurations at all — not whether the query returned rows.
`CollectionViewSection` has a `UseListViewColumns="true"` escape hatch that requests `ListView`
columns instead, as a stopgap for DTOs that have not been given a collection view yet.

## Deriving CollectionView and LookupView from ListView

You rarely write a `CollectionViewConfiguration` by hand. A `ListViewConfiguration` can be marked to
produce derived copies:

```csharp
new ListViewConfiguration(...)
    .AlsoUseToCreateCollectionViewConfiguration()
    .AlsoUseToCreateLookupViewConfiguration()
```

`ViewDTO` then clones the ListView configuration into the other view types
(`ConvertToAnotherViewConfiguration`). Pass `thenRemoveThis: true` if the column should appear in the
derived view but *not* in the list view.

The `SetConfigurationsVia*PropertyInfo` helpers do this for you, with
`useToCreateCollectionViewConfiguration` / `useToCreateLookupViewConfiguration` parameters
(default `true`).

> **Trap, now fixed:** `SetConfigurationsViaEnumPropertyInfo` was the only helper without those
> parameters. Every enum column — `Request.Status`, `Log.Type`, `Log.ProcessType` — therefore had no
> CollectionView configuration and silently vanished from every master-detail tab. If you add a new
> helper, make sure it offers the same switches.

## Deciding what a collection tab shows

A child grid inside a details screen usually wants a **narrower** column set than the entity's own
list screen. That is the entire reason `CollectionView` exists as a separate view type. Declare the
wide set on ListView and mark only the useful columns for the collection view:

```csharp
// Shown in both the full list and the collection tab
new ListViewConfiguration(toSelect: new AttributeToSelect(0), ...)
    .AlsoUseToCreateCollectionViewConfiguration(),

// Shown only on the full list screen — noise inside a tab
new ListViewConfiguration(toSelect: new AttributeToSelect(4), ...),
```

`LogDTO` is the worked example: ListView shows date, operation, type, message, source, method; the
collection tab shows only date, operation, type, message.

## Column order

Order comes from `AttributeToSelect(priority)`; the grid sorts visible columns by `Priority`
ascending. The priority-only `ListViewConfiguration(int priority, ...)` overload creates the
`AttributeToSelect` for you.

## Filterable but not visible

Pass `toSelect: null` to get a column that can be filtered on but is never selected — it produces a
`FilteredColumn` and no `SelectedColumn`. Combine with
`new AttributeToFilter(..., createFilterComponentFromThis: false)` so the filter bar does not render
an input for it either.

This is how the automatic Logs tab filters `Log` by `EntityType` and `EntityId` without showing those
two columns to the user, and without offering them as filter boxes.

## Default sort

`AttributeToOrder.IsOrdered` is computed — it is `true` whenever `SortingDirection` is non-null. So
supplying a direction is what makes a column the default sort:

```csharp
toOrder: new AttributeToOrder(isOrderable: true, sortingDirection: SortingDirection.Descending)
```

There is no separate `isOrdered` parameter; passing one will not compile.

## Filters with no value are dropped

`MetaColumnDTO.ConvertToColumn` skips any filtered column whose `FilterDTO.Parameters` is empty. A
configured-but-unset filter costs nothing at query time — only filters carrying values reach the SQL.
