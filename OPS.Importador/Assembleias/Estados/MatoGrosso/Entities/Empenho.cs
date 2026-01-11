namespace OPS.Importador.Assembleias.Estados.MatoGrosso.Entities
{
    public class Empenho
    {
        public int Entidade { get; set; }
        public int EmpenhoNum { get; set; }
        public int Exercicio { get; set; }
        public DateTime Data { get; set; }
        public int Fornecedor { get; set; }
        public string Nome { get; set; }
        public string Historico { get; set; }
        public decimal ValorEmpenhado { get; set; }
        public decimal ValorAnulado { get; set; }
        public decimal ValorLiquidado { get; set; }
        public decimal ValorRetido { get; set; }
        public decimal ValorPago { get; set; }
        public decimal ValorAPagar { get; set; }
    }
}
