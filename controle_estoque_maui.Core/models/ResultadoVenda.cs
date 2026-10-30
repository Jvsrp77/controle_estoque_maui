namespace controle_estoque_maui.models
{
    /// <summary>
    /// Desfecho de uma tentativa de venda. Separar "não havia saldo" de
    /// "deu erro" evita que a tela precise interpretar exceção para
    /// descobrir o que aconteceu.
    /// </summary>
    public enum ResultadoVenda
    {
        Sucesso,
        SaldoInsuficiente,
        ProdutoNaoEncontrado
    }
}
