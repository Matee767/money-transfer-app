# Money Transfer

A small full-stack web application that simulates money transfers between accounts.
Transfers only change account balances stored in the database — no real banking
operations are involved.

**Stack:** .NET 8 Web API · MS SQL Server 2022 · React (Vite + TypeScript) · Docker

## Quick start

The only prerequisite is [Docker](https://docs.docker.com/get-docker/) (with Docker Compose).

```bash
docker compose up --build
```

Then open:

| URL | What |
|-----|------|
| http://localhost:3000 | Web app (accounts, transactions, transfer form) |
| http://localhost:8080/swagger | API documentation (Swagger UI) |

Everything is pre-configured: the database schema is created and demo accounts are
seeded automatically on first start. No manual setup is needed.

> The first launch downloads the SQL Server image (~1.5 GB) and may take a few
> minutes. The API waits for the database to become healthy before starting.

## Architecture

```
┌──────────────┐      ┌───────────────────┐      ┌────────────────┐
│  web (nginx) │ /api │  api (.NET 8)     │      │  db (MS SQL    │
│  React SPA   ├─────►│  ASP.NET Core     ├─────►│  Server 2022)  │
│  port 3000   │proxy │  Web API, EF Core │      │                │
└──────────────┘      └───────────────────┘      └────────────────┘
```

- **web** — React SPA served by nginx; nginx proxies `/api/*` to the API container,
  so the browser talks to a single origin (no CORS in production).
- **api** — ASP.NET Core Web API with EF Core. Creates the schema and seeds demo
  data on startup (with retries while SQL Server boots).
- **db** — SQL Server 2022 (Express edition) with a named volume for persistence.

## API

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/accounts` | All accounts with current balances |
| GET | `/api/transactions` | Completed transfers, newest first |
| POST | `/api/transfers` | Execute a transfer |
| GET | `/health` | Liveness probe |

### POST /api/transfers

```json
{ "fromAccountId": 1, "toAccountId": 2, "amount": 100.50 }
```

Success → `201 Created` with the transaction id and both resulting balances.
Failures return an [RFC 7807 problem details](https://datatracker.ietf.org/doc/html/rfc7807) body:

| Status | Case |
|--------|------|
| `400` | Non-positive amount, or source account equals destination |
| `404` | Source or destination account does not exist |
| `422` | Insufficient funds |
| `409` | Lost an optimistic-concurrency race 3 times (retry the request) |

### Correctness guarantees

- The debit, credit and transaction record are written in **one database
  transaction** — a transfer either fully happens or not at all.
- Every account row carries a **`rowversion` concurrency token**; concurrent
  transfers touching the same account are detected and retried, so no update
  is ever silently lost.
- A database **`CHECK (Balance >= 0)` constraint** guarantees no balance can go
  negative even if application-level checks were bypassed.

## Project structure

```
├── backend/
│   ├── src/MoneyTransfer.Api/
│   │   ├── Controllers/     # HTTP endpoints (thin, no business logic)
│   │   ├── Services/        # TransferService — business rules live here
│   │   ├── Data/            # DbContext, schema config, seeding
│   │   ├── Models/          # Account, TransferTransaction entities
│   │   └── Contracts/       # Request/response DTOs
│   ├── tests/MoneyTransfer.Api.Tests/
│   └── Dockerfile
├── frontend/
│   ├── src/
│   │   ├── components/      # AccountList, TransactionList, TransferForm
│   │   ├── api.ts           # API client
│   │   └── App.tsx
│   ├── nginx.conf           # SPA hosting + /api reverse proxy
│   └── Dockerfile
└── docker-compose.yml
```

## Local development (without Docker)

Requires the .NET 8 SDK, Node.js 20+, and a reachable SQL Server
(the connection string default is in `backend/src/MoneyTransfer.Api/appsettings.json`).

```bash
# API — http://localhost:8080
cd backend
ASPNETCORE_URLS=http://localhost:8080 dotnet run --project src/MoneyTransfer.Api

# Frontend dev server — http://localhost:5173 (proxies /api to :8080)
cd frontend
npm install
npm run dev
```

### Running the tests

```bash
cd backend
dotnet test
```

## Notes

- The SA password in `docker-compose.yml` / `appsettings.json` is intentionally
  committed so the stack runs with zero manual configuration, as required by the
  task. In a real project it would come from a secret store.
- Transfers are a simulation: the only side effect is balance changes in the
  application's own database.
