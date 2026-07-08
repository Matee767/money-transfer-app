# Money Transfer — notes for Claude Code

Full-stack money-transfer simulation: .NET 8 Web API (`backend/`), React + Vite +
TypeScript SPA (`frontend/`), MS SQL Server, all orchestrated by `docker-compose.yml`.

## Commands

```bash
# Full stack (requires Docker)
docker compose up --build          # app on :3000, Swagger on :8080/swagger

# Backend
cd backend
dotnet build                       # builds API + tests via MoneyTransfer.sln
dotnet test                        # xUnit tests (EF InMemory, no DB needed)

# Frontend
cd frontend
npm install
npm run dev                        # dev server on :5173, proxies /api to :8080
npm run build                      # type-checks (tsc -b) then bundles
```

## Architecture rules

- **Controllers are thin.** All business rules (validation, balance checks,
  concurrency handling) live in `backend/src/MoneyTransfer.Api/Services/TransferService.cs`.
  Add new business logic as services registered in `Program.cs`, not in controllers.
- **DTOs in `Contracts/`, entities in `Models/`.** Never return EF entities from
  controllers.
- **Money invariants** are enforced in three layers — keep all of them intact when
  changing transfer logic:
  1. `TransferService` validates and retries on `DbUpdateConcurrencyException`
     (accounts carry a `rowversion` token);
  2. one `SaveChanges` call = one atomic DB transaction per transfer;
  3. `CK_Accounts_Balance_NonNegative` check constraint in `AppDbContext`.
- **Schema changes:** the schema is created via `EnsureCreated` on startup
  (`Data/DbInitializer.cs`). If you introduce EF migrations, replace `EnsureCreated`
  with `Database.Migrate()` — they cannot be mixed on an existing database
  (`EnsureCreated` databases have no migrations history table; the volume must be
  reset with `docker compose down -v`).
- **API error contract:** failures return RFC 7807 problem details with the status
  mapping defined in `TransfersController`. Keep new endpoints consistent with it.
- **Frontend API calls** go through `frontend/src/api.ts` only, and always via the
  relative `/api` path (nginx proxies it in Docker, vite proxies it in dev). Never
  hardcode a backend host in components.

## Gotchas

- Tests use the EF **InMemory** provider: SQL-Server-only behaviors (the check
  constraint, real rowversion generation) are not exercised by tests — don't rely
  on tests alone when touching those.
- `frontend/Dockerfile` uses `npm ci`, so `package-lock.json` must stay committed
  and in sync with `package.json`.
- The SA password is deliberately hardcoded (task requires zero-config startup).
