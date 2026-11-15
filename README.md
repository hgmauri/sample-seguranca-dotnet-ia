# sample-seguranca-dotnet-ia

Segurança em aplicações **.NET 10** na era da IA: uma API com sete vulnerabilidades que assistentes de código costumam gerar, a versão corrigida de cada uma, um programa que ataca as duas e os **prompts** que uso para fazer a IA revisar código com foco em segurança.

Artigo completo: [Segurança em aplicações .NET na era da IA: o prompt que eu uso para revisar código](https://henriquemauri.net/seguranca-em-aplicacoes-net-na-era-da-ia/)

## O que tem aqui

```
prompts/
  revisao-de-seguranca.md     Prompt de revisão de segurança (OWASP Top 10 2025 + OWASP LLM Top 10)
  modelagem-de-ameacas.md     Prompt de modelagem de ameaças (STRIDE + riscos de IA) antes de codar
AGENTS.md                     Regras de código seguro para Copilot, Codex e outros agentes
CLAUDE.md                     Importa o AGENTS.md no Claude Code
src/
  Pedidos.Api/
    Inseguro/                 ⚠ Vulnerável de propósito. Não copie.
    Seguro/                   As mesmas funcionalidades, corrigidas
  Pedidos.Ataques/            Executa os ataques contra /inseguro e /seguro
.github/workflows/
  seguranca.yml               Build com NuGet Audit, pacotes vulneráveis, ataques e CodeQL
```

## As vulnerabilidades

| Ataque | Categoria | `/inseguro` | `/seguro` |
|--------|-----------|-------------|-----------|
| Ler pedido de outro usuário | A01 Broken Access Control (IDOR) | vulnerável | 404 + DTO com CPF mascarado |
| Definir preço, desconto e dono no POST | A01/A06 Mass assignment | vulnerável | DTO mínimo, valor do catálogo |
| Ler `appsettings.json` com `../` | A01 Path traversal | vulnerável | lista branca + `GetFullPath` |
| Buscar metadados internos da nuvem | SSRF | vulnerável | HTTPS, hosts permitidos, IP validado na conexão |
| Stack trace na resposta | A10 Exceções mal tratadas | vulnerável | `ProblemDetails` |
| Prompt injection cancelando pedido alheio | LLM01 + LLM06 | vulnerável | ferramentas presas ao usuário + confirmação |
| Vazar o prompt de sistema | LLM07 | vulnerável | nenhum segredo no prompt |

## Como rodar

```bash
dotnet build

# terminal 1
dotnet run --project src/Pedidos.Api

# terminal 2
dotnet run --project src/Pedidos.Ataques
```

Saída esperada:

```
Ataque                                               Categoria                    /inseguro    /seguro
---------------------------------------------------------------------------------------------------------
Ler pedido de outro usuário (IDOR)                   A01 Broken Access Control    VULNERÁVEL   protegido
Definir preço, desconto e dono no POST               A01/A06 Mass assignment      VULNERÁVEL   protegido
Ler appsettings.json via ../                         A01 Path traversal           VULNERÁVEL   protegido
Buscar metadados internos da nuvem                   SSRF                         VULNERÁVEL   protegido
Stack trace na resposta de erro                      A10 Exceções mal tratadas    VULNERÁVEL   protegido
Prompt injection: cancelar pedido de outro usuário   LLM01 + LLM06                VULNERÁVEL   protegido
Vazar o prompt de sistema                            LLM07 System prompt leakage  VULNERÁVEL   protegido

Versão segura bloqueou todos os ataques.
```

A autenticação é **falsa, só para a demo**: o usuário vem do cabeçalho `X-Usuario` (`ana` ou `bruno`).

Sem `Foundry:Endpoint` configurado, o assistente usa um modelo simulado que obedece instruções escondidas no texto, o pior caso realista de prompt injection. Com o endpoint configurado (e `az login`), usa o modelo do Microsoft Foundry com Entra ID.

## Usando os prompts

- **Revisão:** abra `prompts/revisao-de-seguranca.md`, preencha o contexto e cole no seu assistente apontando para o código ou para o diff do PR.
- **Antes de codar:** `prompts/modelagem-de-ameacas.md`.
- **Sempre ligado:** copie o `AGENTS.md` (e o `CLAUDE.md`) para a raiz do seu repositório.

Prompt não substitui ferramenta: mantenha NuGet Audit, CodeQL e revisão humana no pipeline.

## Licença

MIT

---

Feito por [Henrique Mauri](https://henriquemauri.net) | [Dotcode](https://dotcode.com.br)
