## Why

A change anterior (`add-web-login-and-task-list`) entregou a fatia mínima do cliente web: login e visualização somente-leitura das próprias tarefas, deliberadamente sem CRUD, sem cadastro e sem biblioteca de UI. Com essa base validada, o cliente web agora cobre todos os endpoints restantes da API (cadastro, criar/editar/excluir/concluir tarefa) e adota Angular Material para dar consistência visual ao invés de CSS ad-hoc por tela.

## What Changes

- Tela de cadastro: formulário de e-mail/senha, chamada a `POST /auth/register`, tratamento de e-mail duplicado e de senha que não atende aos requisitos mínimos.
- Criação de tarefa: formulário (título, descrição, status) chamando `POST /tasks`, com validação de campo obrigatório e limites de tamanho refletidos na UI.
- Edição de tarefa: mesmo formulário pré-preenchido, chamando `PUT /tasks/{id}`.
- Conclusão/alteração de status de tarefa: ação rápida na lista (sem abrir o formulário completo) chamando `PUT /tasks/{id}`.
- Exclusão de tarefa: ação na lista com confirmação, chamando `DELETE /tasks/{id}`.
- Adoção do Angular Material (`@angular/material` + `@angular/cdk`) como biblioteca de componentes para todas as telas existentes e novas (login, cadastro, lista, formulário de tarefa), substituindo o CSS simples da change anterior.
- **BREAKING** (interno ao projeto, não à API): o CSS específico de tela escrito na change anterior (`login.css`, `task-list.css`) é removido/substituído por componentes Material; nenhuma URL, rota ou contrato de API muda.

## Capabilities

### New Capabilities
(nenhuma — esta change estende a capability de UI já existente)

### Modified Capabilities
- `task-management-ui`: adiciona cadastro de usuário e CRUD completo de tarefas (criar, editar, concluir/alterar status, excluir) como comportamento observável do cliente web; os requisitos já existentes (login, lista com estados de carregando/erro/vazio, logout, expiração de sessão) permanecem válidos e não mudam de comportamento.

## Impact

- Consome os endpoints já existentes `POST /auth/register`, `POST /tasks`, `PUT /tasks/{id}`, `DELETE /tasks/{id}` como estão; nenhuma mudança de contrato na API (.NET), portanto nenhuma alteração em `src/` ou `tests/`.
- Adiciona `@angular/material` e `@angular/cdk` como dependências de `web/`.
- Refatoração visual das telas de login e lista de tarefas (já existentes) para usar componentes Material, além das telas novas de cadastro e formulário de tarefa.
