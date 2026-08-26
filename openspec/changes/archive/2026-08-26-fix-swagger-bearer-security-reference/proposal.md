## Why

O Swagger UI da API não anexava o header `Authorization` às chamadas mesmo depois de o usuário clicar em "Authorize" e informar o token JWT, causando `401 Unauthorized` (com `WWW-Authenticate: Bearer` sem detalhe de erro, indicando ausência total do header) em todos os endpoints protegidos. O bug foi confirmado inspecionando o `swagger.json` gerado: a seção `security` global vinha como `[{}]` (requisito de segurança vazio) em vez de `[{"Bearer": []}]`.

## What Changes

- Corrigir a configuração do `AddSecurityRequirement` em `Program.cs` (projeto `Api`) para que a referência ao esquema `Bearer` seja resolvida corretamente contra o `OpenApiDocument`, fazendo o Swagger UI de fato anexar o header `Authorization: Bearer <token>` às requisições depois de "Authorize".
- Nenhuma mudança de comportamento da API em si (os endpoints já validavam o JWT corretamente quando o header era enviado via curl/Postman) — o defeito era exclusivo da geração do documento OpenAPI consumido pelo Swagger UI.

## Capabilities

### New Capabilities
(nenhuma)

### Modified Capabilities
(nenhuma — comportamento da API já especificado em `user-auth` não mudou; este é um defeito de implementação na geração do OpenAPI/Swagger, não no contrato da API)

## Impact

- Arquivo afetado: `src/GerenciadorDeTarefas.Api/Program.cs` (configuração do `AddSwaggerGen`/`AddSecurityRequirement`).
- Sem impacto em dependências, banco de dados ou clientes que já enviavam o header manualmente (ex.: curl, Postman) — esses sempre funcionaram corretamente.
