## 1. Scaffold do projeto Angular

- [x] 1.1 Criar o projeto Angular standalone em `web/` (fora da solution .NET) e verificar que `ng serve` sobe a aplicação padrão sem erros
- [x] 1.2 Configurar `proxy.conf.json` com `context: ["/auth", "/tasks"]` apontando para a API (`http://localhost:5246`) e uma função `bypass` que deixa o dev server servir requisições de navegação (`Accept: text/html`) em vez de proxiá-las; verificar com `curl -H "Accept: application/json" http://localhost:4200/tasks` (deve chegar à API, 401 sem sessão) e com um reload de página em `http://localhost:4200/tasks` no navegador (deve renderizar a SPA, não um erro da API)
- [x] 1.3 Definir as rotas base (`/login`, rota protegida da lista de tarefas) no roteador standalone e verificar que ambas resolvem sem erros de configuração

## 2. Estado de sessão e cliente HTTP

- [x] 2.1 Implementar `AuthService` com signals para token/`expiresAt`/estado autenticado, incluindo leitura inicial de `localStorage` na inicialização, e verificar com um teste unitário que o estado autenticado reflete a presença de um token salvo
- [x] 2.2 Implementar chamada a `POST /auth/login` no `AuthService`, persistindo `token`/`expiresAt` em `localStorage` e atualizando os signals em caso de sucesso; verificar com um teste unitário (HTTP mockado) que uma resposta 200 atualiza o estado de sessão
- [x] 2.3 Implementar interceptor HTTP que anexa `Authorization: Bearer <token>` às chamadas para `/tasks` e trata qualquer resposta `401` limpando a sessão (`AuthService`) e redirecionando para `/login` com a URL atual preservada (ex.: `returnUrl`); verificar com um teste unitário que um `401` simulado dispara a navegação para `/login`
- [x] 2.4 Implementar logout no `AuthService` (limpa `localStorage`, zera signals de sessão e de tarefas) navegando para `/login` com `replaceUrl: true`; verificar com um teste unitário que, após logout, o estado autenticado é `false` e `localStorage` não contém mais o token

## 3. Guarda de rota

- [x] 3.1 Implementar `CanActivateFn` que bloqueia rotas protegidas sem sessão ativa, redirecionando para `/login` com a URL solicitada preservada, e aplicá-lo à rota da lista de tarefas; verificar com um teste unitário que a guarda nega ativação sem sessão e permite com sessão
- [x] 3.2 Verificar manualmente (app rodando): acessar a URL da lista de tarefas sem sessão ativa leva à tela de login (cenário "Protected screen visited without a session" da spec)

## 4. Tela de login

- [x] 4.1 Implementar o formulário de login (e-mail, senha) chamando `AuthService`, navegando para `returnUrl` (ou rota padrão da lista de tarefas) em caso de sucesso; verificar manualmente que login com credenciais válidas leva à lista de tarefas
- [x] 4.2 Exibir mensagem de erro a partir de `error.error.title` da resposta da API (com mensagem de fallback genérica quando ausente) e limpar o campo de senha, mantendo o e-mail digitado, quando a chamada de login falhar; verificar manualmente com uma senha incorreta que a mensagem aparece, o campo de senha fica vazio e o usuário permanece na tela

## 5. Lista de tarefas

- [x] 5.1 Implementar a busca de `GET /tasks` ao ativar a tela, com estado de carregamento exibido enquanto a resposta não chega; verificar manualmente (rede lenta simulada via devtools) que o indicador de carregamento aparece antes dos dados
- [x] 5.2 Implementar exibição da lista de tarefas retornada, e de uma mensagem de estado vazio quando a resposta for uma lista vazia; verificar manualmente com um usuário sem tarefas e com um usuário com tarefas (ver Impact/Context: `POST /tasks` pode ser usado via Swagger para popular dados de teste, já que criar tarefa está fora do escopo da UI nesta change)
- [x] 5.3 Implementar estado de erro com opção de "tentar novamente" quando `GET /tasks` falhar (exceto 401, tratado pelo interceptor), disparando nova busca ao ser acionado; verificar manualmente derrubando a API durante o carregamento e confirmando que o botão de retry busca novamente ao subir a API de volta

## 6. Sessão expirada e retorno à rota original

- [x] 6.1 Verificar manualmente: com a lista de tarefas aberta, invalidar o token salvo em `localStorage` (editar via devtools) e disparar uma nova busca (ex.: retry) — confirmar que a aplicação encerra a sessão e mostra a tela de login (cenário "Session expires while viewing the task list")
- [x] 6.2 Verificar manualmente: a partir do estado do passo 6.1, logar novamente com credenciais válidas e confirmar que a aplicação retorna à lista de tarefas (a rota originalmente solicitada), não a uma rota padrão diferente

## 7. Logout e histórico do navegador

- [x] 7.1 Verificar manualmente: autenticado na lista de tarefas, clicar em "Sair" e confirmar que a aplicação volta à tela de login com a sessão e os dados de tarefas limpos
- [x] 7.2 Verificar manualmente: após o logout do passo 7.1, usar o botão "voltar" do navegador e confirmar que a lista de tarefas ou qualquer dado da sessão anterior não é reexibido (a navegação para trás deve levar novamente à tela de login, via guarda de rota)
