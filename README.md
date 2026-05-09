# Tubes_POS_API

Backend API untuk sistem POS (Point of Sale) Food & Beverage berbasis ASP.NET Core Web API.

## Overview

Project ini sudah berkembang dari skeleton awal menjadi backend POS yang mencakup:

- menu management
- transaction cart flow
- payment processing
- transaction history and report
- health checks
- Swagger/OpenAPI
- global error handling
- standardized API responses

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- xUnit

## Current Modules

| Module           | What it covers                                                                                                                 |
| ---------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Menu             | CRUD menu, database-backed persistence, request/response DTOs                                                                  |
| Transaction      | Draft transaction as cart, add/remove item, update quantity, calculate totals                                                  |
| Payment          | Payment request processing, state machine validation, payment persistence, history snapshot creation                           |
| History & Report | Transaction history list, filtering, daily/report summary                                                                      |
| Infrastructure   | Swagger configuration, global exception middleware, custom 404 JSON response, standardized API wrapper, database health checks |

## API Routes

| Area         | Routes                                                                                                                                                                                                              |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Health       | `GET /health`, `GET /health/live`, `GET /health/ready`                                                                                                                                                              |
| Menu         | `GET /api/menus`, `GET /api/menus/{id}`, `POST /api/menus`, `PUT /api/menus/{id}`, `DELETE /api/menus/{id}`                                                                                                         |
| Transactions | `GET /api/transactions`, `GET /api/transactions/{id}`, `POST /api/transactions`, `POST /api/transactions/{id}/items`, `PATCH /api/transactions/{id}/items/{itemId}`, `DELETE /api/transactions/{id}/items/{itemId}` |
| Payments     | `POST /api/payments`                                                                                                                                                                                                |
| Histories    | `GET /api/histories`, `GET /api/histories/{id}`, `GET /api/histories/filter?start=...&end=...`, `GET /api/histories/report?start=...&end=...`                                                                       |

## Project Structure

```text
Tubes_POS_API/
├── Tubes_POS_API.slnx
├── README.md
├── Tubes_POS_API/
│   ├── Controllers/
│   ├── Data/
│   ├── Entities/
│   ├── Middleware/
│   ├── Models/
│   ├── Options/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Tubes_POS_API.csproj
└── Tubes_POS_API.Tests/
```

## Setup

### Prerequisites

- .NET SDK 10

### Run

```bash
dotnet restore
dotnet run --project Tubes_POS_API/Tubes_POS_API.csproj
```

### Test

```bash
dotnet test Tubes_POS_API.Tests/Tubes_POS_API.Tests.csproj
```

## Database Notes

- The app uses SQLite (`pos.db`).
- EF Core mappings are defined in `Tubes_POS_API/Data/AppDbContext.cs`.
- Entities are defined in `Tubes_POS_API/Entities/`.
- If schema changes, use EF migrations instead of relying on `EnsureCreated()`.

## Health Check Notes

- `GET /health/live` checks the app process only.
- `GET /health/ready` checks whether the database is reachable.
- `GET /health` returns a general service summary.

## Demo Flow

1. Open Swagger
2. Check `GET /health/live`
3. Check `GET /api/menus`
4. Create a transaction
5. Add items to transaction
6. Process payment
7. Check `GET /api/histories`
8. Check `GET /api/histories/report`

## Status

This project is for an academic final-semester POS backend demo.
