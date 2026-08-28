## Why

O frontend Angular atual (login, cadastro, lista e formulário de tarefas) usa Angular Material com o tema padrão, sem alinhamento com nenhum design definido previamente. Um protótipo de alta fidelidade (`prototipo/`) já foi criado para as 4 telas do app — login, cadastro, lista de tarefas (com estados de carregando/erro/vazio) e formulário de tarefa —, com um visual "SaaS moderno" mobile-first. Queremos portar esse visual para os componentes Angular reais, usando Tailwind CSS + DaisyUI como a especificação original do protótipo (`prototipo/uploads/especificacao-prototipo-task-manager.md`) determinava, substituindo o Angular Material.

## What Changes

- Adicionar Tailwind CSS e DaisyUI ao projeto Angular (`web/`) como a base de estilização.
- **BREAKING** (interno ao frontend): remover a dependência do Angular Material (`@angular/material`, `@angular/cdk`) e todo uso de `mat-*` nos componentes existentes.
- Reimplementar os templates de Login, Cadastro, Lista de Tarefas e Formulário de Tarefa (novo/editar) seguindo a estrutura e o visual do protótipo em `prototipo/export-src/` (Login.html, Cadastro.html, Tarefas.html, FormularioTarefa.html), usando classes utilitárias Tailwind/DaisyUI (`card`, `input`, `btn`, `badge`, `modal`, etc.) em vez dos tokens de design proprietários do protótipo.
- Lista de tarefas passa a ser um grid responsivo de cards (em vez da lista vertical atual), com estado de carregamento via skeleton (DaisyUI `skeleton`) no lugar do texto "Carregando tarefas…".
- Diálogo de confirmação de exclusão (`confirm-delete-dialog`) reimplementado como modal DaisyUI, mantendo o mesmo comportamento (`MatDialog` → equivalente Angular sem Material, ex: CDK Overlay ou implementação própria).
- Campo de status no formulário de tarefa passa de `mat-select` (dropdown) para um controle segmentado (radio group estilizado), como no protótipo; o filtro de status na lista permanece um seletor inline por tarefa.
- Mensagens de validação inline passam a usar a marcação de erro do DaisyUI (`input-error` + texto de ajuda) em vez de `mat-error`.

### Non-goals (fora de escopo)

- O protótipo inclui, de forma puramente decorativa (sem handlers reais), um link "Esqueceu a senha?" com modal de recuperação simulada e botões "Continuar com Google/GitHub". Nenhum desses recursos tem suporte na API (`user-auth` só expõe `/auth/register` e `/auth/login`). Esta change **não** implementa recuperação de senha nem login social — esses elementos não serão portados para o Angular.
- Nenhuma mudança de comportamento observável pelo usuário: os fluxos de autenticação, CRUD de tarefas, mudança de status e exclusão continuam exatamente como especificado em `openspec/specs/task-management-ui/spec.md`. Esta change é puramente visual/de implementação (troca de biblioteca de UI), por isso não altera specs (`skip_specs: true`).

## Capabilities

### New Capabilities

_Nenhuma._

### Modified Capabilities

_Nenhuma — comportamento observável não muda; ver seção "Non-goals" acima. `skip_specs: true` está declarado em `.openspec.yaml`._

## Impact

- **Código afetado**: todos os componentes em `web/src/app/` (login, register, tasks/task-list, tasks/task-form, tasks/confirm-delete-dialog) e seus arquivos `.html`/`.css`/`.ts` de estilo; `web/src/styles.css`, `web/src/material-theme.scss` (removido), `web/angular.json` (build config).
- **Dependências**: remove `@angular/material`, `@angular/cdk`; adiciona `tailwindcss`, `daisyui` (e dependências associadas, ex. `postcss`, `autoprefixer` se necessário para a integração com Angular CLI).
- **Sem impacto no backend**: nenhuma mudança em `src/` ou `tests/GerenciadorDeTarefas.Tests`; os endpoints e contratos da API permanecem os mesmos.
- **CLAUDE.md**: a seção de arquitetura/stack do frontend precisa ser atualizada após a implementação (Angular Material → Tailwind + DaisyUI).
