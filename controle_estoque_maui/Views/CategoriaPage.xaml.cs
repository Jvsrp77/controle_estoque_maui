using controle_estoque_maui.Interfaces;

namespace controle_estoque_maui.Views
{
    public partial class CategoriaPage : ContentPage
    {
        private readonly ICategoriaDAO _categoriaDAO;

        public CategoriaPage(ICategoriaDAO categoriaDAO)
        {
            InitializeComponent();
            _categoriaDAO = categoriaDAO;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarCategoriasAsync();
        }

        private async Task CarregarCategoriasAsync()
        {
            try
            {
                listaCategoriasCollection.ItemsSource = await _categoriaDAO.ListarTodasAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível carregar as categorias: {ex.Message}", "OK");
            }
        }

        private async void OnSalvarCategoriaClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomeCategoria.Text))
            {
                await DisplayAlert("Aviso", "Preencha o nome da categoria.", "OK");
                return;
            }

            try
            {
                await _categoriaDAO.InserirAsync(txtNomeCategoria.Text.Trim());
                txtNomeCategoria.Text = string.Empty;
                await CarregarCategoriasAsync();
                await DisplayAlert("Sucesso", "Categoria adicionada.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro ao salvar", ex.Message, "OK");
            }
        }
    }
}
