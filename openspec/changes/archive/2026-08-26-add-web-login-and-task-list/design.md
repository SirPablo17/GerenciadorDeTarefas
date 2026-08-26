## Context

Ver proposal.md - Why para a motivação. Fatos verificados na API rodando localmente (porta HTTP `5246`, de `launchSettings.json`), usados para as decisões abaixo em vez de deduzidos:

- `POST /auth/login` (sucesso, 200): `{"token": string, "expiresAt": string (ISO 8601 UTC)}`.
- `POST /auth/login` (credencial inválida, 401): corpo `{"title": string, "status": 401, "instance": "/auth/login"}` — envelope próprio do `ExceptionHandlingMiddleware`, sem campo `errors`.
- `POST /auth/login` (entrada inválida — e-mail vazio/malformado, senha vazia, 400): `ValidationProblemDetails` padrão do ASP.NET (`{"type", "title": "One or more validation errors occurred.", "status": 400, "errors": {"Email": [...], "Password": [...]}, "traceId"}`). Ambos os corpos de erro (400 e 401) têm um campo `title` em comum.
- `GET /tasks` sem token ou com token inválido/expirado (401): corpo **vazio**, apenas o header `WWW-Authenticate: Bearer` — não passa pelo `ExceptionHandlingMiddleware` (falha de autenticação do middleware JWT, não uma exceção). Ou seja, não há mensagem para exibir nesse caso; a UI só tem o código 401.
- `GET /tasks` (sucesso, 200): array de `TaskDto` `{id, title, description, status: 0|1|2, createdAt, updatedAt}` (`status`: 0=Pending, 1=InProgress, 2=Completed), já filtrado pelo usuário do token — a UI não precisa (nem deve) filtrar por usuário no cliente.

## Goals / Non-Goals

**Goals:**
- Definir a arquitetura mínima do cliente Angular (estado de sessão, guarda de rota, interceptor HTTP, proxy de dev) necessária para os cenários em `specs/task-management-ui/spec.md`.
- Deixar registradas as decisões já tomadas pelo usuário (ver proposal e itens abaixo), sem reabri-las.

**Non-Goals:**
- Qualquer decisão de UI visual (biblioteca de componentes, tema) — fora de escopo da change (ver proposal).
- Estrutura de testes do frontend (unitários/e2e) — fora de escopo desta change.
- Empacotamento/deploy de produção do `web/` — escopo é desenvolvimento local (`ng serve` + proxy).

## Decisions

### 1. Angular standalone components + signals, sem NgRx
Decisão do usuário, não reaberta aqui. O estado de sessão (token, `expiresAt`, flag "autenticado") vive em um serviço único (`AuthService`) usando `signal`/`computed`; o estado da lista de tarefas (carregando/erro/dados) vive em um serviço/estado local à tela de tarefas, também com signals. Sem store global (NgRx) porque o escopo desta fatia (duas telas, um recurso de leitura) não justifica o boilerplate.

### 2. Projeto Angular em `web/`, fora da solution .NET
Decisão do usuário. `web/` é um projeto Node/Angular independente, sem referência na `.slnx`; nenhuma alteração em `src/` ou `tests/` (.NET) nesta change.

### 3. Token em `localStorage`
Decisão do usuário. Ao logar com sucesso, `token` e `expiresAt` são gravados em `localStorage`; `AuthService` lê o valor ao inicializar para restaurar a sessão entre reloads.
**Trade-off aceito conscientemente (registrar, não reabrir)**: `localStorage` é acessível a qualquer script executado na página, então um XSS na aplicação permite roubo do token — diferente de um cookie `httpOnly`, inacessível a JavaScript. Não se usou cookie `httpOnly` porque (a) a API não emite `Set-Cookie` nem está preparada para autenticação via cookie/CSRF, o que exigiria mudança de contrato na API — fora do escopo desta change (ver proposal - Impact); e (b) é um projeto de estudo, onde o objetivo desta fatia é validar o fluxo cliente-servidor, não endurecer a aplicação contra XSS. Registrado aqui como dívida técnica consciente, não como lacuna não percebida.

### 4. 401 sempre encerra a sessão (sem refresh)
Decisão do usuário, refletindo um fato da API: não há endpoint de refresh token. Um interceptor HTTP intercepta qualquer resposta `401` de qualquer chamada autenticada e trata isso como "sessão inválida": limpa o estado local (token, dados de tarefas) e redireciona ao login. Como `GET /tasks` retorna 401 com corpo vazio (ver Context), o interceptor não depende do corpo da resposta para decidir — só do status code. Isso cobre tanto token expirado quanto token malformado/adulterado com o mesmo caminho de código.

