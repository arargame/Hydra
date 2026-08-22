# Per-Table Database Routing

Running one table against a different database from the rest.

## Why

Not every table lives in the application's main database. `Log` is the worked example: Tentacle
writes logs to a separate `TentacleLogDatabase` through `ILogDbWriterService`, which resolves its
connection by name:

```csharp
var connection = connectionFactory("LogDbConnection");
```

Reads, however, all went through `TableService`, which was constructed with a single
`DefaultConnection`. So logs were written to one database and read from another. The symptom was a
SQL error that looks like a schema problem but is not:

```
Invalid column name 'EntityType'.
```

Note the wording — *invalid column*, not *invalid object*. The `Log` table existed in the main
database too (an older, narrower copy), so the query found a table and failed on its columns. If you
ever see a column error for a table you believe is correct, check **which database** the query
reached before you go looking at migrations.

## The mechanism

`TableService` can resolve a connection from the table name. The mapping comes from configuration:

```json
"Hydra": {
  "TableConnections": {
    "Log": "LogDbConnection"
  }
}
```

The value is a **connection name**, resolved through the same
`Func<string, MsSqlConnection>` factory used elsewhere — so it reads from `ConnectionStrings`, user
secrets, or environment, exactly like `DefaultConnection` does.

Registration (`Hydra.WebApi/Extensions/ServiceCollectionExtensions.cs`):

```csharp
return new TableService(
    defaultConnection: connection,
    connectionByName: name => connectionFactory(name),
    connectionNameForTable: tableName => config.Get($"Hydra:TableConnections:{tableName}"));
```

## Behaviour and safety

- A table **not** listed uses the default connection. Existing behaviour is unchanged by default.
- The original single-connection `TableService(IDbConnection)` constructor still exists, so a host
  that has not opted in keeps working.
- `ResolveConnection` falls back to the default connection if resolution throws. A typo in the
  mapping degrades one table; it cannot take down every query in the application.

## Limits

Routing is per table, applied when the query is executed. That means:

- **No cross-database joins.** If a table is routed elsewhere, everything the query touches must live
  there too. `LogDTO` deliberately declares no navigations for this reason.
- EF is not involved in this path. `Select` runs through `TableService` → `QueryBuilder` → ADO.NET, so
  a routed table does not need a `DbSet<T>` for reading. It *does* need one if anything constructs a
  `Repository<T>` for it, because `Repository`'s constructor calls `context.Set<T>()`.
