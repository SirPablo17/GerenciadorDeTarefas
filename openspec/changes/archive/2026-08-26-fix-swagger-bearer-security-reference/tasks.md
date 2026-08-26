## 1. Diagnóstico

- [x] 1.1 Reproduzir o 401 via Swagger UI/curl e inspecionar o header `WWW-Authenticate` da resposta para confirmar ausência do header `Authorization` na requisição
- [x] 1.2 Inspecionar o `swagger.json` gerado (`/swagger/v1/swagger.json`) e confirmar que a seção `security` estava vazia (`[{}]`) em vez de referenciar `Bearer`

## 2. Correção

- [x] 2.1 Corrigir `AddSecurityRequirement` em `src/GerenciadorDeTarefas.Api/Program.cs` para resolver `OpenApiSecuritySchemeReference("Bearer", document)` contra o `OpenApiDocument` do callback, e verificar que `dotnet build` do projeto `Api` continua sem erros
- [x] 2.2 Reinspecionar o `swagger.json` gerado e verificar que `security` agora é `[{"Bearer": []}]`

## 3. Verificação

- [x] 3.1 Repetir o fluxo completo via curl (registrar, logar, criar/listar tarefa com o token) e verificar que todas as chamadas autenticadas retornam sucesso (201/200) e chamadas sem token retornam 401
- [x] 3.2 Rodar `dotnet test` na solução completa e verificar que os 46 testes continuam passando
