using controle_estoque_maui.models;

namespace controle_estoque_maui.Interfaces
{
    public interface ICategoriaDAO
    {
        Task InserirAsync(string nome);
        Task<List<Categoria>> ListarTodasAsync();
    }
}
