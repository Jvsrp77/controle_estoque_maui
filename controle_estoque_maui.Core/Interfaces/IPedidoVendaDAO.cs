using controle_estoque_maui.models;

namespace controle_estoque_maui.Interfaces
{
    public interface IPedidoVendaDAO
    {
        /// <summary>
        /// Registra a venda e baixa o estoque numa única transação.
        /// Devolve o desfecho em vez de lançar exceção para saldo insuficiente.
        /// </summary>
        Task<ResultadoVenda> RegistrarVendaAsync(int idProduto, int quantidadeVendida, decimal precoUnitario);
    }
}
