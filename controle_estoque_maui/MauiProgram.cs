using controle_estoque_maui.Config;
using controle_estoque_maui.DAO;
using controle_estoque_maui.Interfaces;
using controle_estoque_maui.Views;
using Microsoft.Extensions.Logging;

namespace controle_estoque_maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configuração: a string de conexão deixou de viver dentro do código.
            builder.Services.AddSingleton(AppConfig.Carregar());

            // Acesso a dados, atrás das interfaces.
            builder.Services.AddSingleton<IProdutoDAO, ProdutoDAO>();
            builder.Services.AddSingleton<IPedidoVendaDAO, PedidoVendaDAO>();
            builder.Services.AddSingleton<ICategoriaDAO, CategoriaDAO>();
            builder.Services.AddSingleton<IFornecedorDAO, FornecedorDAO>();

            // Páginas e shell, para receberem as dependências pelo construtor.
            builder.Services.AddTransient<EstoquePage>();
            builder.Services.AddTransient<FornecedorPage>();
            builder.Services.AddTransient<CategoriaPage>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
