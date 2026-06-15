using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;

namespace controle_estoque_maui.Views
{
    public partial class CategoriaPage : ContentPage
    {
        private readonly string _connectionString = "Server=localhost;Port=3307;Database=controle_estoque_eletronicos;Uid=root;Pwd=;";

        public CategoriaPage()
        {
            InitializeComponent();
            CarregarCategorias();
        }

        private void CarregarCategorias()
        {
            var lista = new List<dynamic>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id_categoria, nome FROM Categorias", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new
                        {
                            IdCategoria = reader["id_categoria"].ToString(),
                            Nome = reader["nome"].ToString()
                        });
                    }
                }
            }
            listaCategoriasCollection.ItemsSource = lista;
        }

        private void OnSalvarCategoriaClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomeCategoria.Text))
            {
                DisplayAlert("Aviso", "Preencha o nome da categoria!", "OK");
                return;
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO Categorias (nome) VALUES (@nome)", conn))
                {
                    cmd.Parameters.AddWithValue("@nome", txtNomeCategoria.Text);
                    cmd.ExecuteNonQuery();
                }
            }

            DisplayAlert("Sucesso", "Categoria adicionada!", "OK");
            txtNomeCategoria.Text = string.Empty;
            CarregarCategorias();
        }
    }
}