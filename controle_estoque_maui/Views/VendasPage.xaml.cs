using controle_estoque_maui.Interfaces;

namespace controle_estoque_maui.Views
{
    public partial class VendasPage : ContentPage
    {
        private readonly IVendaDAO _vendaDAO;

        public VendasPage(IVendaDAO vendaDAO)
        {
            InitializeComponent();
            _vendaDAO = vendaDAO;
        }

        // Recarrega a cada vez que a aba aparece: a venda acontece na aba
        // Estoque, e o histórico precisa refletir isso sem o usuário pedir.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarAsync();
        }

        private async void OnAtualizarClicked(object? sender, EventArgs e) => await CarregarAsync();

        private async Task CarregarAsync()
        {
            try
            {
                var resumo = await _vendaDAO.ObterResumoAsync();

                lblFaturamentoHoje.Text = resumo.FaturamentoHoje.ToString("C2");
                lblFaturamentoMes.Text = resumo.FaturamentoMes.ToString("C2");
                lblUnidadesHoje.Text = $"{resumo.UnidadesHoje} unidades";
                lblPedidosTotal.Text = resumo.PedidosTotal.ToString();

                var vendas = await _vendaDAO.ListarUltimasAsync();
                listaVendas.ItemsSource = vendas;
                lblVazio.IsVisible = vendas.Count == 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível carregar o histórico: {ex.Message}", "OK");
            }
        }
    }
}