### 5. Erro de login exibido a partir do campo `title`
Tanto a resposta 401 (credencial inválida) quanto a 400 (entrada inválida, ex.: e-mail malformado) trazem um campo `title` de texto pronto para exibição (ver Context). A tela de login exibe `error.error.title` quando presente, com uma mensagem genérica de fallback caso a chamada falhe por outro motivo (ex.: rede indisponível). Isso evita acoplar a UI ao formato por-campo de `ValidationProblemDetails.errors`, que não é necessário para o comportamento pedido nesta change (mensagem clara, sem detalhamento por campo).
**Alternativa considerada**: renderizar erros por campo a partir de `errors`. Descartada por não ser um comportamento observável pedido nesta change (ver spec) e por acoplar a UI a um formato de erro que não é o mesmo entre 400 e 401.

### 6. Guarda de rota com retorno à URL original
Um `CanActivateFn` (guarda funcional) verifica o estado de sessão do `AuthService` antes de ativar qualquer rota protegida. Sem sessão, redireciona para `/login` guardando a URL solicitada (query param `returnUrl` ou `router.getCurrentNavigation()` + `state`). Esse mesmo mecanismo cobre tanto "acesso direto sem sessão" quanto "sessão expirou durante o uso" (o interceptor do item 4 dispara a mesma navegação para `/login`, preservando a URL em que o usuário estava). Após login bem-sucedido, a tela de login navega para `returnUrl` se presente, senão para a rota padrão (lista de tarefas).

### 7. Logout limpa estado e evita reexibição via "voltar" do navegador
Logout: limpa `localStorage` (token/expiresAt), zera os signals de sessão e de tarefas carregadas, e navega para `/login` substituindo a entrada do histórico (`Router.navigate(['/login'], { replaceUrl: true })`) em vez de empilhar. A guarda de rota (item 6) garante que, mesmo que o botão "voltar" leve a uma entrada de histórico de uma rota protegida, a ausência de sessão redireciona de novo ao login antes de renderizar qualquer dado — como as telas são componentes Angular que buscam dados do zero ao ativar (sem cache persistente fora do `AuthService`), não há dado de tarefa remanescente em memória para reexibir.

### 8. Proxy do dev server com bypass para `/auth` e `/tasks`
Fato verificado: a API responde em `/auth/*` e `/tasks/*` na raiz, sem prefixo `/api` (ver Context / launchSettings.json, porta `5246`). Isso colide em path com a possível rota Angular da tela de tarefas. `proxy.conf.json` declara `context: ["/auth", "/tasks"]` apontando para `http://localhost:5246`, com uma função `bypass` que deixa o dev server (Angular) servir a requisição — em vez de proxiá-la à API — quando o `Accept` da requisição indica navegação de página (`text/html`), e só encaminha à API chamadas XHR/fetch (`Accept: application/json` ou sem header de navegação). Isso permite que a rota Angular da lista de tarefas exista em `/tasks` sem que um reload de página nessa URL seja capturado pelo proxy e enviado à API.
**Alternativa considerada**: nomear a rota Angular da lista de tarefas com outro path (ex.: `/`) para nunca colidir com `/tasks`. Descartada por ser workaround de nomenclatura em vez de resolver a causa (o dev server tratando `/tasks` como um path só de API); o bypass resolve isso independentemente de como as rotas Angular forem nomeadas no futuro.

## Risks / Trade-offs

- [Token em `localStorage` vulnerável a XSS] → aceito conscientemente para este projeto de estudo; ver Decisão 3 para o raciocínio completo.
- [Sem refresh token, logout forçado a cada expiração] → aceitável: é o contrato atual da API (fora do escopo desta change alterar), e o retorno à rota original após novo login (Decisão 6) reduz o atrito.
- [Mensagem de erro de login não detalha por campo] → aceitável: não é um comportamento pedido nesta change; pode ser revisitado numa change futura se a UX exigir.
- [`bypass` no proxy é específico de desenvolvimento local] → sem efeito em produção, pois não há build/deploy de produção no escopo desta change (ver Non-Goals).

## Migration Plan

Não aplicável — capability nova, sem dado existente para migrar. Rollout é local: `ng serve` consumindo a API já em execução (`dotnet run --project src/GerenciadorDeTarefas.Api`) via o proxy da Decisão 8.

## Open Questions

- Versão exata do Angular CLI/framework a usar no scaffold (`ng new`). Não afeta as specs, a abordagem ou a quebra de tasks — decidir no momento da implementação, usando a versão estável mais recente disponível no ambiente.
