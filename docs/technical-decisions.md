# Decisões Técnicas e Trade-offs

## 1) Arquitetura em camadas

**Decisão**
- Separar o sistema em `Domain`, `Application`, `Infrastructure` e `Api`.

**Motivo**
- Isolar regra de negócio de detalhes de framework e persistência.

**Trade-off**
- Mais arquivos e mais wiring de DI.

## 2) Regra de conflito no domínio

**Decisão**
- A verificação de overlap fica na entidade `Kit`.

**Motivo**
- Regra crítica de negócio deve estar no centro do sistema.

**Trade-off**
- Testes de domínio são obrigatórios para evitar regressão.

## 3) Cancelamento no aggregate root

**Decisão**
- Cancelamento ocorre via `Kit.CancelReservation`.

**Motivo**
- Evitar que camada de aplicação manipule diretamente coleção interna.

**Trade-off**
- O aggregate root concentra mais responsabilidades comportamentais.

## 4) Persistência com EF Core + SQLite

**Decisão**
- SQLite para simplicidade de execução local e portfólio.

**Motivo**
- Setup rápido para demonstração técnica.

**Trade-off**
- Não representa cenários de alta concorrência de banco de produção.

## 5) Paginação no banco

**Decisão**
- `GetPageAsync` e `CountAsync` no repositório.

**Motivo**
- Evitar paginação em memória e reduzir custo de leitura.

**Trade-off**
- Contrato de repositório fica mais orientado a consulta.

## 6) Validação de entrada na API

**Decisão**
- FluentValidation para requests (`CreateKit`, `ReserveKit`, `GetKits`, `AuthToken`).

**Motivo**
- Falhar cedo e manter handlers focados no caso de uso.

**Trade-off**
- Dependência adicional no pipeline HTTP.

## 7) Autenticação e autorização com JWT

**Decisão**
- Token JWT com roles (`Viewer`, `Manager`, `Admin`) e policies (`ReadKits`, `ManageKits`).
- Chave de assinatura e credenciais fora do repositório (User Secrets/variáveis de ambiente).

**Motivo**
- Cobrir cenário real de controle de acesso e evitar endpoints abertos em uma API pública.
- As credenciais ficam simples por ser demo, mas o fluxo é o mesmo de um sistema real.

**Trade-off**
- Configuração adicional e gestão de credenciais de ambiente.

## 8) Contrato de erro com ProblemDetails

**Decisão**
- Middleware central converte exceções para `ProblemDetails` completo.

**Motivo**
- Contrato padrão HTTP com suporte a `code`, `traceId` e `correlationId`.

**Trade-off**
- Testes de integração precisam validar payload mais flexível.

## 9) Observabilidade básica

**Decisão**
- Correlation id por request, logs JSON estruturados, `health` e `metrics`.

**Motivo**
- Facilitar troubleshooting quando algo dá errado e ter um mínimo de visão da saúde da API.
- Mesmo em projeto pequeno, isso ajuda a simular rotina de suporte.

**Trade-off**
- Mais elementos de infraestrutura para configurar em deploy.

## 10) Assincronia ponta a ponta

**Decisão**
- Use cases, repositório e controller em `async`.

**Motivo**
- Melhor uso de recursos I/O em API real.

**Trade-off**
- Código ligeiramente mais verboso.

## 11) Testes

**Decisão**
- Combinar testes unitários (domínio/aplicação) com integração HTTP.

**Motivo**
- Cobrir regra de negócio e comportamento real de endpoint.

**Trade-off**
- Tempo de execução maior que suíte somente unitária.

## 12) CI simples e objetivo

**Decisão**
- Pipeline com `restore`, `build` e `test`.

**Motivo**
- Garantir baseline de qualidade em push e PR.

**Trade-off**
- Ainda não inclui quality gates avançados (coverage mínima, SAST).

## 13) Mensageria com RabbitMQ

**Decisão**
- Publicar eventos de integração (`reservation.created`, `reservation.cancelled`).

**Motivo**
- Demonstrar desacoplamento e permitir processamento assíncrono fora do request principal.

**Trade-off**
- Mais infraestrutura local para rodar e monitorar.
