## Why

O backend já suporta número sequencial por tarefa e filtro por status (`implementar-funcionalidade-concluido`, arquivada), mas o frontend ainda não expõe nada disso: todas as tarefas aparecem juntas numa única lista, sem separar as concluídas, e o número da tarefa não é exibido em lugar nenhum. Esta mudança leva essas duas capacidades do backend até a tela de tarefas.

## What Changes

- A tela de tarefas passa a ter **duas abas**: "Ativas" (tarefas Pendente + Em andamento, comportamento atual) e "Concluídas" (tarefas com status Concluída). Ao mudar o status de uma tarefa para Concluída pelo seletor já existente no card, ela some da aba Ativas e passa a aparecer na aba Concluídas (e vice-versa, se o status for revertido).
- Cada card de tarefa passa a exibir o **número da tarefa** (ex.: "#3") retornado pelo backend.
- Cada aba tem seus próprios estados de carregamento/erro/vazio, reaproveitando o padrão já existente na lista única de hoje (loading, erro com retry, vazio).
- Fora de escopo: qualquer mudança no backend (já implementada); mudar a forma como o formulário de criação/edição de tarefa funciona; paginação ou ordenação além do que já existe.

## Capabilities

### New Capabilities
_Nenhuma capacidade nova — a mudança se enquadra inteiramente em `task-management-ui`._

### Modified Capabilities
- `task-management-ui`: a tela de tarefas passa a separar tarefas ativas e concluídas em abas, e a exibir o número da tarefa em cada card.

## Impact

- **Frontend models**: `web/src/app/core/models.ts` — `TaskDto` ganha o campo `number`.
- **Frontend service**: `web/src/app/tasks/tasks.service.ts` — mantém `load()` buscando todas as tarefas do usuário (sem depender do filtro `?status=` do backend); a separação em Ativas/Concluídas acontece no cliente a partir da lista já carregada.
- **Frontend UI**: `web/src/app/tasks/task-list/` (`task-list.ts`/`.html`) — adiciona a navegação por abas, os estados de loading/erro/vazio por aba, e a exibição do número da tarefa no card.
- **Tests**: novos specs Vitest para a lógica de separação por aba e para os estados vazio/carregando/erro de cada aba.
