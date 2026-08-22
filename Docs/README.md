# Hydra — Framework Notes

Working notes on how Hydra actually behaves, written while building Tentacle against it. Each page
covers one subsystem: what the mechanism is, the invariants that must hold, and the traps that have
already cost debugging time.

These are not a tutorial. They are the things that are not obvious from reading the code, and that
you would otherwise have to rediscover.

| Page | Covers |
|---|---|
| [view-configurations.md](view-configurations.md) | `ViewType`, per-view column sets, `ListViewConfiguration` and its CollectionView/LookupView derivatives, why a view can come back empty |
| [filters.md](filters.md) | The filter model, how multiple filters combine into a tree, the parameter-numbering invariant that keeps SQL and parameters aligned |
| [enum-and-value-display.md](enum-and-value-display.md) | Why grids used to show `0`/`1`/`2`, how enum labels are resolved, where to put a label and where not to |
| [database-routing.md](database-routing.md) | Running a table against a different database (the `Log` case) |
| [theming.md](theming.md) | The component kit emits semantic classes only; how a host application supplies the look |

## The one-paragraph model

An entity is described once, in a `ViewDTO`, as a set of **configurations**. Each configuration
belongs to a `ViewType` (ListView, CollectionView, DetailsView, CreateView, EditView, LookupView).
When a request arrives, the server picks the configurations matching the requested view type, turns
them into a `TableDTO` of meta columns, builds parameterised SQL from it, executes it, and returns
the same `TableDTO` — now carrying rows plus the filter/sort/paging state. The Blazor side renders
whatever that `TableDTO` describes and posts it back unchanged on the next request. There is one
wire contract and no per-entity UI code.

The corollary, and the source of most surprises: **the column set is resolved per view type, and
there is no fallback between view types.** If a column has no configuration for the view you asked
for, it does not exist in that view.
