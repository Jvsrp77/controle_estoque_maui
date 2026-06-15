using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient; 
using controle_estoque_maui.models;
using controle_estoque_maui.Interfaces;

namespace controle_estoque_maui.DAO
{
    public class ProdutoDAO : IProdutoDAO
    {
        private readonly string _connectionString = "Server=localhost;Port=3307;Database=controle_estoque_eletronicos;Uid=root;Pwd=;";

        public void
            Inserir(Produto produto)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Produto (nome, preco, quantidade_estoque, id_categoria, id_fornecedor) VALUES (@nome, @preco, @qtd, @idCat, @idFor)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", produto.Nome);
                    cmd.Parameters.AddWithValue("@preco", produto.Preco);
                    cmd.Parameters.AddWithValue("@qtd", produto.QuantidadeEstoque);
                    cmd.Parameters.AddWithValue("@idCat", produto.IdCategoria);
                    cmd.Parameters.AddWithValue("@idFor", produto.IdFornecedor);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Alterar(Produto produto)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE Produto SET nome = @nome, preco = @preco, quantidade_estoque = @qtd, id_categoria = @idCat, id_fornecedor = @idFor WHERE id_produto = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", produto.IdProduto);
                    cmd.Parameters.AddWithValue("@nome", produto.Nome);
                    cmd.Parameters.AddWithValue("@preco", produto.Preco);
                    cmd.Parameters.AddWithValue("@qtd", produto.QuantidadeEstoque);
                    cmd.Parameters.AddWithValue("@idCat", produto.IdCategoria);
                    cmd.Parameters.AddWithValue("@idFor", produto.IdFornecedor);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Excluir(int id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Produto WHERE id_produto = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Produto ObterPorId(int id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Produto WHERE id_produto = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Produto
                            {
                                IdProduto = Convert.ToInt32(reader["id_produto"]),
                                Nome = reader["nome"].ToString(),
                                Preco = Convert.ToDecimal(reader["preco"]),
                                QuantidadeEstoque = Convert.ToInt32(reader["quantidade_estoque"]),
                                IdCategoria = Convert.ToInt32(reader["id_categoria"]),
                                IdFornecedor = Convert.ToInt32(reader["id_fornecedor"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<Produto> ListarTodos()
        {
            var lista = new List<Produto>();
            using (var conn = new MySqlConnection(_connectionString))
            {

                conn.Open();
                string sql = "SELECT * FROM Produto";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Produto
                        {
                            IdProduto = Convert.ToInt32(reader["id_produto"]),
                            Nome = reader["nome"].ToString(),
                            Preco = Convert.ToDecimal(reader["preco"]),
                            QuantidadeEstoque = Convert.ToInt32(reader["quantidade_estoque"]),
                            IdCategoria = Convert.ToInt32(reader["id_categoria"]),
                            IdFornecedor = Convert.ToInt32(reader["id_fornecedor"])
                        });
                    }
                }
            }
            return lista;
        }

        public List<Produto> ListarComEstoque()
        {
            var lista = new List<Produto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Produto WHERE quantidade_estoque > 0";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Produto
                        {
                            IdProduto = Convert.ToInt32(reader["id_produto"]),
                            Nome = reader["nome"].ToString(),
                            Preco = Convert.ToDecimal(reader["preco"]),
                            QuantidadeEstoque = Convert.ToInt32(reader["quantidade_estoque"]),
                            IdCategoria = Convert.ToInt32(reader["id_categoria"]),
                            IdFornecedor = Convert.ToInt32(reader["id_fornecedor"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}