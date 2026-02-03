using OPS.Core.Enumerators;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FiltroParlamentarDTO
    {
        //public string filter { get; set; }
        [JsonPropertyName("sorting")]
        public string Sorting { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("id_parlamentar")]
        public string IdParlamentar { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("despesa")]
        public string Despesa { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("fornecedor")]
        public string Fornecedor { get; set; }

        [JsonPropertyName("periodo")]
        public int Periodo { get; set; }

        [JsonPropertyName("documento")]
        public string Documento { get; set; }

        //public string PeriodoCustom { get; set; }

        [JsonPropertyName("agrupamento")]
        public EnumAgrupamentoAuditoria Agrupamento { get; set; }

        public FiltroParlamentarDTO()
        {
            this.Count = 100;
            this.Page = 1;
            this.Agrupamento = EnumAgrupamentoAuditoria.Parlamentar;
        }
    }
}
