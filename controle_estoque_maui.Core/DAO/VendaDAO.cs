using controle_estoque_maui.Config;
using controle_estoque_maui.Interfaces;
using controle_estoque_maui.models;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.DAO
{
    /// <summary>
    /// Leitura do que a transação de venda gravou. É o lado de consulta das
    /// tabelas Pedidos_Vendas e Itens_Pedido.
    /// </summary>
    public class VendaDAO : IVendaDAO
    {
        private readonly string _connectionString;

        public VendaDAO(AppConfig config) => _connectionString = config.ConnectionString;

        public async Task<List<VendaItem>> ListarUltimasAsync(int limite = 100)
        {
            // O nome do produto vem do JOIN, mas o preço vem de Itens_Pedido:
            // é o preço praticado na venda, não o preço atual do catálogo.
            const string sql = @"SELECT p.id_pedido,
                                        p.data_venda,
                                        i.id_produto,
                                        pr.nome,
                                        i.quantidade,
                                        i.preco_unitario
                                   FROM Pedidos_Vendas p
                                   JOIN Itens_Pedido   i  ON i.id_pedido  = p.id_pedido
                                   JOIN Produto        pr ON pr.id_produto = i.id_produto
                                  ORDER BY p.data_venda DESC, p.id_pedido DESC
                                  LIMIT @limite";

            var lista = new List<VendaItem>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@limite", limite);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new VendaItem
                {
                    IdPedido = reader.GetInt32(0),
                    DataVenda = reader.GetDateTime(1),
                    IdProduto = reader.GetInt32(2),
                    Produto = reader.GetString(3),
                    Quantidade = reader.GetInt32(4),
                    PrecoUnitario = reader.GetDecimal(5)
                });
            }

            return lista;
        }

        public async Task<ResumoVendas> ObterResumoAsync()
        {
            const string sql = @"SELECT
                   COALESCE(SUM(CASE WHEN DATE(data_venda) = CURDATE()
                                     THEN valor_total ELSE 0 END), 0)                   AS hoje,
                   COALESCE(SUM(CASE WHEN YEAR(data_venda)  = YEAR(CURDATE())
                                      AND MONTH(data_venda) = MONTH(CURDATE())
                                     THEN valor_total ELSE 0 END), 0)                   AS mes,
                   COUNT(*)                                                             AS pedidos,
                   COALESCE((SELECT SUM(i.quantidade)
                               FROM Itens_Pedido i
                               JOIN Pedidos_Vendas p2 ON p2.id_pedido = i.id_pedido
                              WHERE DATE(p2.data_venda) = CURDATE()), 0)                AS unidades_hoje
              FROM Pedidos_Vendas";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return new ResumoVendas();

            return new ResumoVendas
            {
                FaturamentoHoje = reader.GetDecimal(0),
                FaturamentoMes = reader.GetDecimal(1),
                PedidosTotal = reader.GetInt32(2),
                UnidadesHoje = Convert.ToInt32(reader.GetValue(3))
            };
        }
    }
}
