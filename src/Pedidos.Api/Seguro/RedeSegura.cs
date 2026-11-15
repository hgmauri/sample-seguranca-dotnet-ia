using System.Net;
using System.Net.Sockets;

namespace Pedidos.Api.Seguro;

public static class RedeSegura
{
    /// <summary>
    /// Handler para chamadas a URLs informadas pelo usuário: sem redirect, com timeout curto e
    /// validando o IP no momento da conexão. Validar só antes (com Dns.GetHostAddresses) não basta:
    /// o DNS pode responder outro IP na segunda consulta (DNS rebinding).
    /// </summary>
    public static SocketsHttpHandler CriarHandler() => new()
    {
        AllowAutoRedirect = false,
        ConnectTimeout = TimeSpan.FromSeconds(5),
        ConnectCallback = async (contexto, ct) =>
        {
            var enderecos = await Dns.GetHostAddressesAsync(contexto.DnsEndPoint.Host, ct);
            var destino = enderecos.FirstOrDefault(ip => !EhEnderecoInterno(ip))
                ?? throw new HttpRequestException("Destino não permitido.");

            if (enderecos.Any(EhEnderecoInterno))
                throw new HttpRequestException("Destino não permitido.");

            var socket = new Socket(destino.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(destino, contexto.DnsEndPoint.Port, ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };

    public static bool EhEnderecoInterno(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip)) return true;
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal) return true;

        var b = ip.GetAddressBytes();
        return ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && (
            b[0] == 10 || b[0] == 127 || b[0] == 0 ||
            (b[0] == 172 && b[1] is >= 16 and <= 31) ||
            (b[0] == 192 && b[1] == 168) ||
            (b[0] == 169 && b[1] == 254) || // metadados de nuvem (169.254.169.254)
            (b[0] == 100 && b[1] is >= 64 and <= 127));
    }

}
