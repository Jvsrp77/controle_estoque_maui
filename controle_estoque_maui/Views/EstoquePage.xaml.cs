using controle_estoque_maui.Interfaces;
using controle_estoque_maui.models;

namespace controle_estoque_maui.Views
{
    public partial class EstoquePage : ContentPage
    {
        private readonly IProdutoDAO _produtoDAO;
        private readonly IPedidoVendaDAO _pedidoVendaDAO;
        private readonly ICategoriaDAO _categoriaDAO;
        private readonly IFornecedorDAO _fornecedorDAO;

        /// <summary>Produto em edição; nulo quando o formulário está cadastrando.</summary>
        private Produto? _emEdicao;

        public EstoquePage(IProdutoDAO produtoDAO,
                           IPedidoVendaDAO pedidoVendaDAO,
                           ICategoriaDAO categoriaDAO,
                           IFornecedorDAO fornecedorDAO)
        {
            InitializeComponent();
            _produtoDAO = produtoDAO;
            _pedidoVendaDAO = pedidoVendaDAO;
            _categoriaDAO = categoriaDAO;
            _fornecedorDAO = fornecedorDAO;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarCombosAsync();
            await CarregarDadosEstoqueAsync();
        }

        private async Task CarregarCombosAsync()
        {
            try
            {
                pickerCategoria.ItemsSource = await _categoriaDAO.ListarTodasAsync();
                pickerFornecedor.ItemsSource = await _fornecedorDAO.ListarTodosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível carregar categorias e fornecedores: {ex.Message}", "OK");
            }
        }

        private async Task CarregarDadosEstoqueAsync()
        {
            try
            {
                var lista = await _produtoDAO.ListarTodosAsync();

                lblTotalProdutos.Text = lista.Sum(p => p.QuantidadeEstoque > 0 ? p.QuantidadeEstoque : 0).ToString();
                lblProdutosEsgotados.Text = lista.Count(p => !p.TemEstoque).ToString();
                listaProdutosCollection.ItemsSource = lista;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        private async void OnSalvarClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtPreco.Text) ||
                string.IsNullOrWhiteSpace(txtQuantidade.Text))
            {
                await DisplayAlert("Aviso", "Preencha nome, preço e quantidade.", "OK");
                return;
            }

            if (pickerCategoria.SelectedItem is not Categoria categoria ||
                pickerFornecedor.SelectedItem is not Fornecedor fornecedor)
            {
                await DisplayAlert("Aviso", "Selecione a categoria e o fornecedor.", "OK");
                return;
            }

            if (!decimal.TryParse(txtPreco.Text, out var preco) ||
                !int.TryParse(txtQuantidade.Text, out var quantidade))
            {
                await DisplayAlert("Aviso", "Preço e quantidade precisam ser números.", "OK");
                return;
            }

            var produto = new Produto
            {
                IdProduto = _emEdicao?.IdProduto ?? 0,
                Nome = txtNome.Text.Trim(),
                Preco = preco,
                QuantidadeEstoque = quantidade,
                IdCategoria = categoria.IdCategoria,
                IdFornecedor = fornecedor.IdFornecedor
            };

            try
            {
                btnSalvar.IsEnabled = false;

                if (_emEdicao is null)
                {
                    await _produtoDAO.InserirAsync(produto);
                    await DisplayAlert("Sucesso", "Produto cadastrado.", "OK");
                }
                else
                {
                    await _produtoDAO.AlterarAsync(produto);
                    await DisplayAlert("Sucesso", "Produto alterado.", "OK");
                }

                LimparFormulario();
                await CarregarDadosEstoqueAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro ao salvar", ex.Message, "OK");
            }
            finally
            {
                btnSalvar.IsEnabled = true;
            }
        }

