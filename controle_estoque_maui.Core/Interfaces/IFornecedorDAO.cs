using controle_estoque_maui.models;

namespace controle_estoque_maui.Interfaces
{
    public interface IFornecedorDAO
    {
        Task InserirAsync(string nomeEmpresa, string cnpj);
        Task<List<Fornecedor>> ListarTodosAsync();
    }
}
