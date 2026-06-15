using System;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.DAO
{
    public class PedidoVendaDAO
    {
        private readonly string _connectionString = "Server=localhost;Port=3307;Database=controle_estoque_eletronicos;Uid=root;Pwd=;";

        public void RegistrarVenda(int idProduto, int quantidadeVendida, decimal precoUnitario)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Grava na tabela geral de faturamento/vendas
                        decimal valorTotalVenda = quantidadeVendida * precoUnitario;
                        string sqlPedido = "INSERT INTO Pedidos_Vendas (data_venda, valor_total) VALUES (CURRENT_TIMESTAMP, @total); SELECT LAST_INSERT_ID();";

                        int idPedidoGerado;
                        using (var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction))
                        {
                            cmdPedido.Parameters.AddWithValue("@total", valorTotalVenda);
                            idPedidoGerado = Convert.ToInt32(cmdPedido.ExecuteScalar());
                        }

                        // 2. Grava na tabela intermediária associando o produto à venda
                        string sqlItem = "INSERT INTO Itens_Pedido (id_pedido, id_produto, quantidade, preco_unitario) VALUES (@idPedido, @idProd, @qtd, @preco)";
                        using (var cmdItem = new MySqlCommand(sqlItem, conn, transaction))
                        {
                            cmdItem.Parameters.AddWithValue("@idPedido", idPedidoGerado);
                            cmdItem.Parameters.AddWithValue("@idProd", idProduto);
                            cmdItem.Parameters.AddWithValue("@qtd", quantidadeVendida);
                            cmdItem.Parameters.AddWithValue("@preco", precoUnitario);
                            cmdItem.ExecuteNonQuery();
                        }

                        // 3. Altera a tabela produto atualizando o saldo restante
                        string sqlBaixa = "UPDATE Produto SET quantidade_estoque = quantidade_estoque - @qtd WHERE id_produto = @idProd";
                        using (var cmdBaixa = new MySqlCommand(sqlBaixa, conn, transaction))
                        {
                            cmdBaixa.Parameters.AddWithValue("@qtd", quantidadeVendida);
                            cmdBaixa.Parameters.AddWithValue("@idProd", idProduto);
                            cmdBaixa.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}