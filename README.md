# DecorRental API

API for managing decoration kits and reservations with conflict prevention. Built as a portfolio project using Clean Architecture, domain-first modeling, and tests.

## Architecture

- DecorRental.Domain: entities, value objects, domain rules
- DecorRental.Application: use cases (commands/handlers)
- DecorRental.Infrastructure: EF Core + SQLite persistence
- DecorRental.Api: HTTP endpoints and composition root

## Business rule

- A kit cannot be reserved if there is an overlapping active reservation.
- Cancelled reservations do not block new reservations.

## How to run

1. Restore and build
```
dotnet build
```

2. Run the API
```
dotnet run --project .\DecorRental.Api
```

The API applies EF Core migrations automatically on startup and creates `decorental.db` if it does not exist.

## Tests

```
dotnet test
```

## Endpoints (summary)

- POST /api/kits
- GET /api/kits
- GET /api/kits/{id}
- POST /api/kits/{id}/reservations
- POST /api/kits/{id}/reservations/{reservationId}/cancel

See `DecorRental.Api/decorrental-api.http` for sample requests.
