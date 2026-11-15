using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Pedidos.Api.Infra;

/// <summary>
/// Autenticação FALSA, só para a demo: o usuário vem do cabeçalho X-Usuario.
/// Em produção: Entra ID, JWT, cookies. O foco do exemplo é o que vem DEPOIS da autenticação.
/// </summary>
public sealed class AutenticacaoDeDemo(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Esquema = "Demo";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var usuario = Request.Headers["X-Usuario"].ToString();

        if (usuario is not ("ana" or "bruno"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var identidade = new ClaimsIdentity([new Claim(ClaimTypes.Name, usuario)], Esquema);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identidade), Esquema)));
    }
}
