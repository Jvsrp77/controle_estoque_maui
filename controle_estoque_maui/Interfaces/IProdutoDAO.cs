using System.Collections.Generic;
using controle_estoque_maui.models;

namespace controle_estoque_maui.Interfaces
{
    public interface IProdutoDAO
    {
        void Inserir(Produto produto);
        void Alterar(Produto produto);
        void Excluir(int id);
        Produto ObterPorId(int id);
        List<Produto> ListarTodos();
        List<Produto> ListarComEstoque();
    }
}

