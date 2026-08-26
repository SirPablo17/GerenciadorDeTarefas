## 1. Angular Material

- [x] 1.1 Rodar `ng add @angular/material` em `web/` (tema pré-definido, tipografia global, animações) e verificar que `ng build` continua limpo e que `provideAnimationsAsync()` (ou equivalente adicionado pelo schematic) aparece em `app.config.ts`
- [x] 1.2 Adicionar a fonte de ícones do Material (link no `index.html`, conforme o schematic oferece) e verificar visualmente que um `mat-icon` renderiza um ícone, não o nome em texto

## 2. Login e lista de tarefas: migração visual e de formulário

- [x] 2.1 Migrar o formulário de login de `FormsModule`/`ngModel` para `ReactiveFormsModule`, preservando o comportamento existente (campo de senha limpo e mensagem de erro em credencial inválida), usando `mat-form-field`/`mat-error`; verificar manualmente os cenários de login já existentes (credenciais válidas → lista; credenciais inválidas → mensagem clara, senha limpa, e-mail mantido)
- [x] 2.2 Migrar a tela de lista de tarefas para `MatCard`/`MatList` (um card ou item por tarefa) mantendo os estados de carregando/erro-com-retry/vazio já existentes; verificar manualmente que os três estados continuam se comportando como antes (cenários da spec já cobertos na change anterior)
- [x] 2.3 Migrar o cabeçalho da lista (título + botão "Sair") para `MatToolbar`/`MatButton` e verificar visualmente que o logout continua funcionando

## 3. Cadastro de usuário

- [x] 3.1 Implementar `AuthService.register()` chamando `POST /auth/register`; em caso de sucesso, encadear uma chamada a `login()` com as mesmas credenciais (login automático); verificar com um teste unitário (HTTP mockado) que uma resposta 201 de registro é seguida por uma chamada a `POST /auth/login` com o mesmo e-mail/senha
- [x] 3.2 Implementar a tela de cadastro (`/register`) com Reactive Forms e Material: e-mail, senha, validação client-side espelhando as regras da API (mínimo 8 caracteres, 1 maiúscula, 1 minúscula, 1 dígito); em sucesso, navegar para a lista de tarefas; verificar manualmente o cadastro de um usuário novo até a lista de tarefas aparecer, sem precisar digitar a senha de novo
- [x] 3.3 Exibir mensagem de erro a partir de `error.error.title` quando o cadastro falhar (e-mail duplicado ou senha/e-mail inválidos vindos do servidor, complementando a validação client-side), mantendo o usuário na tela; verificar manualmente cadastrando duas vezes o mesmo e-mail e confirmando a mensagem "já está em uso"
- [x] 3.4 Adicionar link de navegação entre login e cadastro (`/login` ↔ `/register`) e verificar manualmente que ambos os links navegam corretamente

## 4. Criar e editar tarefa

- [x] 4.1 Implementar `TasksService.create()` (`POST /tasks`) e `TasksService.update()` (`PUT /tasks/{id}`, enviando `title`/`description`/`status` completos); verificar com testes unitários (HTTP mockado) que cada método faz a chamada correta e retorna/propaga sucesso e erro de validação (400)
- [x] 4.2 Implementar o componente `TaskForm` compartilhado entre `/tasks/new` e `/tasks/:id/edit`, com Reactive Forms e Material (`mat-form-field` para título/descrição, `mat-select` para status), protegido pela guarda de rota existente; no modo edição, carregar a tarefa via `GET /tasks/{id}` para pré-preencher o formulário; verificar manualmente que abrir `/tasks/:id/edit` de uma tarefa existente mostra os campos já preenchidos
- [x] 4.3 Implementar submissão do formulário: em sucesso, voltar para a lista de tarefas com a tarefa criada/editada visível; em erro de validação (400), manter o usuário no formulário exibindo mensagem clara por campo ou geral; verificar manualmente os cenários "Successful task creation", "Invalid task input rejected", "Successful task edit" e "Invalid task edit rejected" da spec
- [x] 4.4 Adicionar ação "Nova tarefa" na tela de lista navegando para `/tasks/new`, e ação "Editar" em cada item da lista navegando para `/tasks/:id/edit`; verificar manualmente que ambas as ações navegam para o formulário correto

## 5. Alterar status e excluir tarefa

- [x] 5.1 Implementar a ação de mudar status diretamente na lista (ex.: `mat-select` por item, reenviando `title`/`description` já carregados junto com o novo `status` via `TasksService.update()`), atualizando a lista após sucesso; verificar manualmente o cenário "Changing status from the task list" da spec, incluindo marcar uma tarefa como concluída
- [x] 5.2 Implementar `TasksService.remove()` (`DELETE /tasks/{id}`) e verificar com um teste unitário (HTTP mockado) que a chamada é feita corretamente e trata sucesso (204) e erro (404)
- [x] 5.3 Implementar a ação "Excluir" em cada item da lista abrindo um `MatDialog` de confirmação; ao confirmar, chamar `TasksService.remove()` e atualizar a lista; ao cancelar, não fazer nenhuma chamada; verificar manualmente os cenários "Confirmed deletion" e "Cancelled deletion" da spec

## 6. Verificação end-to-end

- [x] 6.1 Com a API rodando, percorrer manualmente o fluxo completo em uma sessão: cadastrar um usuário novo → criar uma tarefa → editar essa tarefa → mudar seu status pela lista → excluir a tarefa (confirmando) → sair (logout); confirmar que a lista reflete cada mudança corretamente e volta vazia no fim
- [x] 6.2 Rodar `ng test` e confirmar que todos os testes (os já existentes da change anterior e os novos desta change) passam
- [x] 6.3 Rodar `ng build` (produção) e confirmar que o bundle gera sem erros nem warnings novos
