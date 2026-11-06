# Hydra: Enterprise Full-Stack Application Framework

> **Build robust, secure, and metadata-driven enterprise web applications in .NET 9 and Blazor with zero repetitive boilerplate.**

---

## What is Hydra?

**Hydra** is a high-productivity full-stack enterprise framework built for the modern .NET ecosystem. It bridges the gap between database models, RESTful APIs, and Blazor user interfaces through a contract-driven, stateless architecture.

Instead of writing repetitive controllers, services, repositories, DTO mappers, Blazor pages, filters, and forms for every single entity, Hydra provides an intelligent, metadata-driven engine:

```
[ Domain Entity + ViewDTO ] 
           │
           ▼
[ Automatic REST WebApi + 5-Layer Security + Audit Logs + In-Memory Cache ]
           │
           ▼
[ Dynamic Blazor UI: HydraGrid + GenericFormView + Live Client Validation ]
```

---

## Why Hydra?

### 1. Zero-Boilerplate Full-Stack CRUD
In traditional enterprise applications, introducing a new entity requires writing hundreds of lines across multiple layers:
- Entity & Migrations
- Repository & Custom Queries
- Service & Business Orchestration
- Controller Endpoints (Get, Post, Put, Delete, List)
- Frontend DTOs & Mappings
- Blazor List Grid, Details View, and Create/Edit Forms
- Custom CSS Grid Layouts

**With Hydra:**
You define your `BaseObject<T>` entity and a declarative `ViewDTO`. Hydra automatically generates:
- Full RESTful CRUD endpoints (`MainController<T>`)
- High-performance, paginated, multi-filtered grids (`HydraGrid`)
- Form engines with 12-column responsive layouts (`GenericFormView`)
- Real-time client-side and server-side validation

### 2. Metadata-Driven & Truly Stateless Architecture
The Blazor frontend never hardcodes database structure, column widths, or filter types.
- The backend serves a rich **`TableDTO`** containing **`MetaColumnDTO`** definitions.
- The UI dynamically renders the appropriate input primitives (`DropdownList`, `TextArea`, `DatePicker`, `Switch`, `Number`, `File`).
- Users can dynamically reorder, hide, or display columns via the column chooser modal without touching the database schema.

### 3. Enterprise Security by Design (OWASP & CWE-209 Compliant)
Security is baked into the foundation, not bolted on as an afterthought:
- **Prevention of Information Exposure (CWE-209):** Database topology, raw SQL errors, and constraint names (`FK_*`, `UQ_*`) are never leaked to end users or API responses. They are translated into safe, universal human-readable messages while full technical diagnostics are stored in secure developer logs.
- **5-Layer Granular Authorization:** Security boundaries protect endpoints (`ControllerActionBased`), navigation (`NavMenuBased`), pages (`ViewBased`), buttons (`ComponentBased`), and columns (`EntityPropertyBased`). Unauthorized columns are removed from metadata AND redacted from JSON payloads before hitting the wire.

### 4. Dual-Shield DRY Validation
Thanks to C# running on both the backend and Blazor client:
- Validation rules are declared once on the entity using compile-time safe `nameof(...)` syntax and `IValidatableObject`.
- **Client-Side Shield:** Executed instantly in the browser on clicking Save/Create. Invalid fields receive immediate visual error highlights with zero network traffic.
- **Server-Side Shield:** Executed in `Service<T>.CreateAsync` and `UpdateAsync`. Rejects any invalid API calls before touching the database transaction.
- **Localization-Ready:** Built-in hooks format messages via pluggable localization dictionaries.

### 5. Automated Deep Audit Logging & Change Tracking
- Every insert, update, and delete is automatically captured.
- On updates, `Repository<T>` tracks only the modified properties (`GetModifiedProperties()`), storing before-and-after values along with timestamp, user ID, and correlation ID.

### 6. Pluggable Enterprise Modules
Hydra features an expanding ecosystem of plug-and-play modules:
- **Hydra.AccessManagement:** Roles, Permissions, SystemUsers, JWT token issuance, and granular permission scanners.
- **Hydra.MailManagement:** FluentEmail integration, SMTP delivery, dynamic mail templates, and attachment handling.
- **Hydra.CacheManagement:** High-throughput in-memory LRU caching with automatic entity lifecycle eviction.

---

## Ecosystem Architecture

