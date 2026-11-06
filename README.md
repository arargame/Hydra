# Hydra Framework

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%2F%20Server-purple.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Architecture: Clean](https://img.shields.io/badge/Architecture-Clean%20%26%20Stateless-emerald.svg)](PRODUCT_OVERVIEW.md)
[![Security: CWE--209](https://img.shields.io/badge/Security-CWE--209%20Compliant-orange.svg)](QUALITY_STANDARDS.md)

**Hydra** is a metadata-driven, full-stack application framework for .NET 9 and Blazor. It unifies backend domain modeling, automated REST APIs, and responsive Blazor UI components through a contract-driven, stateless architecture.

---

## Quick Navigation

- 📖 **[Product Overview & Why Hydra?](PRODUCT_OVERVIEW.md)** — Architectural vision, problem-solution matrix, and core capabilities.
- 🛡️ **[Quality & Engineering Standards](QUALITY_STANDARDS.md)** — SOLID invariants, CWE-209/OWASP information disclosure prevention, dual-shield validation, and coding standards.
- 🎓 **[Hydra Academy](Academy/README.md)** — Comprehensive chapter-by-chapter book documenting the framework from the ground up.
- 🌐 **[Hydra Academy Interactive HTML](Academy/index.html)** — Single-page offline documentation web app.
- 🐙 **[Reference Application: Tentacle](../Tentacle/README.md)** — Enterprise work orchestration platform built on Hydra.

---

## What Hydra Delivers

1. **Zero-Boilerplate Full-Stack CRUD:** Declare an entity (`BaseObject<T>`) and a declarative `ViewDTO`; Hydra automatically provides full REST endpoints, paginated & filterable grids (`HydraGrid`), and responsive 12-column form views (`GenericFormView`).
2. **Metadata-Driven UI (`TableDTO` / `MetaColumnDTO`):** Truly stateless UI that dynamically configures input types, dropdowns, ranges, and user preferences with zero hardcoded front-end database models.
3. **Dual-Shield Validation (DRY & C# Shared):** The exact same C# `IValidatableObject` rules run on both the Blazor client (instant zero-network UI feedback) and the WebApi backend (tamper-proof validation).
4. **Security by Design (CWE-209 & 5-Layer Authorization):** Raw database exceptions and internal schema details are strictly quarantined to developer logs, returning safe, universal human-readable messages to end users. Includes 5-layer granular authorization down to column-level data redaction.
5. **Deep Change Tracking & Auditing:** Automated audit logs record modified properties on update (`GetModifiedProperties()`), before/after values, timestamps, and user IDs.
6. **Pluggable Modular Ecosystem:** Pre-integrated modules for Access Management (JWT, Permissions), Mail Management (FluentEmail, SMTP), and In-Memory LRU Caching.

---

## Core Repositories & Solution Structure

Hydra is organized into three decoupled repositories:

| Repository / Project | Role | Description |
|---|---|---|
| **`Hydra` (Core)** | Domain & Data Layer | `BaseObject<T>`, `Repository<T>`, `Service<T>`, `ValidationManager`, `SqlExceptionHelper`, DAL, CQRS partials |
| **`Hydra.WebApi`** | Transport & Security | `MainController<T>`, `PermissionAuthorizationFilter`, `PropertySecurity`, Session context |
| **`Hydra.RazorClassLibrary`** | Blazor UI Engine | `HydraGrid`, `GenericFormView`, `GenericDetailsView`, `FilterBarComponent`, dynamic column chooser |
| **`Tentacle`** | Reference Platform | Real-world enterprise work orchestration application powered by Hydra |

---

## Low-Level SQL Query Generation (Legacy Core Engine)

Hydra originally began with a fluent, programmatic SQL query generation engine. That engine continues to power high-performance bulk operations and raw ADO.NET table views alongside Entity Framework Core:

```csharp
var table = new Table(name: "Person.Person", alias: "pper")
    .SetMetaColumns(new SelectedColumn(name: nameof(PersonPerson.FirstName)),
                    new SelectedColumn(name: nameof(PersonPerson.LastName)),
                    new OrderedColumn(name: nameof(PersonPerson.FirstName), direction: "desc"),
                    new FilteredColumn(name: nameof(PersonPerson.FirstName),
                        filter: new ContainsFilter(value: "ia")))
    .SetJoins(t => new List<IJoinTable>()
        {
            new JoinTable("Person.PersonPhone","pphn",JoinType.Inner)
                .SetLeftTable((Table?)t)
                .On(nameof(PersonPerson.BusinessEntityId), nameof(PersonPhone.BusinessEntityId))
                .SetMetaColumns(new SelectedColumn("PhoneNumber", "PersonPhone.PhoneNumber", false))
        })
    .SetPageNumber(1)
    .SetPageSize(10);
```

---

## Contributing & License

Hydra is an open-source framework distributed under the [MIT License](LICENSE). Contributions, architecture discussions, and issues are welcome via GitHub.
