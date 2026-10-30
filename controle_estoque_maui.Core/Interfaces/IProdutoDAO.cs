using controle_estoque_maui.models;

namespace controle_estoque_maui.Interfaces
{
    public interface IProdutoDAO
    {
        Task InserirAsync(Produto produto);
        Task AlterarAsync(Produto produto);
        Task ExcluirAsync(int id);
        Task<Produto?> ObterPorIdAsync(int id);
        Task<List<Produto>> ListarTodosAsync();
        Task<List<Produto>> ListarComEstoqueAsync();
    }
}