```mermaid
flowchart TD
    subgraph UI ["Frontend: Hydra.RazorClassLibrary"]
        HG["HydraGrid (Sorting / Paging / Filtering)"]
        GFV["GenericFormView (Create / Edit Form Engine)"]
        GDV["GenericDetailsView"]
        CCM["ColumnChooserModal"]
        CV["Client-Side IValidatableObject Validation"]
    end

    subgraph API ["Transport: Hydra.WebApi"]
        MC["MainController<T>"]
        PAF["PermissionAuthorizationFilter"]
        PS["PropertySecurity (Column Redaction)"]
        SN["StatusNotifier & Status Catalog"]
    end

    subgraph CORE ["Core: Hydra"]
        BO["BaseObject<T>"]
        SRV["Service<T> (CQRS Command / Query Partials)"]
        REP["Repository<T> (Change Tracker / Audit Log)"]
        UOW["UnitOfWork (Transactions / Concurrency Retry)"]
        VAL["ValidationManager (Universal Templates / i18n)"]
        SEH["SqlExceptionHelper (CWE-209 Security Shield)"]
        LRU["CacheService (LRU In-Memory Cache)"]
    end

    subgraph REF ["Reference Application: Tentacle"]
        TNT["Tentacle Work Orchestration Platform"]
    end

    UI <--> |HTTP / JSON (TableDTO + ResponseObject)| API
    API --> CORE
    REF --> UI
    REF --> API
    REF --> CORE
```

---

## Feature Comparison: Traditional .NET vs. Hydra

| Feature | Traditional .NET Enterprise | Hydra Framework |
|---|---|---|
| **Adding a New Entity CRUD** | ~8-12 files (DTO, Mapper, Controller, Service, Repo, 4 Blazor pages) | **1 Entity + 1 ViewDTO class** |
| **Grid Filtering & Sorting** | Manual LINQ queries & custom UI controls | **Automatic (Dynamic Contains, Range, Equals filters)** |
| **Form Layouts** | Manually styled HTML rows/cols | **Automatic 12-column responsive grid with type deduction** |
| **Validation Rules** | Duplicated in JS/Blazor and FluentValidation | **Declared once on entity (`IValidatableObject`), runs on both** |
| **Database Error Exposure** | Raw SQL/Constraint leaks (CWE-209 risk) | **Sanitized user messages + Isolated developer logs** |
| **Column-Level Security** | Complex custom authorization code | **Automatic metadata redaction & payload stripping** |
| **Audit Trails** | Manual trigger or custom EF interceptors | **Built-in `ChangeTracker` logging modified properties** |
| **Styling & Theming** | Hardcoded CSS classes | **Framework-agnostic semantic CSS (`hydra-*`) + Theme overrides** |

---

## Quick Example: Creating an Entity

### 1. Declare the Entity
```csharp
public class Employee : BaseObject<Employee>
{
    public Guid PositionId { get; set; }
    
    [ForeignKey("PositionId")]
    public Position? Position { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
            yield return result;

        if (string.IsNullOrWhiteSpace(Name))
            yield return ValidationManager.Required(nameof(Name), "Full Name");

        if (PositionId == Guid.Empty)
            yield return ValidationManager.MustBeSelected(nameof(PositionId), "Position");
    }
}
```

### 2. Declare the ViewDTO
```csharp
[RegisterAsViewDTO("Employee")]
public class EmployeeDTO : ViewDTO
{
    public Guid PositionId { get; set; }

    public override DTO LoadConfigurations()
    {
        SetConfigurationsViaStringPropertyInfo(
            propertyInfo: ReflectionHelper.GetPropertyOf<EmployeeDTO>(x => x.Name),
            displayName: "Full Name",
            attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
        );

        SetConfigurationsForNavigations(
            leftTableKeyName: "PositionId",
            rightTableName: "Position",
            columnNameToDisplay: "Name",
            displayName: "Position"
        );

        SetConfigurationsForBaseObjectMembers();
        return this;
    }
}
```

### 3. Result
- WebApi endpoints: `/api/Employee/Create`, `/api/Employee/Update`, `/api/Employee/Delete`, `/api/Employee/GetTable`
- Blazor pages: Interactive, paginated grid with instant filtering, column selection, responsive form, and real-time validation highlights.

---

## Documentation Links

- [Quality & Engineering Standards](QUALITY_STANDARDS.md) — Architectural invariants, CWE-209 compliance, and design patterns.
- [Hydra Academy](Academy/README.md) — Comprehensive chapter-by-chapter book documenting the framework from the ground up.
- [Academy HTML Book](Academy/index.html) — Interactive single-page documentation app.
- [Reference Application: Tentacle](../Tentacle/README.md) — Real-world production implementation of Hydra.
