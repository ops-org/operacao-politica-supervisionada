namespace OPS.Core.DTOs
{
    public class ParlamentarCustoAnualDTO
    {
        public int Ano { get; set; }

        public decimal CotaParlamentar { get; set; }
        public decimal VerbaGabinete { get; set; }
        public decimal SalarioPatronal { get; set; }
        public decimal AuxilioMoradia { get; set; }
        public decimal AuxilioSaude { get; set; }
        public decimal Diarias { get; set; }
    }
}