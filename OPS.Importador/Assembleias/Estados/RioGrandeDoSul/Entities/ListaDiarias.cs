using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class ListaDiarias
    {
        [JsonPropertyName("dataDiaria")]
        public string DataDiaria { get; set; }

        [JsonPropertyName("mes")]
        public int Mes { get; set; }

        [JsonPropertyName("idBeneficiario")]
        public int IdBeneficiario { get; set; }

        [JsonPropertyName("idMovimento")]
        public int IdMovimento { get; set; }

        [JsonPropertyName("idRequisicaoDiaria")]
        public int IdRequisicaoDiaria { get; set; }

        [JsonPropertyName("idTipoDiaria")]
        public int IdTipoDiaria { get; set; }

        public TipoDiariaRS TipoDiaria
        {
            get
            {
                return (TipoDiariaRS)IdTipoDiaria;
            }
        }

        [JsonPropertyName("destino")]
        public string Destino { get; set; }

        [JsonPropertyName("ida")]
        public string Ida { get; set; }

        [JsonPropertyName("volta")]
        public string Volta { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }

        [JsonPropertyName("diarias")]
        public string Diarias { get; set; }

        [JsonPropertyName("totalPago")]
        public string TotalPago { get; set; }
    }
}
