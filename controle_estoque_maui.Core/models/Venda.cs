namespace controle_estoque_maui.models
{
    /// <summary>Uma linha do histórico: o item de um pedido, já com o nome do produto.</summary>
    public class VendaItem
    {
        public int IdPedido { get; set; }
        public DateTime DataVenda { get; set; }
        public int IdProduto { get; set; }
        public string Produto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public decimal Total => Quantidade * PrecoUnitario;

        /// <summary>Preço do momento da venda, que pode diferir do preço atual do produto.</summary>
        public string PrecoFormatado => PrecoUnitario.ToString("C2");
    }

    public class ResumoVendas
    {
        public decimal FaturamentoHoje { get; set; }
        public decimal FaturamentoMes { get; set; }
        public int UnidadesHoje { get; set; }
        public int PedidosTotal { get; set; }
    }
}
