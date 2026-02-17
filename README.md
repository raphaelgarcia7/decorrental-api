# DecorRental API

<p align="center">
  <img width="360" alt="DecorRental" src="https://github.com/user-attachments/assets/9bcd611e-183a-44c9-bf4a-845d98fb97c4" />
</p>
API para gestão de kits de decoração e reservas por período.

## Contexto real

Este projeto nasceu para resolver um problema real da empresa de decoração “pegue e monte” da minha mãe.
Antes, o controle de kits e períodos de reserva era manual, aumentando risco de conflito de agenda e retrabalho.
A API centraliza essa operação com regras de negócio claras e rastreáveis.

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
- Docker + Docker Compose
- xUnit (unitário + integração)

## Arquitetura

- `DecorRental.Domain`: entidades, value objects e regras de negócio.
- `DecorRental.Application`: casos de uso e orquestração.
- `DecorRental.Infrastructure`: persistência EF Core.
- `DecorRental.Api`: controllers, autenticação/autorização, validação, middleware e contrato HTTP.
Diagrama: `docs/architecture.md`.

## Como executar

### Local

```bash
dotnet build
dotnet run --project .\DecorRental.Api
```

A API aplica migrations no startup.

### Docker

1. Crie um `.env` com base no `.env.example`.
2. Rode:

```bash
docker compose up --build -d
```

API disponível em `http://localhost:8080`.

Para encerrar:

```bash
docker compose down
```

## Autenticação (JWT)

As credenciais e chave JWT **não ficam versionadas**.
Você pode configurar de duas formas:

- Docker: preencha o arquivo `.env`.
- Local: use User Secrets.

```bash
dotnet user-secrets --project .\DecorRental.Api set "Jwt:SigningKey" "sua-chave-com-pelo-menos-32-caracteres"
dotnet user-secrets --project .\DecorRental.Api set "Jwt:Users:0:Username" "viewer"
dotnet user-secrets --project .\DecorRental.Api set "Jwt:Users:0:Password" "<senha-viewer>"
dotnet user-secrets --project .\DecorRental.Api set "Jwt:Users:0:Role" "Viewer"
dotnet user-secrets --project .\DecorRental.Api set "Jwt:Users:1:Username" "manager"
dotnet user-secrets --project .\DecorRental.Api set "Jwt:Users:1:Password" "<senha-manager>"
dotnet user-secrets --project .\DecorRental.Api set "Jwt:Users:1:Role" "Manager"
```

1. Gere token em `POST /api/auth/token`.
2. Envie `Authorization: Bearer {token}` nas rotas protegidas.

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
Serve para verificar se a API está saudável e ter dados básicos de execução.

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

## Planos futuros

- Criar frontend web (React + TypeScript) para operação diária da locadora.
- Tela de calendário de disponibilidade por kit.
- Fluxo de autenticação no frontend com perfis de acesso.
- Dashboard com indicadores de reservas ativas, canceladas e ocupação.
- Preparar deploy completo (API + frontend) com ambiente de staging e produção.

## Referências rápidas

- CI: `.github/workflows/ci.yml`
- Postman: `DecorRental.postman_collection.json`
- HTTP requests: `DecorRental.Api/decorrental-api.http`
- Decisões técnicas: `docs/technical-decisions.md`

