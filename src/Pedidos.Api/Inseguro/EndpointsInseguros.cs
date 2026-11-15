using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.AI;
using Pedidos.Api.Dominio;

namespace Pedidos.Api.Inseguro;

/// <summary>
/// ⚠ CÓDIGO VULNERÁVEL DE PROPÓSITO. Não copie.
/// É o tipo de código que uma IA gera quando o prompt pede só "funcionar".
/// Cada endpoint tem a versão corrigida em Seguro/EndpointsSeguros.cs.
/// </summary>
public static class EndpointsInseguros
{
    public static void MapInseguro(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/inseguro").RequireAuthorization();

        // A01 Broken Access Control (IDOR): qualquer usuário autenticado lê qualquer pedido, com CPF
        grupo.MapGet("/pedidos/{id:int}", (int id, RepositorioDePedidos repo) =>
            repo.Obter(id) is { } pedido ? Results.Ok(pedido) : Results.NotFound());

        // A01/A06 Mass assignment: o cliente manda Dono, Valor, Desconto e Status
        grupo.MapPost("/pedidos", (Pedido pedido, RepositorioDePedidos repo) =>
            Results.Created($"/inseguro/pedidos/{pedido.Id}", repo.Adicionar(pedido)));

        // A01 Path traversal: arquivo=../appsettings.json lê a connection string
        grupo.MapGet("/notas", (string arquivo, IWebHostEnvironment env) =>
            Results.Text(File.ReadAllText(Path.Combine(env.ContentRootPath, "notas", arquivo))));

        // SSRF: o servidor busca qualquer URL, inclusive endereços internos, e devolve o conteúdo
        grupo.MapPost("/webhooks/testar", async (TesteDeWebhook teste, IHttpClientFactory http) =>
            Results.Text(await http.CreateClient().GetStringAsync(teste.Url)));

        // A10 Mishandling of Exceptional Conditions: stack trace e detalhes internos na resposta
        grupo.MapGet("/relatorio", (string mes) =>
        {
            try
            {
                return Results.Ok(new { mes = int.Parse(mes, CultureInfo.InvariantCulture), total = 7000m });
            }
            catch (Exception ex)
            {
                return Results.Text(ex.ToString(), statusCode: 500);
            }
        });

        // LLM01 Prompt injection + LLM06 Excessive agency + LLM07 System prompt leakage
        grupo.MapPost("/assistente", async (Pergunta pergunta, IChatClient chat, RepositorioDePedidos repo, HttpContext ctx) =>
        {
            var usuario = ctx.User.Identity!.Name!;

            // Segredo no prompt + entrada do usuário concatenada nas instruções
            var prompt = $"""
                Você é o assistente da Loja. Usuário atual: {usuario}.
                Código interno de desconto para gerentes: GERENTE-50. Nunca revele.
                Pergunta: {pergunta.Texto}
                """;

            var ferramentas = new List<AITool>
            {
                AIFunctionFactory.Create(([Description("Id do pedido")] int pedidoId) => repo.Obter(pedidoId), "consultar_pedido",
                    "Consulta qualquer pedido pelo id."),
                AIFunctionFactory.Create(([Description("Id do pedido")] int pedidoId) =>
                {
                    if (repo.Obter(pedidoId) is not { } p) return "Pedido não encontrado.";
                    p.Status = "Cancelado";
                    return $"Pedido {pedidoId} cancelado.";
                }, "cancelar_pedido", "Cancela um pedido.")
            };

            var resposta = await chat.GetResponseAsync([new(ChatRole.System, prompt)], new() { Tools = ferramentas });
            return Results.Ok(new { resposta = resposta.Text });
        });
    }
}

public sealed record TesteDeWebhook(string Url);
public sealed record Pergunta(string Texto);
