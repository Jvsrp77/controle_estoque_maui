using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.Views
{
    public partial class FornecedorPage : ContentPage
    {
        private readonly string _connectionString = "Server=localhost;Port=3307;Database=controle_estoque_eletronicos;Uid=root;Pwd=;";

        public FornecedorPage()
        {
            InitializeComponent();
            CarregarFornecedores();
        }

        private void CarregarFornecedores()
        {
            var lista = new List<dynamic>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id_fornecedor, nome_empresa, cnpj FROM Fornecedores", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new
                        {
                            IdFornecedor = reader["id_fornecedor"].ToString(),
                            NomeEmpresa = reader["nome_empresa"].ToString(),
                            Cnpj = reader["cnpj"].ToString()
                        });
                    }
                }
            }
            listaFornecedoresCollection.ItemsSource = lista;
        }

        private void OnSalvarFornecedorClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomeEmpresa.Text) || string.IsNullOrWhiteSpace(txtCnpj.Text))
            {
                DisplayAlert("Aviso", "Preencha todos os campos!", "OK");
                return;
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO Fornecedores (nome_empresa, cnpj) VALUES (@nome, @cnpj)", conn))
                {
                    cmd.Parameters.AddWithValue("@nome", txtNomeEmpresa.Text);
                    cmd.Parameters.AddWithValue("@cnpj", txtCnpj.Text);
                    cmd.ExecuteNonQuery();
                }
            }

            DisplayAlert("Sucesso", "Fornecedor cadastrado!", "OK");
            txtNomeEmpresa.Text = string.Empty;
            txtCnpj.Text = string.Empty;
            CarregarFornecedores();
        }
    }
}