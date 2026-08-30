## Why

A funcionalidade de tarefas hoje é simples demais para o próximo passo de UX: o frontend vai precisar de uma aba dedicada de "Concluído", separando tarefas concluídas das ativas, e os usuários precisam de um número de tarefa legível (em vez do GUID interno) para referenciar suas tarefas. Esta mudança prepara o backend para essas duas necessidades, sem tocar no frontend ainda.

## What Changes

- Ao criar uma tarefa, o sistema passa a gerar automaticamente um **número sequencial por usuário** (1, 2, 3... reiniciando para cada usuário), retornado na resposta da tarefa. O número é imutável e nunca reaproveitado, mesmo após exclusões.
- O endpoint de listagem de tarefas (`GET /tasks`) passa a aceitar um **filtro opcional por status** (`?status=Completed`, etc.), permitindo consultar somente tarefas concluídas ou somente as ativas — a base para a futura aba "Concluído" no frontend. Sem o filtro, o comportamento atual (retornar todas as tarefas do usuário) é preservado.
- Fora de escopo: nenhuma mudança no frontend (`web/`). A aba "Concluído" em si (UI) fica para uma mudança futura.

## Capabilities

### New Capabilities
_Nenhuma capacidade nova — a mudança se enquadra inteiramente em `task-management`._

### Modified Capabilities
- `task-management`: a criação de tarefa passa a gerar um número sequencial por usuário; a listagem de tarefas passa a suportar filtro opcional por status.

## Impact

- **Domain**: `TaskItem` ganha um campo `Number` (int); `User` (ou nova entidade de suporte) precisa rastrear o próximo número a emitir por usuário para geração seguro contra concorrência.
- **Infrastructure**: migração EF Core para a nova coluna/contador; `ITaskRepository`/`IUserRepository` (ou repositório dedicado) precisam suportar a geração atômica do número e o filtro por status na consulta.
- **Application**: `TaskService.CreateAsync` passa a atribuir o número; `TaskDto` ganha o campo `Number`; `ITaskService.ListByUserAsync` ganha parâmetro opcional de status.
- **Api**: `TasksController.List` passa a aceitar query string `status`.
- **Tests**: novos testes unitários para geração de número (incluindo casos de múltiplas tarefas do mesmo usuário e isolamento entre usuários) e para o filtro de status na listagem.
