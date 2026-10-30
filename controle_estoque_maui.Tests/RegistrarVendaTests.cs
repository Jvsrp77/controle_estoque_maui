using controle_estoque_maui.DAO;
using controle_estoque_maui.models;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.Tests;

[Collection("banco")]
public class RegistrarVendaTests
{
    private readonly BancoDeTesteFixture _banco;
    private readonly ProdutoDAO _produtos;
    private readonly PedidoVendaDAO _vendas;

    public RegistrarVendaTests(BancoDeTesteFixture banco)
    {
        _banco = banco;
        _produtos = new ProdutoDAO(banco.Config);
        _vendas = new PedidoVendaDAO(banco.Config);
    }

    [Fact]
    public async Task Venda_com_saldo_baixa_o_estoque_e_grava_pedido_e_item()
    {
        var id = await CriarProduto(estoque: 10, preco: 100m);

        var resultado = await _vendas.RegistrarVendaAsync(id, 3, 100m);

        Assert.Equal(ResultadoVenda.Sucesso, resultado);

        var produto = await _produtos.ObterPorIdAsync(id);
        Assert.Equal(7, produto!.QuantidadeEstoque);

        // Os testes compartilham o banco, então conte só o que é deste produto.
        Assert.Equal(1, await Contar("Itens_Pedido", $"id_produto = {id}"));
        Assert.Equal(1, await Contar(
            "Pedidos_Vendas p JOIN Itens_Pedido i ON i.id_pedido = p.id_pedido",
            $"i.id_produto = {id}"));
    }

    [Fact]
    public async Task Venda_sem_saldo_nao_grava_nada()
    {
        var id = await CriarProduto(estoque: 2, preco: 50m);
        var pedidosAntes = await Contar("Pedidos_Vendas");

        var resultado = await _vendas.RegistrarVendaAsync(id, 5, 50m);

        Assert.Equal(ResultadoVenda.SaldoInsuficiente, resultado);

        // O estoque não se mexeu...
        var produto = await _produtos.ObterPorIdAsync(id);
        Assert.Equal(2, produto!.QuantidadeEstoque);

        // ...e o rollback não deixou pedido órfão para trás.
        Assert.Equal(pedidosAntes, await Contar("Pedidos_Vendas"));
        Assert.Equal(0, await Contar("Itens_Pedido", $"id_produto = {id}"));
    }

    [Fact]
    public async Task Venda_de_produto_inexistente_e_recusada()
    {
        var resultado = await _vendas.RegistrarVendaAsync(999_999, 1, 10m);
        Assert.Equal(ResultadoVenda.ProdutoNaoEncontrado, resultado);
    }

    [Fact]
    public async Task Venda_exata_do_saldo_restante_e_aceita()
    {
        var id = await CriarProduto(estoque: 4, preco: 25m);

        Assert.Equal(ResultadoVenda.Sucesso, await _vendas.RegistrarVendaAsync(id, 4, 25m));
        Assert.Equal(0, (await _produtos.ObterPorIdAsync(id))!.QuantidadeEstoque);

        // E a próxima já não passa.
        Assert.Equal(ResultadoVenda.SaldoInsuficiente, await _vendas.RegistrarVendaAsync(id, 1, 25m));
    }

    [Fact]
    public async Task Vendas_simultaneas_nao_levam_o_saldo_a_negativo()
    {
        // Este é o teste que justifica ter a conferência dentro do UPDATE:
        // dez vendas de 1 unidade disputando um estoque de 6.
        var id = await CriarProduto(estoque: 6, preco: 10m);

        var tentativas = Enumerable.Range(0, 10)
            .Select(_ => _vendas.RegistrarVendaAsync(id, 1, 10m));

        var resultados = await Task.WhenAll(tentativas);

        Assert.Equal(6, resultados.Count(r => r == ResultadoVenda.Sucesso));
        Assert.Equal(4, resultados.Count(r => r == ResultadoVenda.SaldoInsuficiente));

        var produto = await _produtos.ObterPorIdAsync(id);
        Assert.Equal(0, produto!.QuantidadeEstoque);
    }

    // ------------------------------------------------------------- apoio

    private async Task<int> CriarProduto(int estoque, decimal preco)
    {
        await _produtos.InserirAsync(new Produto
        {
            Nome = $"Produto {Guid.NewGuid():N}"[..20],
            Preco = preco,
            QuantidadeEstoque = estoque,
            IdCategoria = 1,
            IdFornecedor = 1
        });

        var todos = await _produtos.ListarTodosAsync();
        return todos.Max(p => p.IdProduto);
    }

    private async Task<int> Contar(string tabela, string? filtro = null)
    {
        var sql = $"SELECT COUNT(*) FROM {tabela}"
                  + (filtro is null ? "" : $" WHERE {filtro}");

        await using var conn = new MySqlConnection(_banco.Config.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
