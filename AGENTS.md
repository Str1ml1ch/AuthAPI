# AuthAPI — Agent Instructions

## Project Overview

A .NET 8 ASP.NET Core Web API providing JWT-based authentication and user management, backed by SQL Server + ASP.NET Core Identity.

**Solution projects:**

| Project | Role |
|---|---|
| `AuthAPI` | Web host — controllers, middleware, DI composition root |
| `AuthAPI.Core` | Shared models/DTOs, no EF or HTTP dependencies |
| `AuthAPI.DAL` | EF Core DbContext, Identity, storage classes, specifications, migrations |
| `AuthAPI.Tests` | xUnit + Moq + EF In-Memory unit tests |

**Dependency direction:** `AuthAPI` → `AuthAPI.DAL` → `AuthAPI.Core`. Tests reference both `AuthAPI` and `AuthAPI.DAL`.

---

## Build & Test Commands

```bash
dotnet restore
dotnet build
dotnet run --project AuthAPI/AuthAPI.csproj
dotnet test
```

**EF Migrations** (run from solution root):
```bash
dotnet ef migrations add <Name> --project AuthAPI.DAL --startup-project AuthAPI
dotnet ef database update --project AuthAPI.DAL --startup-project AuthAPI
```

---

## Key Patterns & Conventions

### Storage Pattern (not generic repository)
Each CRUD operation is its own interface + class pair, in its own folder:

```
AuthAPI.DAL/Storage/{Operation}{Entity}/
  I{Operation}{Entity}Storage.cs   ← interface
  {Operation}{Entity}Storage.cs    ← implementation (injects AuthDbContext)
```

Examples: `ICreateUserStorage` / `CreateUserStorage`, `IGetUserByIdStorage` / `GetUserByIdStorage`.

- All async methods accept `CancellationToken ct`
- Implementations project directly to `Core` models inside LINQ (`.Select(u => new UserModel {...})`) — never load entity, then map
- Register all storage classes as `Scoped` in `AuthAPI.DAL/ServiceCollectionExtensions.cs` → `AddServices()`

### Specification Pattern
`ISpecification<T>` exposes `Expression<Func<T, bool>> ToExpression()`. Add new specs under `AuthAPI.DAL/Specifications/{Entity}/`.

### Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| EF entities | `PascalCase` noun | `User`, `ApplicationUser` |
| Identity wrappers | `Application` prefix | `ApplicationUser`, `ApplicationRole`, `ApplicationUserRole` |
| Storage interfaces | `I{Operation}{Entity}Storage` | `IGetUserByIdStorage` |
| Storage implementations | `{Operation}{Entity}Storage` | `GetUserByIdStorage` |
| Core DTOs / read models | `{Entity}Model` | `UserModel` |
| Specifications | `{Entity}By{Field}Specification` | `UserByNameSpecification` |
| EF configurations | `{Entity}Configuration` | `ApplicationUserConfiguration` |
| Request DTOs (in controllers) | `{Action}Request` | `LoginRequest`, `RegisterRequest` |

### EF Entity Configuration
Each entity has its own `IEntityTypeConfiguration<T>` class under `AuthAPI.DAL/Configurations/`, applied automatically via `modelBuilder.ApplyConfigurationsFromAssembly(...)` in `AuthDbContext`.

---

## Authentication & JWT

- Identity: `IdentityDbContext<ApplicationUser, ApplicationRole, ..., ApplicationUserRole>`
- JWT Bearer auth with `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` from `appsettings.json`
- Token claims: `NameIdentifier`, `Email`, one `Role` claim per role, signed with `HmacSha256`
- Endpoints: `POST /api/auth/login`, `POST /api/auth/register`

---

## Testing Conventions

- **Framework**: xUnit + Moq + EF In-Memory
- **`GlobalUsings.cs`**: `global using Xunit;` — do not re-add in test files
- **Controller tests**: instantiate the controller directly; mock `UserManager<ApplicationUser>` via its `IUserStore`; use `ConfigurationBuilder().AddInMemoryCollection(...)` for `IConfiguration`
- **Storage tests**: use `DbHelper` static helper to create an isolated in-memory `AuthDbContext` per test (`UseInMemoryDatabase(Guid.NewGuid().ToString())`); seed both `ApplicationUser` and `User` rows with matching FKs via `DbHelper.SeedUser(ctx, ...)`
- **Test class naming**: `{StorageClass}Tests` (e.g. `GetUserByIdStorageTests`)
- Assert on concrete result types (`OkObjectResult`, `UnauthorizedResult`, `BadRequestObjectResult`); use reflection for anonymous return values

---

## Important Notes

- `DefaultConnection` (SQL Server) is configured in `appsettings.Development.json` or User Secrets — not in the committed `appsettings.json`
- The shared NuGet package `Homework.Ticketing.System.Shared` provides the `BaseDbEntity` base class used by DAL entities
- Redis connection string is under `ConnectionStrings:Redis` (`localhost:6379` by default)
- `ExceptionHandlerMiddleware` catches all unhandled exceptions and returns `500` JSON — do not add redundant try/catch in controllers
