## 1. Solution & Project Setup

- [x] 1.1 Criar `GerenciadorDeTarefas.slnx` na raiz e os projetos `src/GerenciadorDeTarefas.Domain`, `src/GerenciadorDeTarefas.Application`, `src/GerenciadorDeTarefas.Infrastructure`, `src/GerenciadorDeTarefas.Api` (net10.0), e `tests/GerenciadorDeTarefas.Tests` (xUnit); verificar que `dotnet build` na solução completa é bem-sucedido
- [x] 1.2 Configurar referências entre projetos (`Application` → `Domain`; `Infrastructure` → `Application` + `Domain`; `Api` → `Infrastructure` + `Application`; `Tests` → todos) e verificar com `dotnet build` que não há dependência invertida
- [x] 1.3 Adicionar pacotes NuGet: `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design` (Infrastructure); `Microsoft.AspNetCore.Authentication.JwtBearer` (Api); `FluentValidation.AspNetCore` (Application/Api); `Microsoft.Extensions.Identity.Core` para `PasswordHasher<T>` (Infrastructure); `xunit`, `Microsoft.AspNetCore.Mvc.Testing`, `Moq` (Tests); verificar com `dotnet restore`

## 2. Domain Layer

- [x] 2.1 Criar entidade `User` (Id, Email, PasswordHash) e entidade `TaskItem` (Id, Title, Description, Status/IsCompleted, UserId, CreatedAt, UpdatedAt) e verificar que o projeto `Domain` compila sem dependências externas
- [x] 2.2 Definir interfaces `IUserRepository` (GetByEmail, Add, GetById) e `ITaskRepository` (GetById, ListByUser, Add, Update, Delete) no `Domain` e verificar que compilam

## 3. Application Layer

- [x] 3.1 Criar DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`, `TaskDto`, `CreateTaskRequest`, `UpdateTaskRequest`
- [x] 3.2 Definir interfaces de infraestrutura usadas pela Application: `IPasswordHasher`, `ITokenGenerator` (ambas no `Domain` ou `Application`, implementadas depois na `Infrastructure`)
- [x] 3.3 Implementar `IAuthService`/`AuthService` com métodos `RegisterAsync` (rejeita email duplicado) e `LoginAsync` (valida credenciais, retorna JWT via `ITokenGenerator`); cobrir com testes unitários (mock de `IUserRepository`/`IPasswordHasher`/`ITokenGenerator`) que passem
- [x] 3.4 Implementar `ITaskService`/`TaskService` com `CreateAsync`, `ListByUserAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`, todos recebendo `userId` e aplicando isolamento por dono (retornando "não encontrado" para tarefas de outro usuário); cobrir com testes unitários (mock de `ITaskRepository`) que passem, incluindo o caso de acesso a tarefa de outro usuário
- [x] 3.5 Criar validadores FluentValidation: `RegisterRequestValidator`, `LoginRequestValidator`, `CreateTaskRequestValidator` (título obrigatório), `UpdateTaskRequestValidator`; cobrir com testes unitários que passem para casos válidos e inválidos

## 4. Infrastructure Layer

- [x] 4.1 Criar `AppDbContext` (EF Core) com `DbSet<User>` e `DbSet<TaskItem>`, mapeando relação User→Tasks e configurando SQLite como provider
- [x] 4.2 Gerar migration inicial (`dotnet ef migrations add InitialCreate`) e verificar que `dotnet ef database update` cria o arquivo SQLite com as tabelas esperadas
- [x] 4.3 Implementar `UserRepository` e `TaskRepository` sobre o `AppDbContext`, implementando as interfaces do `Domain`
- [x] 4.4 Implementar `PasswordHasher` (wrapper sobre `PasswordHasher<T>` do ASP.NET Core Identity) implementando `IPasswordHasher`
- [x] 4.5 Implementar `JwtTokenGenerator` implementando `ITokenGenerator`, gerando token assinado com claim de `userId` e expiração configurável

## 5. Api Layer

- [x] 5.1 Configurar `Program.cs`: DI de todos os serviços/repositórios/interfaces (mapeando abstração → implementação), `DbContext`, autenticação JWT Bearer (chave/issuer/audience via `appsettings.json`/configuração), FluentValidation, Swagger/OpenAPI; verificar que `dotnet run` sobe a API sem erros
- [x] 5.2 Implementar `AuthController` com `POST /auth/register` e `POST /auth/login`, delegando para `IAuthService`, retornando os códigos de status corretos (201, 200, 400, 401)
- [x] 5.3 Implementar `TasksController` com `[Authorize]`, extraindo `userId` do claim do token, expondo `POST /tasks`, `GET /tasks`, `GET /tasks/{id}`, `PUT /tasks/{id}`, `DELETE /tasks/{id}`, delegando para `ITaskService` e retornando os códigos de status corretos (200/201/204/400/401/404)
- [x] 5.4 Adicionar middleware/filtro global de tratamento de erros retornando respostas `ProblemDetails` consistentes para erros de validação e exceções não tratadas
- [x] 5.5 Configurar `launchSettings.json` do projeto `Api` com `launchBrowser: true` e `launchUrl` apontando para a página do Swagger UI no perfil de desenvolvimento; verificar que `dotnet run` abre o navegador automaticamente exibindo os endpoints da API

## 6. Testes de Integração

- [x] 6.1 Configurar `WebApplicationFactory` no projeto de testes usando SQLite (arquivo temporário ou in-memory) isolado por execução de teste
- [x] 6.2 Escrever testes de integração cobrindo o fluxo de auth: registro bem-sucedido, registro com email duplicado, login bem-sucedido, login com credenciais inválidas, e acesso negado a endpoint de tarefas sem token — todos passando
- [x] 6.3 Escrever testes de integração cobrindo o CRUD completo de tarefas com usuário autenticado: criar, listar, obter por id, atualizar, deletar, incluindo o caso de tentar acessar/atualizar/deletar tarefa de outro usuário (deve retornar 404) — todos passando

## 7. Verificação Final

- [x] 7.1 Rodar `dotnet test` na solução completa e verificar que todos os testes (unitários e de integração) passam
- [x] 7.2 Validar manualmente via Swagger/HTTP client o fluxo completo: registrar usuário, logar, usar o token para criar/listar/obter/atualizar/deletar uma tarefa
