## Why

O `README.md` ficou desatualizado em relação ao estado atual do projeto: a seção "Stack" ainda descreve o frontend como usando **Angular Material**, mas a change `redesign-frontend-tailwind-daisyui` (já arquivada) substituiu isso por **Tailwind CSS + DaisyUI**. A seção "Saiba mais" também lista a change `add-material-design-and-full-crud` como "ainda não arquivada", quando na verdade já está em `openspec/changes/archive/`, e não menciona a change de redesign mais recente. Como o README é a porta de entrada do projeto, essas divergências passam informação incorreta sobre a stack e o histórico de changes.

## What Changes

- Atualizar a seção "Stack" (Frontend) do `README.md`: trocar a menção a Angular Material (`@angular/material` + `@angular/cdk`) por Tailwind CSS + DaisyUI.
- Atualizar a seção "Saiba mais": corrigir a referência a `add-material-design-and-full-crud` (agora arquivada, não mais em andamento) e adicionar a menção à change `redesign-frontend-tailwind-daisyui`, que foi a que efetivamente trocou a biblioteca de UI.
- Revisar o restante do `README.md` em busca de outras menções remanescentes a Angular Material ou a informações desalinhadas com o estado atual do repositório, corrigindo o que for encontrado.

## Capabilities

### New Capabilities

_Nenhuma._

### Modified Capabilities

_Nenhuma — mudança é puramente documental, não altera comportamento observável de nenhuma capability. `skip_specs: true` está declarado em `.openspec.yaml`._

## Impact

- **Código afetado**: apenas `README.md`, na raiz do projeto.
- **Sem impacto em código de produção**: nenhuma mudança em `src/`, `web/` ou `tests/GerenciadorDeTarefas.Tests`.
