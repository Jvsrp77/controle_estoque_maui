using controle_estoque_maui.Config;
using controle_estoque_maui.Interfaces;
using controle_estoque_maui.models;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.DAO
{
    public class FornecedorDAO : IFornecedorDAO
    {
        private readonly string _connectionString;

        public FornecedorDAO(AppConfig config) => _connectionString = config.ConnectionString;

        public async Task InserirAsync(string nomeEmpresa, string cnpj)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(
                "INSERT INTO Fornecedores (nome_empresa, cnpj) VALUES (@nome, @cnpj)", conn);
            cmd.Parameters.AddWithValue("@nome", nomeEmpresa);
            cmd.Parameters.AddWithValue("@cnpj", cnpj);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Fornecedor>> ListarTodosAsync()
        {
            var lista = new List<Fornecedor>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(
                "SELECT id_fornecedor, nome_empresa, cnpj FROM Fornecedores ORDER BY id_fornecedor", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                lista.Add(new Fornecedor
                {
                    IdFornecedor = reader.GetInt32(0),
                    NomeEmpresa = reader.GetString(1),
                    Cnpj = reader.GetString(2)
                });

            return lista;
        }
    }
}
