# Hydra Engineering & Quality Standards

This document establishes the mandatory architectural, security, and quality standards for all components within the Hydra ecosystem (`Hydra.Core`, `Hydra.WebApi`, `Hydra.RazorClassLibrary`, and consuming reference applications such as `Tentacle`).

---

## 1. Core Engineering Principles

All code contributed to Hydra must strictly satisfy:

- **SOLID Principles:**
  - **Single Responsibility (SRP):** Classes and components do one job. Repositories handle data access, Services orchestrate business rules, Controllers handle transport/routing, and View Components render UI.
  - **Open/Closed (OCP):** Base classes (`BaseObject<T>`, `Service<T>`, `Repository<T>`) are extensible through overrides and hooks without modifying core behavior.
  - **Liskov Substitution (LSP):** Derived entities and custom services preserve the base interface invariants.
  - **Interface Segregation (ISP):** Distinct interfaces for distinct needs (`IHasId`, `IHasAuditFields`, `IBaseObject<T>`, `IValidatableObject`, `IUnitOfWork`).
  - **Dependency Inversion (DIP):** High-level modules depend on abstractions (`IService<T>`, `IRepository<T>`, `ILogService`), resolved via DI.
- **DRY (Don't Repeat Yourself):** Shared metadata (`TableDTO`, `MetaColumnDTO`), shared validation rules (`IValidatableObject`), and shared styles are written once and utilized across both backend and frontend.
- **YAGNI (You Aren't Gonna Need It):** No premature abstractions or speculative code. Build features based on concrete operational requirements.

---

## 2. Security Standards (OWASP & CWE Compliance)

### 2.1 Prevention of Information Exposure (CWE-209 / OWASP Top 10)
> **MANDATORY INVARIANT:** Internal database topology, table names, column names, raw SQL exceptions, and constraint identifiers (`FK_*`, `UQ_*`, `PK_*`) MUST NEVER be transmitted to end users, external API responses, or client-side UI error banners.

1. **Strict Separation of Concerns:**
   - **Developer Logs (`Log` Table):** Detailed technical exceptions, inner exceptions, SQL error numbers (e.g. 547, 2601), and constraint names are captured and saved securely via `ILogService`.
   - **User Responses (`ResponseObject`):** Sanitized, human-readable, universal messages are provided via `SqlExceptionHelper.ToSafeUserMessage(ex)`.
2. **Standard Safe User Messages:**
   - Foreign key violations on insert/update: `"A selected related record does not exist or has been removed."`
   - Foreign key violations on delete: `"This record cannot be deleted because it is currently referenced by other records."`
   - Unique/duplicate key conflicts: `"A record with this information already exists."`
   - Deadlocks and database timeouts: `"The system is currently busy. Please try again in a few moments."`

### 2.2 Five-Layer Granular Authorization (Academy 5.5)
Security must be enforced across five concentric boundaries:
1. **Controller / Action Layer:** Deny-by-default permission checks (`PermissionAuthorizationFilter`).
2. **Navigation Menu Layer:** Menus hide unauthorized sections before navigation.
3. **View / Page Layer:** Route guards prevent direct URL traversal to unauthorized CRUD views.
4. **Component / Button Layer:** Action buttons (Edit, Delete, Create) are hidden or disabled dynamically.
5. **Property / Column Layer (`PropertySecurity`):** Columns denied to a user are redacted from metadata AND stripped from JSON payloads, preventing wire sniffing.

### 2.3 Authentication & Session Integrity
- Stateless JWT bearer token authentication with strong signing keys.
- Claims-based user identification (`CurrentUserId`, `SessionContext`).
- Platform isolation via `PlatformId` header validation.

---

## 3. Validation Standards (Defense-in-Depth)

Validation is governed by a **Dual-Layer Shield**:

1. **Client-Side Shield (`GenericFormView.razor`):**
   - Blazor executes entity validation (`IValidatableObject.Validate`) and `IsRequired` checks directly in the browser before dispatching an HTTP request.
   - Prevents invalid payloads from generating unnecessary network traffic, server CPU cycles, or database transaction rollbacks.
   - Invalid fields receive high-contrast red borders (`hydra-form__field--invalid`) and field-level error messages (`hydra-form__field-error`).
2. **Server-Side Shield (`Service<T>.CreateAsync / UpdateAsync`):**
   - Every service command verifies `entity.IsValid(out var validationResults)` before calling the repository.
   - API calls bypassing the UI (e.g. cURL, Swagger, scripts) are rejected with HTTP 400 and validation envelopes.
3. **Compile-Time Safety & Localization:**
   - Property names inside `Validate()` must use `nameof(...)` syntax (e.g., `nameof(PositionId)`).
   - Messages are formatted via `ValidationManager.FormatMessage` with a pluggable `Localizer` delegate for seamless multi-language translation.

---

## 4. Performance & Data Integrity Standards

1. **Stateless Table Architecture:**
   - `TableDTO` acts as the single source of truth between server and UI. The UI maintains zero hardcoded database structures.
2. **Unit of Work & Concurrency:**
   - Multi-operation workflows are enclosed in `IUnitOfWork.CommitAsync()` with automatic retry logic for transient concurrency conflicts (`DbUpdateConcurrencyException`).
3. **Deep Change Tracking & Audit Logging:**
   - `Repository<T>` automatically tracks modified properties on update (`GetModifiedProperties()`) and logs full historical audit payloads.
4. **LRU In-Memory Cache:**
   - High-read entities leverage `ICacheService` with automatic invalidation on update and delete.

---

## 5. Documentation & Code Cleanliness

1. **Living Architecture Documentation:**
   - Every architectural component must be mirrored in `Hydra/Academy/` (the book) and `Hydra/Docs/` (task notes).
   - No dead documentation: code paths in docs must reference exact filenames and line conventions.
2. **Code Commenting & Commit Standards:**
   - Code, inline comments, and Git commit messages must be written in professional English.
   - User conversations and implementation plans are delivered in Turkish as per project conventions.
