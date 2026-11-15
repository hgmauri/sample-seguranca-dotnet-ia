using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.AI;
using Pedidos.Api.Dominio;
using Pedidos.Api.Infra;
using Pedidos.Api.Inseguro;
using Pedidos.Api.Seguro;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(AutenticacaoDeDemo.Esquema)
    .AddScheme<AuthenticationSchemeOptions, AutenticacaoDeDemo>(AutenticacaoDeDemo.Esquema, null);
builder.Services.AddAuthorization();

// Erros viram ProblemDetails, sem stack trace para o cliente
builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("assistente", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.User.Identity?.Name ?? "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection("Webhooks"));
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("webhooks", c => c.Timeout = TimeSpan.FromSeconds(10))
    .ConfigurePrimaryHttpMessageHandler(RedeSegura.CriarHandler);

builder.Services.AddSingleton<RepositorioDePedidos>();
builder.Services.AddSingleton<ConfirmacoesPendentes>();

var endpoint = builder.Configuration["Foundry:Endpoint"];
builder.Services
    .AddChatClient(string.IsNullOrWhiteSpace(endpoint)
        ? new ChatClientSimulado()
        : FoundryChatClientFactory.Criar(endpoint, builder.Configuration["Foundry:Deployment"]!))
    .UseFunctionInvocation(configure: f => f.MaximumIterationsPerRequest = 3);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.XContentTypeOptions = "nosniff";
    ctx.Response.Headers.XFrameOptions = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Simula o serviço de metadados da nuvem (169.254.169.254): só responde para chamadas da própria máquina
app.MapGet("/interno/metadados", (HttpContext ctx) =>
    ctx.Connection.RemoteIpAddress is { } ip && System.Net.IPAddress.IsLoopback(ip)
        ? Results.Ok(new { token = "TOKEN-FALSO-DE-IDENTIDADE-DA-NUVEM" })
        : Results.NotFound());

app.MapInseguro();
app.MapSeguro();

app.Run();
