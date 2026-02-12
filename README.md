# DecorRental API

API para gestão de kits de decoração e reservas por período.

## Objetivo

Demonstrar arquitetura em camadas (Domain, Application, Infrastructure, Api), foco em regra de negócio e testes automatizados.

## Regra de negócio principal

- Um kit não pode ter duas reservas ativas com período sobreposto.
- Reserva cancelada não bloqueia novo período.

## Stack

- .NET 9
- ASP.NET Core Web API
- EF Core 9 + SQLite
- FluentValidation
- xUnit

## Arquitetura

- `DecorRental.Domain`: entidades, value objects e regras de negócio
- `DecorRental.Application`: casos de uso e orquestração
- `DecorRental.Infrastructure`: persistência EF Core
- `DecorRental.Api`: endpoints, validação e middleware

## Como executar

```bash
dotnet build
dotnet run --project .\DecorRental.Api
```

A API aplica migrations no startup.

## Testes

```bash
dotnet test
```

Inclui testes unitários e testes de integração HTTP.

## Endpoints

- `POST /api/kits`
- `GET /api/kits?page=1&pageSize=20`
- `GET /api/kits/{id}`
- `GET /api/kits/{id}/reservations`
- `POST /api/kits/{id}/reservations`
- `POST /api/kits/{id}/reservations/{reservationId}/cancel`

## CI

Workflow em `.github/workflows/ci.yml`.

## Referências rápidas

- coleção Postman: `DecorRental.postman_collection.json`
- requisições HTTP: `DecorRental.Api/decorrental-api.http`
- decisões técnicas: `docs/technical-decisions.md`
