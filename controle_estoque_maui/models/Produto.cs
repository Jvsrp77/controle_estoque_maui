namespace controle_estoque_maui.models
{
    public class Produto
    {
        public int IdProduto { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int IdCategoria { get; set; }
        public int IdFornecedor { get; set; }

        // Propriedade para validar de forma simples se há estoque restante
        public bool TemEstoque => QuantidadeEstoque > 0;
    }
}
