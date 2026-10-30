namespace controle_estoque_maui.models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nome { get; set; } = string.Empty;

        /// <summary>Texto exibido no Picker e na listagem.</summary>
        public override string ToString() => $"{IdCategoria} - {Nome}";
    }
}
