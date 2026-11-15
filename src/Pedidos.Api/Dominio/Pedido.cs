namespace Pedidos.Api.Dominio;

public sealed class Pedido
{
    public int Id { get; set; }
    public string Dono { get; set; } = "";
    public string Produto { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
    public decimal Desconto { get; set; }
    public string Status { get; set; } = "Novo";
    public string CpfCliente { get; set; } = "";
}

/// <summary>Banco em memória, só para o exemplo.</summary>
public sealed class RepositorioDePedidos
{
    private readonly Lock _trava = new();
    private int _proximoId = 1003;

    private readonly Dictionary<int, Pedido> _pedidos = new()
    {
        [1001] = new() { Id = 1001, Dono = "ana", Produto = "Notebook", Quantidade = 1, Valor = 5200m, CpfCliente = "123.456.789-09" },
        [1002] = new() { Id = 1002, Dono = "bruno", Produto = "Monitor", Quantidade = 2, Valor = 1800m, CpfCliente = "987.654.321-00" }
    };

    public static readonly IReadOnlyDictionary<string, decimal> Catalogo = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Notebook"] = 5200m,
        ["Monitor"] = 900m,
        ["Teclado"] = 250m
    };

    public Pedido? Obter(int id) { lock (_trava) return _pedidos.GetValueOrDefault(id); }

    public IReadOnlyList<Pedido> DoUsuario(string usuario) { lock (_trava) return [.. _pedidos.Values.Where(p => p.Dono == usuario)]; }

    public Pedido Adicionar(Pedido pedido)
    {
        lock (_trava)
        {
            pedido.Id = _proximoId++;
            _pedidos[pedido.Id] = pedido;
            return pedido;
        }
    }
}
