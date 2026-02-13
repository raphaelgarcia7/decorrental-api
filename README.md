# DecorRental API

API para gestao de kits de decoracao e reservas por periodo.

## Objetivo

Demonstrar arquitetura em camadas (`Domain`, `Application`, `Infrastructure`, `Api`) com regra de negocio central no dominio, validacao, testes automatizados e observabilidade basica.

## Regra de negocio principal

- Um kit nao pode ter duas reservas ativas com periodo sobreposto.
- Reserva cancelada nao bloqueia novo periodo.

## Stack

- .NET 9
- ASP.NET Core Web API
- EF Core 9 + SQLite
- JWT Bearer Authentication
- FluentValidation
- Prometheus metrics + Health Checks
- xUnit (unitario + integracao)

## Arquitetura

- `DecorRental.Domain`: entidades, value objects e regras de negocio.
- `DecorRental.Application`: casos de uso e orquestracao.
- `DecorRental.Infrastructure`: persistencia EF Core.
- `DecorRental.Api`: controllers, autenticacao/autorizacao, validacao, middleware e contrato HTTP.

## Como executar

```bash
dotnet build
dotnet run --project .\DecorRental.Api
```

A API aplica migrations no startup.

## Autenticacao (JWT)

1. Gere token em `POST /api/auth/token`.
2. Envie `Authorization: Bearer {token}` nas rotas protegidas.

Usuarios locais de desenvolvimento (configurados em `DecorRental.Api/appsettings.json`):

- `viewer` / `viewer123` (somente leitura)
- `manager` / `manager123` (leitura + escrita)
- `admin` / `admin123` (leitura + escrita)

## Contrato de erro

Erros retornam `application/problem+json` com `ProblemDetails` e extensoes:

- `code`
- `traceId`
- `correlationId`
- `errors` (quando falha de validacao)

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

## Referencias rapidas

- CI: `.github/workflows/ci.yml`
- Postman: `DecorRental.postman_collection.json`
- HTTP requests: `DecorRental.Api/decorrental-api.http`
- Decisoes tecnicas: `docs/technical-decisions.md`
