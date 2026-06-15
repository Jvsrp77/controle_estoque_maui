using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using controle_estoque_maui.DAO;
using controle_estoque_maui.models;

namespace controle_estoque_maui.Views
{
    public partial class EstoquePage : ContentPage
    {
        private readonly ProdutoDAO _produtoDAO;
        private readonly PedidoVendaDAO _pedidoVendaDAO;

        public EstoquePage()
        {
            InitializeComponent();
            _produtoDAO = new ProdutoDAO();
            _pedidoVendaDAO = new PedidoVendaDAO();
            CarregarDadosEstoque();
        }

        private void CarregarDadosEstoque()
        {
            try
            {
                List<Produto> listaCompleta = _produtoDAO.ListarTodos();

                int totalEstoque = 0;
                int totalEsgotados = 0;

                foreach (var prod in listaCompleta)
                {
                    if (prod.QuantidadeEstoque > 0)
                        totalEstoque += prod.QuantidadeEstoque;
                    else
                        totalEsgotados++;
                }

                lblTotalProdutos.Text = totalEstoque.ToString();
                lblProdutosEsgotados.Text = totalEsgotados.ToString();
                listaProdutosCollection.ItemsSource = listaCompleta;
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        private void OnCadastrarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtPreco.Text) ||
                string.IsNullOrWhiteSpace(txtQuantidade.Text) || string.IsNullOrWhiteSpace(txtIdCategoriaProd.Text) ||
                string.IsNullOrWhiteSpace(txtIdFornecedorProd.Text))
            {
                DisplayAlert("Aviso", "Preencha todos os campos, incluindo os IDs de Categoria e Fornecedor!", "OK");
                return;
            }

            try
            {
                Produto novo = new Produto
                {
                    Nome = txtNome.Text,
                    Preco = Convert.ToDecimal(txtPreco.Text),
                    QuantidadeEstoque = Convert.ToInt32(txtQuantidade.Text),
                    IdCategoria = Convert.ToInt32(txtIdCategoriaProd.Text),
                    IdFornecedor = Convert.ToInt32(txtIdFornecedorProd.Text)
                };

                _produtoDAO.Inserir(novo);
                DisplayAlert("Sucesso", "Produto cadastrado amarrando todas as tabelas relacionais!", "OK");

                txtNome.Text = string.Empty;
                txtPreco.Text = string.Empty;
                txtQuantidade.Text = string.Empty;
                txtIdCategoriaProd.Text = string.Empty;
                txtIdFornecedorProd.Text = string.Empty;

                CarregarDadosEstoque();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro de Chave Estrangeira", "Esse ID de Categoria ou Fornecedor não existe! Cadastre-os primeiro nas abas correspondentes. Erro: " + ex.Message, "OK");
            }
        }

        private void OnRetirarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdRetirada.Text) || string.IsNullOrWhiteSpace(txtQtdRetirada.Text))
            {
                DisplayAlert("Aviso", "Preencha o ID e a Quantidade a ser retirada!", "OK");
                return;
            }

            try
            {
                int id = Convert.ToInt32(txtIdRetirada.Text);
                int qtdARetirar = Convert.ToInt32(txtQtdRetirada.Text);

                Produto produtoBanco = _produtoDAO.ObterPorId(id);

                if (produtoBanco == null)
                {
                    DisplayAlert("Erro", "Produto não encontrado com este ID!", "OK");
                    return;
                }

                if (produtoBanco.QuantidadeEstoque < qtdARetirar)
                {
                    DisplayAlert("Estoque Insuficiente", $"Você só possui {produtoBanco.QuantidadeEstoque} unidades disponíveis.", "OK");
                    return;
                }

                _pedidoVendaDAO.RegistrarVenda(produtoBanco.IdProduto, qtdARetirar, produtoBanco.Preco);
                DisplayAlert("Venda Concluída", "Retirada efetuada! Dados gravados em 'Pedidos_Vendas' e 'Itens_Pedido'.", "OK");

                txtIdRetirada.Text = string.Empty;
                txtQtdRetirada.Text = string.Empty;

                CarregarDadosEstoque();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro ao Retirar", ex.Message, "OK");
            }
        }
    }
}