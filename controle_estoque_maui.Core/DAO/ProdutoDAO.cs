using controle_estoque_maui.Config;
using controle_estoque_maui.Interfaces;
using controle_estoque_maui.models;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.DAO
{
    public class ProdutoDAO : IProdutoDAO
    {
        private const string Colunas =
            "id_produto, nome, preco, quantidade_estoque, id_categoria, id_fornecedor";

        private readonly string _connectionString;

        public ProdutoDAO(AppConfig config) => _connectionString = config.ConnectionString;

        public async Task InserirAsync(Produto produto)
        {
            const string sql = @"INSERT INTO Produto (nome, preco, quantidade_estoque, id_categoria, id_fornecedor)
                                 VALUES (@nome, @preco, @qtd, @idCat, @idFor)";

            await using var conn = await AbrirAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            Preencher(cmd, produto);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AlterarAsync(Produto produto)
        {
            const string sql = @"UPDATE Produto
                                    SET nome = @nome, preco = @preco, quantidade_estoque = @qtd,
                                        id_categoria = @idCat, id_fornecedor = @idFor
                                  WHERE id_produto = @id";

            await using var conn = await AbrirAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", produto.IdProduto);
            Preencher(cmd, produto);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            await using var conn = await AbrirAsync();
            await using var cmd = new MySqlCommand("DELETE FROM Produto WHERE id_produto = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Produto?> ObterPorIdAsync(int id)
        {
            await using var conn = await AbrirAsync();
            await using var cmd = new MySqlCommand(
                $"SELECT {Colunas} FROM Produto WHERE id_produto = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Ler(reader) : null;
        }

        public Task<List<Produto>> ListarTodosAsync() =>
            ListarAsync($"SELECT {Colunas} FROM Produto ORDER BY id_produto");

        public Task<List<Produto>> ListarComEstoqueAsync() =>
            ListarAsync($"SELECT {Colunas} FROM Produto WHERE quantidade_estoque > 0 ORDER BY id_produto");

        private async Task<List<Produto>> ListarAsync(string sql)
        {
            var lista = new List<Produto>();

            await using var conn = await AbrirAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                lista.Add(Ler(reader));

            return lista;
        }

        private async Task<MySqlConnection> AbrirAsync()
        {
            var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }

        private static void Preencher(MySqlCommand cmd, Produto p)
        {
            cmd.Parameters.AddWithValue("@nome", p.Nome);
            cmd.Parameters.AddWithValue("@preco", p.Preco);
            cmd.Parameters.AddWithValue("@qtd", p.QuantidadeEstoque);
            cmd.Parameters.AddWithValue("@idCat", p.IdCategoria);
            cmd.Parameters.AddWithValue("@idFor", p.IdFornecedor);
        }

        private static Produto Ler(System.Data.Common.DbDataReader r) => new()
        {
            IdProduto = r.GetInt32(0),
            Nome = r.GetString(1),
            Preco = r.GetDecimal(2),
            QuantidadeEstoque = r.GetInt32(3),
            IdCategoria = r.GetInt32(4),
            IdFornecedor = r.GetInt32(5)
        };
    }
}
