# Prompt: modelagem de ameaças antes de codar

> Use antes de implementar uma funcionalidade nova. Leva cinco minutos e evita a maior parte das falhas de design.

Você é um engenheiro de segurança de aplicações. Antes de qualquer código, faça a modelagem de ameaças da funcionalidade abaixo.

**Funcionalidade:** <descreva em 3 a 5 linhas: quem usa, o que faz, quais dados entram e saem, se usa IA e com quais ferramentas>

**Arquitetura atual:** <API ASP.NET Core, banco, filas, serviços externos, modelo no Microsoft Foundry...>

Entregue, em português e de forma objetiva:

1. **Ativos:** o que precisa ser protegido (dados, ações, dinheiro, reputação).
2. **Fronteiras de confiança:** onde entra dado não confiável (usuário, outro sistema, documento, resposta do modelo).
3. **Ameaças por STRIDE** (Spoofing, Tampering, Repudiation, Information disclosure, Denial of service, Elevation of privilege). Para cada uma: cenário concreto, probabilidade e impacto (alto, médio ou baixo).
4. **Ameaças específicas de IA**, se houver modelo: prompt injection direta e indireta, ferramentas com permissão demais, vazamento do prompt de sistema, saída do modelo usada sem validação, consumo sem limite.
5. **Controles:** o que implementar em .NET para cada ameaça alta ou média, com o nome da API ou do recurso (policy de autorização, rate limiter, Data Protection, Key Vault, Managed Identity).
6. **Casos de teste de segurança:** as requisições maliciosas que precisam ser bloqueadas, para virar teste automatizado.

Não escreva a implementação. Se faltar informação para avaliar uma ameaça, liste a pergunta.
