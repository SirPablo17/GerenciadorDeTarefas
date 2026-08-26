## Why

A API (`user-auth`, `task-management`) hoje só é acessível via Swagger ou HTTP direto. Não existe frontend. Esta change entrega a primeira fatia vertical de um cliente web Angular — login e visualização das próprias tarefas — validando de ponta a ponta a integração com a API (autenticação, roteamento protegido, tratamento de sessão) antes de construir qualquer funcionalidade de escrita (criar/editar/concluir tarefa).

## What Changes

- Novo projeto Angular standalone em `web/`, fora da solution .NET.
- Tela de login: formulário de e-mail/senha, chamada a `POST /auth/login`, tratamento de credencial inválida com mensagem e limpeza do campo de senha.
- Guarda de rota que redireciona para o login quando não há sessão ativa.
- Tela de lista de tarefas: chamada a `GET /tasks`, com estados de carregando, erro (com nova tentativa) e lista vazia.
- Interceptor HTTP que anexa o token às requisições e trata resposta `401` encerrando a sessão e redirecionando ao login, preservando a rota de destino para retorno pós-login.
- Ação de logout que limpa o estado da sessão (token e dados carregados) e impede que o botão voltar do navegador reexiba a lista após sair.
- Proxy do dev server configurado para encaminhar `/auth` e `/tasks` à API sem colidir com o roteamento do Angular.

## Capabilities

### New Capabilities
- `task-management-ui`: comportamento observável do cliente web — autenticação (login/logout, sessão, expiração de token) e visualização da lista de tarefas do usuário autenticado.

### Modified Capabilities
(nenhuma — `user-auth` e `task-management` descrevem o comportamento da API e não mudam de contrato nesta change)

## Impact

- Novo diretório `web/` (projeto Angular), sem alteração em `src/` ou `tests/` (API .NET).
- Consome os endpoints existentes `POST /auth/login` e `GET /tasks` como estão; nenhuma mudança de contrato na API.
- Introduz dependência de tooling de frontend (Node/Angular CLI) ao processo de desenvolvimento local, documentada em design.md.
