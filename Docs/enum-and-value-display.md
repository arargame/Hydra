# Enum and Value Display

Why grids used to show `0`, `1`, `2`, and where a human-readable label belongs.

## The problem

`TableService`/`QueryBuilder` read rows straight from the database. An enum column comes back as its
underlying value — `0`, `1`, `2` — or, if the writer serialised it, as the member name. Rendering the
raw cell value therefore produced meaningless numbers.

Nothing was missing from the metadata: `MetaColumnDTO` already carried

- `ValueType` (`ColumnValueType.Enum`, `.Boolean`, …), and
- `PropertyTypeName` (e.g. `"LogProcessType"`)

so the client had everything needed to resolve a label. It just was not doing it.

## Where the conversion lives

`LookupService` (Blazor side) owns it, and every grid and details screen goes through it:

```csharp
Lookup.GetDisplayText(column, rawValue)
```

- `ColumnValueType.Enum` → `GetEnumDisplayText(column.PropertyTypeName, raw)`
- `ColumnValueType.Boolean` → `Yes` / `No`
- anything else → unchanged

Putting it in one place means the same text appears in the grid, the details summary, the
create/edit dropdown and the list filter — because they all read from `GetEnumOptions`.

## Labels come from the enum itself

`GetEnumMemberLabel` resolves, in order:

1. `[Display(Name = "...")]` on the member
2. `[Description("...")]` on the member
3. the member name

```csharp
public enum RequestStatus
{
    [Display(Name = "In Progress")]
    InProgress,
    ...
}
```

So renaming a label is a one-line change on the enum, and it propagates everywhere. This approach is
carried over from GedenLines, whose `Helper.EnumInformation(..., getDisplayName: true)` did the same
thing.

## Where NOT to put a label

An attribute lives in the assembly that declares the enum. A label on an enum in the **shared Hydra
core** is imposed on every application that uses Hydra, and no host application can override it —
you cannot attach an attribute to another assembly's type.

So:

- Enums in `Hydra` (`LogType`, `LogProcessType`, …) stay **unlabelled**; their member names are the
  neutral default.
- Enums owned by an application (`RequestStatus`, `RequestPriority`, …) carry that application's
  labels.

This is the same reasoning that keeps CSS out of the component kit — see [theming.md](theming.md).

## The value may arrive as an int *or* as a name

Do not assume one. In this ecosystem both happen:

- `Repository`/EF writes enums as their underlying `int`.
- `LogDbWriterService` writes `log.Type.ToString()` — the **member name**.

`GetEnumDisplayText` handles three cases, in order: match the raw value against the option keys
(int), against the option labels (already resolved), and finally parse it as an enum member name and
match on the resulting int. That last step matters once a label differs from the member name —
`"InProgress"` will not be found among labels that read `"In Progress"`.

If the enum type cannot be found in the loaded assemblies, the raw value is returned unchanged. Data
is never lost to a failed lookup.

## Search follows the displayed text

`HydraGrid`'s quick-search matches against `CellText(...)`, not the raw value — so typing `Error`
finds rows whose stored value is `1`. Keep that in mind if you add another cell renderer: search and
display should read from the same function.
