# FactionAPIServices

_Overview for programmers who don't know C# or ASP.NET_

---

## What is this?

A **REST API backend** for the [Vampirism Minecraft mod](https://github.com/TeamLapen/Vampirism) ecosystem. It provides three main services:

1. **Supporter registry** — tracks players who support the mod, along with their in-game faction (vampire, hunter, etc.) and cosmetic appearance settings
2. **Mod configuration store** — a key-value store for mod settings, fetched by the game client at runtime
3. **Telemetry collection** — anonymous usage stats (Minecraft version, mod version, number of mods)

The API is consumed by the running game clients/servers automatically (e.g., on startup to fetch config or check supporter perks).

---

## Tech Stack — The Quick Translation

| C# / ASP.NET term | What it means in plain terms |
|---|---|
| **ASP.NET Core** | The web framework — like Express (Node), Flask (Python), or Spring (Java) |
| **.NET 10** | The runtime/platform version |
| **Minimal APIs** | Route handlers defined as lambdas/functions — no class boilerplate needed |
| **Entity Framework Core (EF Core)** | ORM — maps C# classes to database rows, like SQLAlchemy or Hibernate |
| **`record` types** | Immutable data structs — value equality, no setters; like frozen dataclasses in Python |
| **`IResult` return type** | The API response object — like `Response` in other frameworks |
| **Middleware** | Request/response pipeline — same concept as Express middleware or Django middleware |
| **Dependency Injection (DI)** | Built-in IoC container — services registered at startup, injected where needed |
| **.NET Aspire** | Local orchestration tool — spins up the app + its dependencies (PostgreSQL) in Docker |
| **Mapperly** | Code-generator for converting between DTOs and database models at compile time |

---

## Project Layout

```
FactionAPIServices/
├── src/
│   ├── FactionAPI.Services/              ← Entry point (Program.cs), startup config
│   ├── FactionAPI.Services.Api/          ← HTTP route handlers + request/response models
│   ├── FactionAPI.Services.Infrastructure/ ← Database models, EF Core context, auth
│   ├── FactionAPI.Services.ServiceDefaults/ ← Shared observability config (OpenTelemetry)
│   └── FactionAPI.Services.Aspire/       ← Local dev orchestration (Docker + DB wiring)
├── .github/workflows/                    ← CI/CD: build on push, publish Docker image on tag
└── FactionAPIServices.slnx              ← Solution file (like a Makefile listing all sub-projects)
```

Think of the 5 sub-projects as **modules with enforced boundaries** — they reference each other like library imports:

```
Aspire → Services → Api → Infrastructure
                  ↑
            ServiceDefaults
```

---

## How a Request Flows Through the Code

```
HTTP Request
    │
    ▼
Program.cs  ←── registers routes, middleware, DI services on startup
    │
    ▼
Auth Middleware  ←── checks Bearer token, attaches identity claims
    │
    ▼
Route Handler (Endpoints.cs)  ←── the actual business logic function
    │
    ▼
EF Core (FactionContext.cs)  ←── translates C# queries → SQL → PostgreSQL
    │
    ▼
HTTP Response (JSON)
```

### Example: GET /api/v1/supporter/list

```csharp
// src/FactionAPI.Services.Api/V1/Endpoints.cs
app.MapGet("/api/v1/supporter/list", async (
    IDbContextFactory<FactionContext> dbFactory,   // injected DB connection pool
    [AsParameters] SupporterListRequest request    // query params bound automatically
) => {
    await using var db = await dbFactory.CreateDbContextAsync();
    var query = db.Supporters.Include(s => s.Appearances).AsQueryable();
    // ... filter by faction, type, etc. ...
    return Results.Ok(supporters.Select(SupporterMapper.ToDto));
});
```

- **`IDbContextFactory`** is injected by the DI container (like `@Inject` in Java)
- **`[AsParameters]`** auto-binds query string params to a struct — no manual parsing
- **`Results.Ok(...)`** returns a `200 OK` with a JSON-serialized body

---

## Database

**PostgreSQL** (TimescaleDB variant for time-series telemetry data).

### Schema

```
Supporter                         SupporterAppearance
──────────────────────────        ─────────────────────────────
PlayerId (UUID, PK)               PlayerId (FK → Supporter)
FactionId (ResourceLocation)      Key  (string)       ← composite PK
Name (string)                     Value (string)
Status (enum: Dev/Contributor/…)
BookId (nullable string)

TelemetryEntry                    ConfigValue
──────────────────────────        ─────────────────────────────
Timestamp (PK)                    Key (ResourceLocation, PK)
Side (enum: Client/Server)        Value (string/JSON)
MinecraftVersion (string)
ModVersion (string)
ModCount (int)

ApiToken
──────────────────────────
Name (string, PK)
Token (SHA-256 hash)
ModIds (string[])
LegacyAll (bool)        ← grants access to all mods
```

**`ResourceLocation`** is a custom type for Minecraft-style `"namespace:path"` identifiers (e.g., `"vampirism:vampire"`, `"vampirism:max_level"`). It's stored as a plain string in the DB but typed in C#.

EF Core migrations handle all schema changes — files in `Infrastructure/Migrations/` are auto-generated diffs, applied on startup.

---

## Authentication

Two independent schemes run in parallel:

### 1. API Token (for game clients / mod tools)
- Client sends `Authorization: Bearer <token>`
- The handler SHA-256 hashes it, looks it up in the `ApiTokens` table
- If found, it stamps the request identity with **claims** (like JWT scopes):
  - `mod_id: vampirism` for each mod the token covers
  - `legacy_all: true` if the token has broad access
- Route handlers check these claims: `user.HasModAccess("vampirism")` before allowing writes

### 2. Admin Token (for admin endpoints)
- Single static secret from `appsettings.json` / environment variable
- Simple string comparison — no DB lookup
- Required for token CRUD (`/api/admin/tokens`)

---

## API Endpoints

### Public / Game Client Endpoints

| Method | Path | What it does |
|--------|------|--------------|
| `GET` | `/api/v1/supporter/list` | List supporters, filterable by faction/type/hasBook |
| `POST` | `/api/v1/supporter/set` | Replace all supporters (bulk upsert) |
| `GET` | `/api/v1/telemetry/basic` | Record one telemetry data point |
| `GET` | `/api/v1/config/get` | Get a single config value by key |
| `GET` | `/api/v1/config/list` | List all config values (optionally filter by modId) |
| `POST` | `/api/v1/config/set` | Upsert multiple config values |
| `GET/POST` | `/api/v0/…` | Legacy versions of supporter + telemetry endpoints |

### Admin Endpoints (require admin token)

| Method | Path | What it does |
|--------|------|--------------|
| `GET` | `/api/admin/tokens` | List all token names |
| `POST` | `/api/admin/tokens` | Create a new API token |
| `DELETE` | `/api/admin/tokens/{name}` | Delete a token |
| `PATCH` | `/api/admin/tokens/{name}` | Add/remove mod IDs, toggle LegacyAll |

---

## Running Locally

**.NET Aspire** acts as a local orchestrator — it launches PostgreSQL in Docker and wires the connection string automatically:

```bash
dotnet run --project src/FactionAPI.Services.Aspire
```

This starts:
1. A TimescaleDB PostgreSQL container with a persistent volume
2. The API service, connected to that DB
3. The Aspire dashboard (observability UI) on `http://localhost:18888`

The API auto-applies EF Core migrations on startup, so no manual `CREATE TABLE` needed.

---

## CI/CD

Two GitHub Actions workflows:

- **`build.yml`** — runs on every push/PR: restores NuGet packages, builds in Release mode
- **`publish.yml`** — runs on git tags: builds a multi-stage Docker image, pushes to GitHub Container Registry (GHCR)

The Dockerfile stages: `sdk` → build → publish (output only) → final runtime image (small `aspnet` base image, non-root user).

---

## Key Files to Read First

| File | Why |
|------|-----|
| `src/FactionAPI.Services/Program.cs` | Where everything is wired up — DI registrations, middleware, route mounting |
| `src/FactionAPI.Services.Api/V1/Endpoints.cs` | The main business logic — all current API handlers |
| `src/FactionAPI.Services.Infrastructure/FactionContext.cs` | Database schema definition in code |
| `src/FactionAPI.Services.Infrastructure/Auth/ApiTokenAuthenticationHandler.cs` | How auth works |
| `src/FactionAPI.Services.Aspire/AppHost.cs` | How the app and its dependencies are orchestrated locally |
