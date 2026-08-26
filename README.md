# GerenciadorDeTarefas

Uma aplicação simples de gerenciamento de tarefas (task management) — API REST em .NET e um cliente web em Angular — construída como **projeto de estudo** para aprender na prática os conceitos de **Spec Driven Development (SDD)** e do fluxo de trabalho do **OpenSpec**.

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

- `openspec/specs/` contém as specs principais e atuais do sistema (hoje: `user-auth`, `task-management` e `task-management-ui`).
- `openspec/changes/archive/` contém o histórico de mudanças já implementadas e arquivadas, cada uma com sua proposta, specs, design e tasks — um bom lugar para ver o processo completo em ação.

## Stack

**Backend** (`src/`):
- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** com **SQLite**
- **Autenticação JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **FluentValidation** para validação de request
- **Swashbuckle / Swagger** para documentação interativa da API
- **xUnit** (via o projeto de testes) para testes de unidade e integração

**Frontend** (`web/`):
- **Angular 22**, standalone components + signals (sem NgRx)
- **Angular Material** (`@angular/material` + `@angular/cdk`) como biblioteca de UI
- **Reactive Forms** para os formulários (login, cadastro, tarefa)
- **Vitest** (via `@angular/build:unit-test`) para testes unitários

## Estrutura do projeto

Solução organizada em camadas (Clean Architecture / DDD simplificado), com o frontend como um projeto Node/Angular independente, fora da solution .NET:

```
src/
  GerenciadorDeTarefas.Domain/         Entidades e contratos de repositório
  GerenciadorDeTarefas.Application/    Serviços de aplicação, DTOs, validadores
  GerenciadorDeTarefas.Infrastructure/ EF Core, persistência, autenticação (JWT, hash de senha)
  GerenciadorDeTarefas.Api/            Controllers, middlewares, ponto de entrada da API
tests/
  GerenciadorDeTarefas.Tests/          Testes de integração e de aplicação
web/
  src/app/core/                        Sessão (AuthService), guarda de rota, interceptor HTTP
  src/app/login/                       Tela de login
  src/app/register/                    Tela de cadastro
  src/app/tasks/                       Lista, formulário (criar/editar) e serviço de tarefas
  proxy.conf.js                        Proxy do dev server para a API (ver "Como rodar")
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

## Frontend

Cliente web em `web/` que consome a API acima:

- **Cadastro** (`/register`) e **login** (`/login`) — após o cadastro, o app já autentica o usuário automaticamente (a API não retorna token no cadastro, então o cliente faz login em seguida com as mesmas credenciais).
- **Lista de tarefas** (`/tasks`, rota protegida) — estados de carregando, erro (com "tentar novamente") e lista vazia.
- **Criar e editar tarefa** (`/tasks/new`, `/tasks/:id/edit`) — mesmo formulário nos dois casos, com validação espelhando as regras da API.
- **Alterar status / concluir** e **excluir** (com confirmação) diretamente na lista.
- **Sessão**: token JWT em `localStorage`; qualquer resposta `401` de uma chamada autenticada encerra a sessão e volta ao login, preservando a rota que o usuário tentava acessar para retornar a ela após novo login.

Essas decisões (e os trade-offs registrados, como o uso de `localStorage` em vez de cookie `httpOnly`) estão documentadas em `design.md` das changes arquivadas — ver "Saiba mais" abaixo.

## Como rodar

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download) e [Node.js](https://nodejs.org/) (LTS recente, com npm).

### Backend

```bash
# build de toda a solução
dotnet build

# rodar a API (Swagger disponível em /swagger no ambiente de desenvolvimento)
dotnet run --project src/GerenciadorDeTarefas.Api --launch-profile http

# rodar os testes
dotnet test
```

> **Importante**: use sempre o perfil `http` (`--launch-profile http`, ou a run configuration "GerenciadorDeTarefas.Api: http" se estiver usando Rider/Visual Studio) — não o `https`. `launchSettings.json` define dois perfis; o `https` também abre a porta 7034, e o middleware `UseHttpsRedirection` passa a redirecionar toda chamada HTTP para lá. Como o proxy do frontend (abaixo) só repassa esse redirecionamento em vez de segui-lo, o navegador acaba tentando se conectar direto à porta HTTPS — e falha, porque o certificado de desenvolvimento não é confiável por padrão (`dotnet dev-certs https --trust` não foi configurado). Na prática: com o perfil `https` selecionado, o login (e todo o resto) para de funcionar no frontend.

### Frontend

```bash
cd web
npm install

# sobe em http://localhost:4200, com proxy para a API em /auth e /tasks
npm start

# rodar os testes
npm test
```

O `proxy.conf.js` encaminha `/auth` e `/tasks` para `http://localhost:5246` (a API rodando com o perfil `http` acima). A API precisa estar no ar antes de usar as telas de login, cadastro ou lista de tarefas.

## Saiba mais

Para ver o fluxo de Spec Driven Development em detalhe, explore:

- `openspec/specs/user-auth/spec.md`, `openspec/specs/task-management/spec.md` e `openspec/specs/task-management-ui/spec.md` — o que o sistema faz hoje.
- `openspec/changes/archive/` — como cada funcionalidade foi proposta, especificada, desenhada e implementada. A change `add-web-login-and-task-list` cobre a fatia inicial do frontend (login + lista); `add-material-design-and-full-crud`, em `openspec/changes/` (ainda não arquivada), cobre o Angular Material e o CRUD completo.
