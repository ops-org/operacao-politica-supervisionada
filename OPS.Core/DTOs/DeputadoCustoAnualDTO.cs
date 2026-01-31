namespace OPS.Core.DTOs
{
    public class DeputadoCustoAnualDTO
    {
        public int ano { get; set; }
        public decimal cota_parlamentar { get; set; }
        public decimal verba_gabinete { get; set; }
        public decimal salario_patronal { get; set; }
        public decimal auxilio_moradia { get; set; }
        public decimal auxilio_saude { get; set; }
        public decimal diarias { get; set; }
    }
}