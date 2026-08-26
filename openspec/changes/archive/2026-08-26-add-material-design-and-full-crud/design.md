## Context

Ver proposal.md - Why para a motivação. Esta change estende o cliente Angular entregue em `add-web-login-and-task-list` (arquivado; ver `openspec/changes/archive/2026-08-26-add-web-login-and-task-list/design.md` para as decisões já tomadas que continuam valendo e não são reabertas aqui: standalone + signals sem NgRx, token em `localStorage`, 401 sempre encerra a sessão, proxy com `bypass` para `/auth` e `/tasks`).

Fatos verificados na API rodando localmente (porta HTTP `5246` / HTTPS `7034`), usados nas decisões abaixo em vez de deduzidos:

- `POST /auth/register` (sucesso): `201 Created`, corpo vazio — não retorna token. E-mail duplicado: `400` `{"title": "O email '...' já está em uso.", "status": 400, "instance": "/auth/register"}`. Senha/e-mail inválidos: `400` `ValidationProblemDetails` padrão (`errors: {Password: [...], Email: [...]}`).
- Regra de senha (`RegisterRequestValidator`): mínimo 8 caracteres, ao menos 1 maiúscula, 1 minúscula e 1 dígito. Sem exigência de caractere especial.
- `POST /tasks` (sucesso): `201 Created`, corpo `TaskDto`. Validação (`CreateTaskRequestValidator`): `title` obrigatório, máx. 200 caracteres; `description` opcional, máx. 2000; `status` deve ser um valor válido do enum. Erros: `400` `ValidationProblemDetails`.
- `PUT /tasks/{id}` (sucesso): `200 OK`, corpo `TaskDto` — é uma substituição completa (`title`, `description`, `status` todos exigidos pelo mesmo validador de criação), não um PATCH parcial. `404` (`{"title": "Tarefa '{id}' não encontrada.", "status": 404, "instance": "..."}`) se a tarefa não existir ou não pertencer ao usuário do token (mesmo tratamento para os dois casos, ver design da change anterior - Decisão 6 do backend).
- `DELETE /tasks/{id}` (sucesso): `204 No Content`. Mesmo `404` de `PUT` se não existir/não pertencer ao usuário.

## Goals / Non-Goals

**Goals:**
- Migrar as telas existentes (login, lista de tarefas) e adicionar as novas (cadastro, formulário de tarefa) usando Angular Material como biblioteca de componentes.
- Cobrir 100% dos endpoints da API a partir do cliente web: `POST /auth/register`, `POST /tasks`, `PUT /tasks/{id}`, `DELETE /tasks/{id}`, além dos já cobertos `POST /auth/login` e `GET /tasks`.

**Non-Goals:**
- Dark mode / alternância de tema — só o tema claro padrão do Material é configurado; a decisão anterior de não ter tema é revista apenas para "adotar uma biblioteca de componentes com tema consistente", não para oferecer alternância.
- Filtro, ordenação, busca ou paginação da lista de tarefas — continua fora de escopo (não solicitado).
- Tela de detalhe de tarefa separada da lista — o formulário de edição já cobre a necessidade de ver/alterar os campos.
- "Lembrar-me", refresh token, logout por inatividade — continuam fora de escopo (decisão da change anterior, não reaberta).
- SSR e testes end-to-end — continuam fora de escopo.

## Decisions

### 1. Angular Material via `ng add @angular/material`, versão compatível com o Angular já instalado (22.1.x)
`@angular/material@22.1.4` declara peer dependency `@angular/core: ^22.0.0 || ^23.0.0`, compatível com o projeto. O schematic `ng add` configura tema pré-definido, tipografia global e `provideAnimationsAsync()` automaticamente, evitando configuração manual dessas três partes.
**Alternativa considerada**: outra biblioteca (PrimeNG, Tailwind + componentes próprios). Descartada porque o usuário pediu Material especificamente.

### 2. Cadastro faz login automático após sucesso
Como `POST /auth/register` não retorna token (`201` vazio), após um cadastro bem-sucedido o `AuthService` chama `login()` internamente com as mesmas credenciais e navega para a lista de tarefas — o usuário não digita a senha uma segunda vez.
**Alternativa considerada**: após cadastro, redirecionar para a tela de login pré-preenchida com o e-mail, exigindo que o usuário digite a senha novamente. Descartada por adicionar fricção sem ganho de segurança relevante neste projeto de estudo (a senha acabou de ser validada e enviada em texto claro pelo mesmo cliente, na mesma requisição de rede).

