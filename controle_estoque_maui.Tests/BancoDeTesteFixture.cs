using controle_estoque_maui.Config;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.Tests;

/// <summary>
/// Cria um banco descartável por execução e o derruba no fim.
/// Os testes nunca tocam o banco de desenvolvimento.
/// </summary>
public sealed class BancoDeTesteFixture : IAsyncLifetime
{
    private const string VariavelServidor = "ESTOQUE_TESTE_SERVIDOR";
    private static readonly string Servidor =
        Environment.GetEnvironmentVariable(VariavelServidor)
        ?? "Server=localhost;Port=3307;Uid=root;Pwd=;";

    public string NomeBanco { get; } = $"estoque_teste_{Guid.NewGuid():N}"[..28];

    public AppConfig Config => AppConfig.Para($"{Servidor}Database={NomeBanco};");

    public async Task InitializeAsync()
    {
        await using var conn = new MySqlConnection(Servidor);
        await conn.OpenAsync();

        await Executar(conn, $"CREATE DATABASE `{NomeBanco}` CHARACTER SET utf8mb4;");
        await Executar(conn, $"USE `{NomeBanco}`;");

        foreach (var ddl in Schema)
            await Executar(conn, ddl);
    }

    public async Task DisposeAsync()
    {
        await using var conn = new MySqlConnection(Servidor);
        await conn.OpenAsync();
        await Executar(conn, $"DROP DATABASE IF EXISTS `{NomeBanco}`;");
    }

    private static async Task Executar(MySqlConnection conn, string sql)
    {
        await using var cmd = new MySqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static readonly string[] Schema =
    {
        @"CREATE TABLE Categorias (
              id_categoria INT AUTO_INCREMENT PRIMARY KEY,
              nome         VARCHAR(100) NOT NULL);",

        @"CREATE TABLE Fornecedores (
              id_fornecedor INT AUTO_INCREMENT PRIMARY KEY,
              nome_empresa  VARCHAR(150) NOT NULL,
              cnpj          VARCHAR(18)  NOT NULL);",

        @"CREATE TABLE Produto (
              id_produto         INT AUTO_INCREMENT PRIMARY KEY,
              nome               VARCHAR(150)  NOT NULL,
              preco              DECIMAL(10,2) NOT NULL,
              quantidade_estoque INT           NOT NULL DEFAULT 0,
              id_categoria       INT           NOT NULL,
              id_fornecedor      INT           NOT NULL,
              FOREIGN KEY (id_categoria)  REFERENCES Categorias(id_categoria),
              FOREIGN KEY (id_fornecedor) REFERENCES Fornecedores(id_fornecedor));",

        @"CREATE TABLE Pedidos_Vendas (
              id_pedido   INT AUTO_INCREMENT PRIMARY KEY,
              data_venda  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
              valor_total DECIMAL(10,2) NOT NULL);",

        @"CREATE TABLE Itens_Pedido (
              id_item        INT AUTO_INCREMENT PRIMARY KEY,
              id_pedido      INT           NOT NULL,
              id_produto     INT           NOT NULL,
              quantidade     INT           NOT NULL,
              preco_unitario DECIMAL(10,2) NOT NULL,
              FOREIGN KEY (id_pedido)  REFERENCES Pedidos_Vendas(id_pedido),
              FOREIGN KEY (id_produto) REFERENCES Produto(id_produto));",

        "INSERT INTO Categorias (nome) VALUES ('Notebooks');",
        "INSERT INTO Fornecedores (nome_empresa, cnpj) VALUES ('Fornecedor Teste', '00.000.000/0001-00');"
    };
}

[CollectionDefinition("banco")]
public sealed class BancoCollection : ICollectionFixture<BancoDeTesteFixture> { }
