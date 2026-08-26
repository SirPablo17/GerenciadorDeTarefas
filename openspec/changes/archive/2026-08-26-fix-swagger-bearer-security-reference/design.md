## Context

Ver proposal.md - Why para o sintoma e o diagnóstico. Causa raiz: o pacote `Swashbuckle.AspNetCore` 10.2.3 usado neste projeto adota o novo modelo de objetos do `Microsoft.OpenApi` 2.x, no qual referências (`OpenApiSecuritySchemeReference`, etc.) precisam ser resolvidas explicitamente contra um `OpenApiDocument` para serializar corretamente como referência (`$ref`-like) no documento final. O código original criava a referência sem esse contexto:

```csharp
options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
{
    { new OpenApiSecuritySchemeReference("Bearer"), [] }
});
```

Sem o `OpenApiDocument`, a referência não conseguia resolver o nome `"Bearer"` contra a definição registrada em `AddSecurityDefinition`, e o Swashbuckle serializava a entrada como um objeto de segurança vazio (`{}`) em vez de `{"Bearer": []}`. Resultado prático: o Swagger UI mostrava o diálogo "Authorize" normalmente (a definição do esquema em si estava correta), mas nenhuma operação ficava de fato associada a um requisito de segurança válido — então o header `Authorization` nunca era anexado às chamadas de "Try it out".

## Goals / Non-Goals

**Goals:**
- Fazer o Swagger UI anexar corretamente o header `Authorization: Bearer <token>` a todas as chamadas de endpoints protegidos após "Authorize".
- Preservar o esquema de segurança `http`/`bearer` (sem exigir que o usuário digite o prefixo `Bearer` manualmente).

**Non-Goals:**
- Não altera o comportamento de validação de JWT da API (`AddJwtBearer`) — esse já estava correto, confirmado via testes de integração e chamadas curl diretas durante o diagnóstico.

## Decisions

### Resolver a referência do esquema de segurança contra o `OpenApiDocument`
Trocar a criação da referência para usar o `document` recebido no callback do `AddSecurityRequirement`:

```csharp
options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    { new OpenApiSecuritySchemeReference("Bearer", document), [] }
});
```

**Alternativa considerada**: voltar para a API antiga do Swashbuckle (`OpenApiSecurityScheme { Reference = new OpenApiReference { ... } }`) — descartada porque essa sobrecarga não existe mais no modelo `Microsoft.OpenApi` 2.x usado pela versão instalada do Swashbuckle; o build já falhava com esse formato (`CS0117`).

**Verificação**: inspecionado o `swagger.json` gerado antes e depois da correção — `security` passou de `[{}]` para `[{"Bearer": []}]`. Repetido o fluxo completo (registro → login → criar/listar tarefa com token) via curl e via `dotnet test` (46/46 testes) para confirmar que nada mais foi afetado.

## Risks / Trade-offs

- [Comportamento específico de uma combinação de versões de pacote (`Swashbuckle.AspNetCore` 10.2.3 + `Microsoft.OpenApi` 2.7.5)] → uma futura atualização de major version desses pacotes pode voltar a mudar essa API; se o Swagger UI parar de enviar o header novamente após um `dotnet update`, revisar primeiro a serialização de `security` no `swagger.json` antes de suspeitar de configuração de JWT.
