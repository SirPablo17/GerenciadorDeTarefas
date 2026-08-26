## Why

Precisamos de uma API REST para gerenciamento de tarefas, construída do zero com .NET, EF Core e SQLite, que sirva como base sólida (CRUD completo, autenticação e organização em camadas seguindo SOLID) para evolução futura do projeto.

## What Changes

- Criar solução .NET (`.slnx`, `net10.0`) organizada em Clean Architecture: `Domain`, `Application`, `Infrastructure`, `Api` e um projeto de testes automatizados.
- Adicionar autenticação de usuários via JWT: registro (`POST /auth/register`) e login (`POST /auth/login`), com hash de senha via `PasswordHasher<T>`.
- Adicionar gerenciamento de tarefas (`TaskItem`) com CRUD completo: criar, listar, obter por id, atualizar e deletar, todos protegidos por autenticação e escopados ao usuário autenticado (multi-tenant — cada usuário só acessa suas próprias tarefas).
- Persistir dados com EF Core + SQLite, incluindo migrations.
- Validar entradas com FluentValidation.
- Aplicar princípios SOLID de forma explícita: controllers dependem de abstrações (`ITaskService`, `IAuthService`), repositórios implementam interfaces definidas no `Domain`, e cada camada tem responsabilidade única.
- Configurar o projeto `Api` para abrir automaticamente o navegador no Swagger UI ao iniciar a aplicação em modo de desenvolvimento (`dotnet run`).

## Capabilities

### New Capabilities
- `user-auth`: registro e login de usuários com emissão de JWT, usado para proteger os demais endpoints da API.
- `task-management`: operações CRUD de tarefas, escopadas ao usuário autenticado.

### Modified Capabilities
(nenhuma — projeto novo, sem specs existentes)

## Impact

- Novo projeto/solução .NET do zero (`.slnx`), sem código pré-existente afetado.
- Dependências: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.Identity` (para `PasswordHasher<T>`), framework de testes (xUnit) + `Microsoft.AspNetCore.Mvc.Testing`.
- Novo banco de dados SQLite local (arquivo `.db`) gerenciado via EF Core migrations.
