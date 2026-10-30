using controle_estoque_maui.models;

namespace controle_estoque_maui.Interfaces
{
    public interface IVendaDAO
    {
        Task<List<VendaItem>> ListarUltimasAsync(int limite = 100);
        Task<ResumoVendas> ObterResumoAsync();
    }
}
