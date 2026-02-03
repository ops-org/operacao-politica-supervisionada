using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FiltroFrequenciaCamaraDTO
    {
        //public string sorting { get; set; }
        //public int count { get; set; }
        //public int page { get; set; }

        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("order")]
        public Dictionary<string, object> Order { get; set; }

        //public string IdParlamentar { get; set; }

        //public string NomeParlamentar { get; set; }

        //public string Despesa { get; set; }

        //public string Uf { get; set; }

        //public string Partido { get; set; }

        //public string Fornecedor { get; set; }

        //public string Periodo { get; set; }

        //public string Documento { get; set; }

        //public eAgrupamentoAuditoria Agrupamento { get; set; }

        public FiltroFrequenciaCamaraDTO()
        {
            this.Start = 0;
            this.Length = 500;
            //this.Agrupamento = eAgrupamentoAuditoria.Parlamentar;
        }
    }
}