### 3. Formulários migram para Reactive Forms (login incluso, por consistência)
Os formulários de cadastro e de tarefa (criar/editar) usam `ReactiveFormsModule` com `Validators` espelhando as regras verificadas da API (Contexto acima), exibindo erro assim que o campo perde o foco ou no submit. A tela de login (`FormsModule`/`ngModel` na change anterior) é migrada para o mesmo padrão, para não manter duas abordagens de formulário no projeto. Os componentes `mat-form-field`/`mat-error` do Material se integram diretamente com `FormControl`, o que é mais direto com Reactive Forms do que com template-driven.
**Alternativa considerada**: manter `ngModel` nos formulários novos. Descartada para não ter duas abordagens de formulário coexistindo sem motivo.

### 4. Formulário de tarefa único para criar/editar, em rota própria
Um único componente `TaskForm` é usado tanto em `/tasks/new` quanto em `/tasks/:id/edit` (o modo é determinado pela presença do parâmetro `:id`); no modo edição, o formulário é pré-preenchido buscando a tarefa via `GET /tasks/{id}`. Ambas as rotas são protegidas pela guarda de rota já existente.
**Alternativa considerada**: formulário em um `MatDialog` sobre a lista, em vez de rota própria. Descartada para manter URLs navegáveis/atualizáveis (F5 no formulário de edição continua funcionando, coerente com a decisão de proxy com `bypass` da change anterior) e por não haver necessidade de manter contexto da lista visível atrás do formulário.

### 5. Exclusão usa `MatDialog` de confirmação (não uma rota)
Diferente do formulário, a exclusão é uma ação binária rápida amarrada a um item específico da lista — um diálogo modal (`MatDialog`) pedindo confirmação é mais direto que navegar para uma rota separada, e a spec exige explicitamente que a ação só ocorra após confirmação.

### 6. Alterar status a partir da lista reenvia o objeto completo
Como `PUT /tasks/{id}` substitui `title`/`description`/`status` juntos (Contexto acima), a ação rápida de mudar status na lista envia o `title`/`description` já carregados da tarefa (inalterados) junto com o novo `status` — não existe endpoint de PATCH parcial na API.

### 7. Sem feedback via snackbar; a própria lista atualizada é a confirmação
Após criar, editar, excluir ou mudar o status de uma tarefa, a lista é recarregada (`GET /tasks`) e a mudança refletida nela é o único feedback — sem `MatSnackBar` de "tarefa criada com sucesso" etc. Mantém o escopo no que a spec pede (mudança refletida na lista) sem adicionar um mecanismo de feedback extra não solicitado.
**Alternativa considerada**: `MatSnackBar` de confirmação em cada operação. Descartada por não ser comportamento pedido e por adicionar estado (fila de snackbars, timing) sem necessidade.

### 8. Lista de tarefas usa `MatCard`/`MatList`, não `MatTable`
A lista continua sendo uma lista simples de itens com título, descrição e status (sem colunas, ordenação ou paginação — ver Non-Goals), então cartões/lista Material bastam; `MatTable` seria over-engineering para esse formato de dado.

## Risks / Trade-offs

- [Ícones do Material via Google Fonts (link externo no `index.html`)] → dependência de rede em desenvolvimento; aceitável pois o restante do projeto já depende de `npm install` com acesso à internet. Sem efeito em produção, pois não há build/deploy de produção no escopo deste projeto.
- [Login automático após cadastro envia a senha em texto claro em uma segunda requisição imediatamente após a primeira] → mesmo risco que o login manual já aceito na change anterior (HTTPS local via `dotnet run`); não é uma superfície nova.
- [Migrar login de `ngModel` para Reactive Forms toca uma tela que já funcionava] → mitigado por reescrever com o mesmo comportamento observável (specs de login não mudam nesta change) e por rodar novamente a verificação manual dos cenários de login já existentes após a migração.

## Migration Plan

Não aplicável — nenhuma mudança de contrato de API ou dado persistido. Rollout é local, como na change anterior (`ng serve` + proxy consumindo a API já em execução).
