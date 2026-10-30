using controle_estoque_maui.Config;
using controle_estoque_maui.Interfaces;
using controle_estoque_maui.models;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.DAO
{
    public class PedidoVendaDAO : IPedidoVendaDAO
    {
        private readonly string _connectionString;

        public PedidoVendaDAO(AppConfig config) => _connectionString = config.ConnectionString;

        /// <summary>
        /// Uma venda são três escritas. Ou as três acontecem, ou nenhuma.
        ///
        /// A conferência de saldo faz parte do próprio UPDATE
        /// (WHERE quantidade_estoque >= @qtd). Conferir antes, em outra consulta,
        /// deixaria uma janela entre a leitura e a escrita em que outra venda
        /// poderia consumir o mesmo saldo.
        ///
        /// A ordem das três escritas importa: a baixa vem primeiro porque é a
        /// única que pode ser recusada e a única disputada por vendas
        /// simultâneas.
        /// </summary>
        public async Task<ResultadoVenda> RegistrarVendaAsync(int idProduto, int quantidadeVendida, decimal precoUnitario)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // 1. Baixa do saldo, condicionada a haver saldo.
                //
                //    Vem primeiro de propósito: é a única etapa que pode ser
                //    recusada, e é a que disputa a linha do produto com outras
                //    vendas. Fazendo-a antes, uma venda recusada não chega a
                //    escrever nada, e vendas simultâneas pegam a trava da linha
                //    sempre na mesma ordem — o que evita deadlock.
                //
                //    A subtração é feita pelo banco: ler para a memória,
                //    subtrair em C# e regravar faria duas vendas simultâneas
                //    se sobrescreverem.
                const string sqlBaixa = @"UPDATE Produto
                                             SET quantidade_estoque = quantidade_estoque - @qtd
                                           WHERE id_produto = @idProd
                                             AND quantidade_estoque >= @qtd";

                int linhasAfetadas;
                await using (var cmd = new MySqlCommand(sqlBaixa, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@qtd", quantidadeVendida);
                    cmd.Parameters.AddWithValue("@idProd", idProduto);
                    linhasAfetadas = await cmd.ExecuteNonQueryAsync();
                }

                // Zero linhas: ou o produto não existe, ou não havia saldo.
                if (linhasAfetadas == 0)
                {
                    await transaction.RollbackAsync();
                    return await ProdutoExisteAsync(conn, idProduto)
                        ? ResultadoVenda.SaldoInsuficiente
                        : ResultadoVenda.ProdutoNaoEncontrado;
                }

                // 2. Registro de faturamento
                decimal valorTotal = quantidadeVendida * precoUnitario;
                const string sqlPedido = @"INSERT INTO Pedidos_Vendas (data_venda, valor_total)
                                           VALUES (CURRENT_TIMESTAMP, @total);
                                           SELECT LAST_INSERT_ID();";

                int idPedido;
                await using (var cmd = new MySqlCommand(sqlPedido, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@total", valorTotal);
                    idPedido = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // 3. Item vendido, com o preço praticado no momento da venda
                const string sqlItem = @"INSERT INTO Itens_Pedido (id_pedido, id_produto, quantidade, preco_unitario)
                                         VALUES (@idPedido, @idProd, @qtd, @preco)";

                await using (var cmd = new MySqlCommand(sqlItem, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@idPedido", idPedido);
                    cmd.Parameters.AddWithValue("@idProd", idProduto);
                    cmd.Parameters.AddWithValue("@qtd", quantidadeVendida);
                    cmd.Parameters.AddWithValue("@preco", precoUnitario);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return ResultadoVenda.Sucesso;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task<bool> ProdutoExisteAsync(MySqlConnection conn, int idProduto)
        {
            await using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Produto WHERE id_produto = @id", conn);
            cmd.Parameters.AddWithValue("@id", idProduto);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }
    }
}
