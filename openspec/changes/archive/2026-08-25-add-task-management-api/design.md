## Context

Projeto novo (greenfield) — não há código ou specs anteriores. Ver proposal.md - Why para a motivação. SDK instalado: .NET 10.0.111 (LTS), com suporte nativo a `.slnx` via `dotnet sln`.

## Goals / Non-Goals

**Goals:**
- Separação de camadas que torne os princípios SOLID (em especial SRP e DIP) visíveis na estrutura do projeto, não apenas na intenção.
- Autenticação própria (registro/login) autossuficiente, sem depender de provedor externo (ex.: IdentityServer, Auth0).
- Isolamento de dados por usuário (multi-tenant) garantido na camada de aplicação, não apenas confiado ao cliente.
- Base testável: services e controllers dependem de abstrações, permitindo testes unitários com mocks e testes de integração com banco real (SQLite).

**Non-Goals:**
- Refresh tokens, revogação de token ou logout server-side (fica para uma iteração futura).
- Recuperação de senha / confirmação de email.
- Papéis/permissões (roles) além de "usuário autenticado dono do recurso".
- Paginação, filtros ou ordenação avançada na listagem de tarefas (lista simples por enquanto).
- Deploy/hospedagem — escopo é o código da API rodando localmente.

## Decisions

### 1. Estrutura em 4 projetos + testes (Clean Architecture)
`Domain` (entidades + interfaces de repositório) ← `Application` (casos de uso, DTOs, validação FluentValidation, interfaces de serviços de infraestrutura como `IPasswordHasher`/`ITokenGenerator`) ← `Infrastructure` (EF Core + SQLite, implementações concretas) ← `Api` (controllers, DI, middleware).
**Alternativa considerada**: projeto único em pastas — descartada porque o objetivo explícito do usuário é demonstrar SOLID/DIP de forma didática, e a separação física de assembly torna violação de dependência um erro de compilação, não só de convenção.

### 2. Controllers (MVC) em vez de Minimal APIs
Controllers com injeção de dependência via construtor deixam o mapeamento para SOLID (SRP por controller, DIP via interfaces injetadas) mais explícito e familiar para revisão.

### 3. Autenticação: JWT emitido pela própria API
`AuthController` expõe `/auth/register` e `/auth/login`. Login valida credenciais e emite um JWT assinado (chave simétrica via configuração) contendo o id do usuário como claim. Middleware `AddAuthentication().AddJwtBearer(...)` protege os endpoints de `TasksController` com `[Authorize]`.
**Alternativa considerada**: aceitar token emitido externamente — descartada porque o usuário optou por registro/login próprios.

### 4. Hash de senha: `PasswordHasher<T>` (ASP.NET Core Identity)
Escolhido em vez de `BCrypt.Net` por já vir no framework (sem dependência externa adicional) e ser a opção padrão recomendada pela Microsoft para cenários que não usam o Identity completo.

### 5. Validação: FluentValidation
Validadores como classes separadas (`CreateTaskRequestValidator`, `RegisterRequestValidator`) em vez de DataAnnotations nos DTOs, reforçando SRP (regra de validação isolada da forma do DTO) e permitindo regras mais expressivas (ex.: força de senha).

### 6. Isolamento multi-tenant na camada de aplicação
`TaskService` sempre recebe o `userId` do usuário autenticado (extraído do claim do token pelo controller) e filtra/valida ownership em toda operação (list/get/update/delete), nunca confiando em um `userId` vindo do corpo da requisição. Um `GetById`/`Update`/`Delete` sobre tarefa de outro usuário retorna "não encontrado" (não "proibido"), para não revelar a existência do recurso a quem não é dono.

### 7. Persistência: EF Core + SQLite com migrations versionadas
`AppDbContext` no projeto `Infrastructure`, com migrations geradas via `dotnet ef migrations add`. Arquivo SQLite local para desenvolvimento.

### 8. Testes automatizados
Projeto `tests/GerenciadorDeTarefas.Tests` com xUnit: testes unitários para `TaskService`/`AuthService` (mockando repositórios/interfaces) e testes de integração para os controllers via `WebApplicationFactory`, usando SQLite em memória ou arquivo temporário por execução.

### 9. Abertura automática do navegador no Swagger UI (ambiente de desenvolvimento)
Ao rodar `dotnet run` em Development, o navegador padrão abre automaticamente na
página do Swagger UI, via `launchBrowser: true` + `launchUrl: "swagger"` no perfil
do `launchSettings.json` do projeto `Api`.
**Alternativa considerada**: chamar `Process.Start` manualmente em `Program.cs` —
descartada por exigir lógica específica de SO para abrir uma URL (comandos diferentes
em Windows/macOS/Linux), duplicar um comportamento que o tooling do .NET já oferece
nativamente, e ser menos previsível fora do IDE/CLI. `launchSettings.json` é a
abordagem padrão suportada por `dotnet run`, Visual Studio e Rider, e não afeta
ambientes de produção (ignorado fora de execução local via `dotnet run`/debug).

## Risks / Trade-offs

- [Múltiplos projetos aumentam boilerplate inicial] → aceito conscientemente: é o ponto do exercício (SOLID explícito), não um acidente de complexidade.
- [JWT com chave simétrica em configuração local] → adequado para escopo atual (sem deploy); documentar que a chave deve vir de secret/variável de ambiente antes de qualquer uso além de desenvolvimento local.
- [Sem refresh token] → sessões expiram e exigem novo login; aceitável pois está fora do escopo (Non-Goals) desta mudança.
- [SQLite para produção tem limitações de concorrência] → aceitável para o escopo atual (projeto de aprendizado/base); não é uma escolha para alta concorrência.
- [`launchSettings.json` não é respeitado em todo hosting/CI] → aceitável: é comportamento apenas de desenvolvimento local, sem efeito em produção.
