using System.Reflection;
using System.Text.Json;

namespace controle_estoque_maui.Config
{
    /// <summary>
    /// Configuração da aplicação, lida de appsettings.json embutido no assembly.
    /// A variável de ambiente ESTOQUE_CONNECTIONSTRING tem precedência, para
    /// permitir apontar para outro banco sem recompilar.
    /// </summary>
    public sealed class AppConfig
    {
        public const string VariavelDeAmbiente = "ESTOQUE_CONNECTIONSTRING";

        public string ConnectionString { get; }

        private AppConfig(string connectionString) => ConnectionString = connectionString;

        public static AppConfig Carregar()
        {
            var doAmbiente = Environment.GetEnvironmentVariable(VariavelDeAmbiente);
            if (!string.IsNullOrWhiteSpace(doAmbiente))
                return new AppConfig(doAmbiente);

            var assembly = Assembly.GetExecutingAssembly();
            var recurso = assembly.GetManifestResourceNames()
                                  .FirstOrDefault(n => n.EndsWith("appsettings.json", StringComparison.Ordinal))
                          ?? throw new InvalidOperationException(
                              "appsettings.json não foi embutido no assembly.");

            using var stream = assembly.GetManifestResourceStream(recurso)!;
            using var doc = JsonDocument.Parse(stream);

            var valor = doc.RootElement.GetProperty("Banco").GetProperty("ConnectionString").GetString();

            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidOperationException("Banco:ConnectionString está vazio em appsettings.json.");

            return new AppConfig(valor);
        }

        /// <summary>Usado pelos testes, que apontam para um banco descartável.</summary>
        public static AppConfig Para(string connectionString) => new(connectionString);
    }
}
