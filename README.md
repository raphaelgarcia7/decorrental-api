# DecorRental API

![Demonstração DecorRental](https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExcW9hM2txcGx5d2l4eDc4aTlkM2Y3MzduN2F6cnNnbm5jMHo3NnBvaiZlcD12MV9naWZzX3NlYXJjaCZjdD1n/13HgwGsXF0aiGY/giphy.gif)

API para gestão de kits de decoração e reservas por período.

## Objetivo

Demonstrar arquitetura em camadas (`Domain`, `Application`, `Infrastructure`, `Api`) com regra de negócio central no domínio, validação, testes automatizados e observabilidade básica.

## Regra de negócio principal

- Um kit não pode ter duas reservas ativas com período sobreposto.
- Reserva cancelada não bloqueia novo período.

## Stack

- .NET 9
- ASP.NET Core Web API
- EF Core 9 + SQLite
- JWT Bearer Authentication
- FluentValidation
- Prometheus metrics + Health Checks
- xUnit (unitário + integração)

## Arquitetura

- `DecorRental.Domain`: entidades, value objects e regras de negócio.
- `DecorRental.Application`: casos de uso e orquestração.
- `DecorRental.Infrastructure`: persistência EF Core.
- `DecorRental.Api`: controllers, autenticação/autorização, validação, middleware e contrato HTTP.

## Como executar

```bash
dotnet build
dotnet run --project .\DecorRental.Api
```

A API aplica migrations no startup.

## Autenticação (JWT)

1. Gere token em `POST /api/auth/token`.
2. Envie `Authorization: Bearer {token}` nas rotas protegidas.

Usuários locais de desenvolvimento (configurados em `DecorRental.Api/appsettings.json`):

- `viewer` / `viewer123` (somente leitura)
- `manager` / `manager123` (leitura + escrita)
- `admin` / `admin123` (leitura + escrita)

## Contrato de erro

Erros retornam `application/problem+json` com `ProblemDetails` e extensões:

- `code`
- `traceId`
- `correlationId`
- `errors` (quando falha de validação)

## Observabilidade

- Correlation id por request (`X-Correlation-Id`).
- Logs estruturados em JSON.
- Health check: `GET /health`.
- Metrics em formato Prometheus: `GET /metrics`.

## Endpoints

- `POST /api/auth/token`
- `POST /api/kits`
- `GET /api/kits?page=1&pageSize=20`
- `GET /api/kits/{id}`
- `GET /api/kits/{id}/reservations`
- `POST /api/kits/{id}/reservations`
- `POST /api/kits/{id}/reservations/{reservationId}/cancel`

## Testes

```bash
dotnet test
```

## Referências rápidas

- CI: `.github/workflows/ci.yml`
- Postman: `DecorRental.postman_collection.json`
- HTTP requests: `DecorRental.Api/decorrental-api.http`
- Decisões técnicas: `docs/technical-decisions.md`
