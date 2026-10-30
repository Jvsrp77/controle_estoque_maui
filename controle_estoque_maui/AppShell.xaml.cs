using controle_estoque_maui.Views;

namespace controle_estoque_maui
{
    public partial class AppShell : Shell
    {
        // As páginas vêm do contêiner, e não de "new" espalhado pelo XAML:
        // é o que permite injetar os DAOs pelo construtor de cada uma.
        public AppShell(EstoquePage estoque, FornecedorPage fornecedores, CategoriaPage categorias)
        {
            InitializeComponent();

            abaEstoque.Content = estoque;
            abaFornecedores.Content = fornecedores;
            abaCategorias.Content = categorias;
        }
    }
}