        private void OnEditarClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: Produto produto }) return;

            _emEdicao = produto;

            txtNome.Text = produto.Nome;
            txtPreco.Text = produto.Preco.ToString("F2");
            txtQuantidade.Text = produto.QuantidadeEstoque.ToString();

            pickerCategoria.SelectedItem =
                (pickerCategoria.ItemsSource as IEnumerable<Categoria>)?
                    .FirstOrDefault(c => c.IdCategoria == produto.IdCategoria);
            pickerFornecedor.SelectedItem =
                (pickerFornecedor.ItemsSource as IEnumerable<Fornecedor>)?
                    .FirstOrDefault(f => f.IdFornecedor == produto.IdFornecedor);

            lblTituloFormulario.Text = $"Editando: {produto.Nome}";
            btnSalvar.Text = "💾 Salvar Alterações";
            btnCancelarEdicao.IsVisible = true;
        }

        private void OnCancelarEdicaoClicked(object? sender, EventArgs e) => LimparFormulario();

        private async void OnExcluirClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: Produto produto }) return;

            bool confirma = await DisplayAlert(
                "Excluir produto",
                $"Excluir {produto.Nome}? A operação não pode ser desfeita.",
                "Excluir", "Cancelar");

            if (!confirma) return;

            try
            {
                await _produtoDAO.ExcluirAsync(produto.IdProduto);
                if (_emEdicao?.IdProduto == produto.IdProduto) LimparFormulario();
                await CarregarDadosEstoqueAsync();
            }
            catch (Exception ex)
            {
                // Produto já vendido tem registro em Itens_Pedido, e a chave
                // estrangeira impede a exclusão: o banco protege o histórico.
                await DisplayAlert("Não foi possível excluir",
                    "Produto com venda registrada não pode ser removido, para não quebrar o histórico de pedidos.\n\n"
                    + ex.Message, "OK");
            }
        }

        private void LimparFormulario()
        {
            _emEdicao = null;
            txtNome.Text = txtPreco.Text = txtQuantidade.Text = string.Empty;
            pickerCategoria.SelectedItem = null;
            pickerFornecedor.SelectedItem = null;
            lblTituloFormulario.Text = "Novo Produto";
            btnSalvar.Text = "➕ Cadastrar Produto";
            btnCancelarEdicao.IsVisible = false;
        }

        private async void OnRetirarClicked(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtIdRetirada.Text, out var id) ||
                !int.TryParse(txtQtdRetirada.Text, out var quantidade) || quantidade <= 0)
            {
                await DisplayAlert("Aviso", "Informe um ID válido e uma quantidade maior que zero.", "OK");
                return;
            }

            try
            {
                btnRetirar.IsEnabled = false;

                var produto = await _produtoDAO.ObterPorIdAsync(id);
                if (produto is null)
                {
                    await DisplayAlert("Erro", "Produto não encontrado com este ID.", "OK");
                    return;
                }

                // A conferência de saldo acontece dentro da transação, no próprio
                // UPDATE. Aqui só traduzimos o desfecho para o usuário.
                var resultado = await _pedidoVendaDAO.RegistrarVendaAsync(produto.IdProduto, quantidade, produto.Preco);

                switch (resultado)
                {
                    case ResultadoVenda.Sucesso:
                        await DisplayAlert("Venda concluída",
                            "Retirada efetuada. Pedido, item e baixa de saldo gravados na mesma transação.", "OK");
                        txtIdRetirada.Text = txtQtdRetirada.Text = string.Empty;
                        await CarregarDadosEstoqueAsync();
                        break;

                    case ResultadoVenda.SaldoInsuficiente:
                        await DisplayAlert("Estoque insuficiente",
                            $"Não há {quantidade} unidades de {produto.Nome} disponíveis. Nada foi gravado.", "OK");
                        break;

                    case ResultadoVenda.ProdutoNaoEncontrado:
                        await DisplayAlert("Erro", "O produto deixou de existir durante a operação.", "OK");
                        break;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro ao retirar", ex.Message, "OK");
            }
            finally
            {
                btnRetirar.IsEnabled = true;
            }
        }
    }
}
