# GerenciadorDeTarefas

Uma API REST simples de gerenciamento de tarefas (task management), construída como **projeto de estudo** para aprender na prática os conceitos de **Spec Driven Development (SDD)** e do fluxo de trabalho do **OpenSpec**.

Não é um produto pensado para produção — o objetivo é usar um domínio pequeno e familiar (tarefas + autenticação) como pretexto para exercitar o ciclo: propor uma mudança, escrever specs, desenhar a solução, quebrar em tarefas, implementar e arquivar.

## O que é Spec Driven Development / OpenSpec

**Spec Driven Development** é uma forma de trabalhar em que o comportamento do sistema é descrito em especificações (requisitos + cenários) *antes* e *durante* a implementação, em vez de deduzido apenas a partir do código depois do fato.

O **[OpenSpec](https://github.com/Fission-AI/OpenSpec)** é a ferramenta/fluxo usada aqui para isso. Cada mudança no sistema passa por um conjunto de artefatos versionados em `openspec/`:

- **proposal.md** — por que a mudança é necessária e o que ela muda.
- **specs/** (delta specs) — requisitos e cenários adicionados/modificados/removidos.
- **design.md** — decisões técnicas de como implementar (quando a mudança justifica).
- **tasks.md** — checklist de implementação rastreável.

Depois que a mudança é implementada, as delta specs são sincronizadas com as specs "principais" em `openspec/specs/` e a mudança é arquivada em `openspec/changes/archive/`.

Neste repositório:

- `openspec/specs/` contém as specs principais e atuais do sistema (hoje: `user-auth` e `task-management`).
- `openspec/changes/archive/` contém o histórico de mudanças já implementadas e arquivadas, cada uma com sua proposta, specs, design e tasks — um bom lugar para ver o processo completo em ação.

## Stack

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** com **SQLite**
- **Autenticação JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **FluentValidation** para validação de request
- **Swashbuckle / Swagger** para documentação interativa da API
- **xUnit** (via o projeto de testes) para testes de unidade e integração

## Estrutura do projeto

Solução organizada em camadas (Clean Architecture / DDD simplificado):

```
src/
  GerenciadorDeTarefas.Domain/         Entidades e contratos de repositório
  GerenciadorDeTarefas.Application/    Serviços de aplicação, DTOs, validadores
  GerenciadorDeTarefas.Infrastructure/ EF Core, persistência, autenticação (JWT, hash de senha)
  GerenciadorDeTarefas.Api/            Controllers, middlewares, ponto de entrada da API
tests/
  GerenciadorDeTarefas.Tests/          Testes de integração e de aplicação
openspec/
  specs/                               Specs principais (estado atual do sistema)
  changes/                             Mudanças em andamento e arquivadas (histórico SDD)
```

## Endpoints principais

- `POST /auth/register` — registra um novo usuário
- `POST /auth/login` — autentica e retorna um token JWT
- `GET /tasks` — lista as tarefas do usuário autenticado
- `POST /tasks` — cria uma tarefa
- `GET /tasks/{id}` — obtém uma tarefa do usuário autenticado
- `PUT /tasks/{id}` — atualiza uma tarefa
- `DELETE /tasks/{id}` — remove uma tarefa

Todos os endpoints de `/tasks` exigem um token JWT válido (obtido via `/auth/login`) e cada usuário só enxerga suas próprias tarefas.

## Como rodar

Pré-requisito: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
# build de toda a solução
dotnet build

# rodar a API (Swagger disponível em /swagger no ambiente de desenvolvimento)
dotnet run --project src/GerenciadorDeTarefas.Api

# rodar os testes
dotnet test
```

## Saiba mais

Para ver o fluxo de Spec Driven Development em detalhe, explore:

- `openspec/specs/user-auth/spec.md` e `openspec/specs/task-management/spec.md` — o que o sistema faz hoje.
- `openspec/changes/archive/` — como cada funcionalidade foi proposta, especificada, desenhada e implementada.
