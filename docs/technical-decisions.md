# Decisoes Tecnicas e Trade-offs

## 1) Arquitetura em camadas

**Decisao**
- Separar o sistema em `Domain`, `Application`, `Infrastructure` e `Api`.

**Motivo**
- Isolar regra de negocio de detalhes de framework e persistencia.

**Trade-off**
- Mais arquivos e mais wiring de DI.

## 2) Regra de conflito no dominio

**Decisao**
- A verificacao de overlap fica na entidade `Kit`.

**Motivo**
- Regra critica de negocio deve estar no centro do sistema.

**Trade-off**
- Testes de dominio sao obrigatorios para evitar regressao.

## 3) Cancelamento no aggregate root

**Decisao**
- Cancelamento ocorre via `Kit.CancelReservation`.

**Motivo**
- Evitar que camada de aplicacao manipule diretamente colecao interna.

**Trade-off**
- O aggregate root concentra mais responsabilidades comportamentais.

## 4) Persistencia com EF Core + SQLite

**Decisao**
- SQLite para simplicidade de execucao local e portfolio.

**Motivo**
- Setup rapido para demonstracao tecnica.

**Trade-off**
- Nao representa cenarios de alta concorrencia de banco de producao.

## 5) Paginacao no banco

**Decisao**
- `GetPageAsync` e `CountAsync` no repositorio.

**Motivo**
- Evitar paginacao em memoria e reduzir custo de leitura.

**Trade-off**
- Contrato de repositorio fica mais orientado a consulta.

## 6) Validacao de entrada na API

**Decisao**
- FluentValidation para requests (`CreateKit`, `ReserveKit`, `GetKits`, `AuthToken`).

**Motivo**
- Falhar cedo e manter handlers focados no caso de uso.

**Trade-off**
- Dependencia adicional no pipeline HTTP.

## 7) Autenticacao e autorizacao com JWT

**Decisao**
- Token JWT com roles (`Viewer`, `Manager`, `Admin`) e policies (`ReadKits`, `ManageKits`).

**Motivo**
- Cobrir cenario real de controle de acesso em API de portfolio.

**Trade-off**
- Configuracao adicional e gestao de credenciais de ambiente.

## 8) Contrato de erro com ProblemDetails

**Decisao**
- Middleware central converte excecoes para `ProblemDetails` completo.

**Motivo**
- Contrato padrao HTTP com suporte a `code`, `traceId` e `correlationId`.

**Trade-off**
- Testes de integracao precisam validar payload mais flexivel.

## 9) Observabilidade basica

**Decisao**
- Correlation id por request, logs JSON estruturados, `health` e `metrics`.

**Motivo**
- Facilitar troubleshooting e monitoracao minima em ambiente real.

**Trade-off**
- Mais elementos de infraestrutura para configurar em deploy.

## 10) Assincronia ponta a ponta

**Decisao**
- Use cases, repositorio e controller em `async`.

**Motivo**
- Melhor uso de recursos I/O em API real.

**Trade-off**
- Codigo ligeiramente mais verboso.

## 11) Testes

**Decisao**
- Combinar testes unitarios (dominio/aplicacao) com integracao HTTP.

**Motivo**
- Cobrir regra de negocio e comportamento real de endpoint.

**Trade-off**
- Tempo de execucao maior que suite somente unitaria.

## 12) CI simples e objetivo

**Decisao**
- Pipeline com `restore`, `build` e `test`.

**Motivo**
- Garantir baseline de qualidade em push e PR.

**Trade-off**
- Ainda nao inclui quality gates avancados (coverage minima, SAST).
