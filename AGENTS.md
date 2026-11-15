# Regras de código seguro para agentes de IA

Este arquivo é lido por agentes de código (Claude Code via `CLAUDE.md`, GitHub Copilot, Codex e outros que suportam `AGENTS.md`).
As regras valem para todo código gerado ou alterado neste repositório. A pasta `src/Pedidos.Api/Inseguro` é **vulnerável de propósito**: nunca copie código de lá.

## Antes de escrever código

- Diga quais pontos de entrada e quais dados sensíveis a mudança toca.
- Se a tarefa envolver autenticação, autorização, criptografia, upload, URL vinda do usuário ou IA com ferramentas, explique o modelo de ameaça em até 5 linhas antes do código.

## Acesso a dados e autorização

- Toda consulta por id filtra pelo usuário ou tenant atual. O id do usuário vem do token (`ClaimsPrincipal`), nunca do corpo ou da query.
- Endpoint novo nasce com `RequireAuthorization()`. Acesso anônimo só quando a tarefa pedir explicitamente.
- Registro de outro usuário responde 404, não 403.

## Entrada e saída

- Endpoints recebem DTOs com só os campos que o cliente pode definir. Nunca receba entidade do banco direto.
- Preço, desconto, dono, status e permissões são calculados no servidor.
- Valide tamanho, formato e intervalo de toda entrada.
- Respostas usam DTOs. Dado pessoal (CPF, e-mail, telefone) sai mascarado quando não for necessário por inteiro.

## Injeção

- SQL só parametrizado: `FromSql`/`FromSqlInterpolated`, LINQ, ou parâmetros no Dapper/ADO.NET. Nunca `FromSqlRaw` com interpolação ou concatenação.
- Caminho de arquivo com entrada do usuário: lista branca de nomes e `Path.GetFullPath` conferindo que o resultado fica dentro da pasta base.
- URL informada pelo usuário: só HTTPS, lista de hosts permitidos, bloqueio de IP interno no momento da conexão e sem redirect automático.
- Nunca chame `Process.Start` com entrada do usuário.

## Segredos e criptografia

- Nenhum segredo em código, `appsettings.json`, log ou prompt. Use Key Vault, user secrets e Managed Identity.
- Token e código aleatório: `RandomNumberGenerator`. Senha: `PasswordHasher<T>` do ASP.NET Core Identity ou algoritmo dedicado (PBKDF2, Argon2). Nunca MD5 ou SHA1.

## Erros e logs

- Erro para o cliente é `ProblemDetails` genérico. Stack trace e mensagem interna só no log.
- Nunca registre senha, token, CPF completo ou cartão.
- Falha em validação de segurança nega o acesso (fail closed).

## IA e agentes

- Instruções do sistema são fixas. Texto do usuário e conteúdo externo entram como mensagem separada e são tratados como dado.
- Nenhum segredo, chave ou regra sensível no prompt de sistema. Assuma que ele vai vazar.
- Ferramentas do modelo operam só sobre dados do usuário atual e com o menor privilégio possível.
- Ação com efeito colateral (cancelar, pagar, enviar, apagar) vira solicitação que uma pessoa confirma.
- Saída do modelo é texto não confiável: codifique ao exibir, valide antes de usar como SQL, URL, comando ou JSON.
- Limite tokens (`MaxOutputTokens`), iterações de ferramentas e requisições por usuário.

## Dependências

- Só adicione pacote que você consegue confirmar que existe no nuget.org, com o nome exato e a versão. Se tiver dúvida, pergunte.
- Versão fixa, nunca flutuante (`*`). O build roda com NuGet Audit e falha com vulnerabilidade moderada ou maior.

## Antes de concluir

- Rode `dotnet build` e o projeto `src/Pedidos.Ataques` contra a API e confira que a versão segura continua bloqueando todos os ataques.
- Liste o que você não conseguiu verificar.
