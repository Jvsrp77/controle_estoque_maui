using controle_estoque_maui.Interfaces;

namespace controle_estoque_maui.Views
{
    public partial class FornecedorPage : ContentPage
    {
        private readonly IFornecedorDAO _fornecedorDAO;

        public FornecedorPage(IFornecedorDAO fornecedorDAO)
        {
            InitializeComponent();
            _fornecedorDAO = fornecedorDAO;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarFornecedoresAsync();
        }

        private async Task CarregarFornecedoresAsync()
        {
            try
            {
                listaFornecedoresCollection.ItemsSource = await _fornecedorDAO.ListarTodosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível carregar os fornecedores: {ex.Message}", "OK");
            }
        }

        private async void OnSalvarFornecedorClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomeEmpresa.Text) || string.IsNullOrWhiteSpace(txtCnpj.Text))
            {
                await DisplayAlert("Aviso", "Preencha todos os campos.", "OK");
                return;
            }

            try
            {
                await _fornecedorDAO.InserirAsync(txtNomeEmpresa.Text.Trim(), txtCnpj.Text.Trim());
                txtNomeEmpresa.Text = string.Empty;
                txtCnpj.Text = string.Empty;
                await CarregarFornecedoresAsync();
                await DisplayAlert("Sucesso", "Fornecedor cadastrado.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro ao salvar", ex.Message, "OK");
            }
        }
    }
}
