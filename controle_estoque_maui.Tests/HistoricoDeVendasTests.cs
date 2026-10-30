using controle_estoque_maui.DAO;
using controle_estoque_maui.models;

namespace controle_estoque_maui.Tests;

[Collection("banco")]
public class HistoricoDeVendasTests
{
    private readonly ProdutoDAO _produtos;
    private readonly PedidoVendaDAO _vendas;
    private readonly VendaDAO _historico;

    public HistoricoDeVendasTests(BancoDeTesteFixture banco)
    {
        _produtos = new ProdutoDAO(banco.Config);
        _vendas = new PedidoVendaDAO(banco.Config);
        _historico = new VendaDAO(banco.Config);
    }

    [Fact]
    public async Task Venda_registrada_aparece_no_historico_com_o_preco_do_momento()
    {
        var nome = $"Monitor {Guid.NewGuid():N}"[..18];
        var id = await CriarProduto(nome, estoque: 10, preco: 900m);

        await _vendas.RegistrarVendaAsync(id, 2, 900m);

        // O preço do catálogo muda depois da venda...
        var produto = (await _produtos.ObterPorIdAsync(id))!;
        produto.Preco = 1500m;
        await _produtos.AlterarAsync(produto);

        var linhas = await _historico.ListarUltimasAsync();
        var linha = Assert.Single(linhas, l => l.IdProduto == id);

        Assert.Equal(nome, linha.Produto);
        Assert.Equal(2, linha.Quantidade);

        // ...mas o histórico preserva o preço praticado na venda.
        Assert.Equal(900m, linha.PrecoUnitario);
        Assert.Equal(1800m, linha.Total);
    }

    [Fact]
    public async Task Venda_recusada_nao_entra_no_historico()
    {
        var id = await CriarProduto($"SSD {Guid.NewGuid():N}"[..14], estoque: 1, preco: 500m);

        var resultado = await _vendas.RegistrarVendaAsync(id, 9, 500m);

        Assert.Equal(ResultadoVenda.SaldoInsuficiente, resultado);
        Assert.DoesNotContain(await _historico.ListarUltimasAsync(), l => l.IdProduto == id);
    }

    [Fact]
    public async Task Resumo_soma_o_faturamento_das_vendas_do_dia()
    {
        var antes = await _historico.ObterResumoAsync();

        var id = await CriarProduto($"Cabo {Guid.NewGuid():N}"[..15], estoque: 10, preco: 50m);
        await _vendas.RegistrarVendaAsync(id, 3, 50m);

        var depois = await _historico.ObterResumoAsync();

        Assert.Equal(antes.FaturamentoHoje + 150m, depois.FaturamentoHoje);
        Assert.Equal(antes.UnidadesHoje + 3, depois.UnidadesHoje);
        Assert.Equal(antes.PedidosTotal + 1, depois.PedidosTotal);
    }

    private async Task<int> CriarProduto(string nome, int estoque, decimal preco)
    {
        await _produtos.InserirAsync(new Produto
        {
            Nome = nome,
            Preco = preco,
            QuantidadeEstoque = estoque,
            IdCategoria = 1,
            IdFornecedor = 1
        });

        return (await _produtos.ListarTodosAsync()).Max(p => p.IdProduto);
    }
}
