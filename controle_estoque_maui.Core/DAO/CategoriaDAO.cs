using controle_estoque_maui.Config;
using controle_estoque_maui.Interfaces;
using controle_estoque_maui.models;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.DAO
{
    public class CategoriaDAO : ICategoriaDAO
    {
        private readonly string _connectionString;

        public CategoriaDAO(AppConfig config) => _connectionString = config.ConnectionString;

        public async Task InserirAsync(string nome)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand("INSERT INTO Categorias (nome) VALUES (@nome)", conn);
            cmd.Parameters.AddWithValue("@nome", nome);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Categoria>> ListarTodasAsync()
        {
            var lista = new List<Categoria>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(
                "SELECT id_categoria, nome FROM Categorias ORDER BY id_categoria", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                lista.Add(new Categoria { IdCategoria = reader.GetInt32(0), Nome = reader.GetString(1) });

            return lista;
        }
    }
}
