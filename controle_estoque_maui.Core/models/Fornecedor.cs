namespace controle_estoque_maui.models
{
    public class Fornecedor
    {
        public int IdFornecedor { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;

        public override string ToString() => $"{IdFornecedor} - {NomeEmpresa}";
    }
}
