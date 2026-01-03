using System;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Rondonia.Entities
{
    public class GrupoDespesaRO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("criado_por__first_name")]
        public string CriadoPorFirstName { get; set; }

        [JsonPropertyName("criado_por__last_name")]
        public string CriadoPorLastName { get; set; }

        [JsonPropertyName("verba__gabinete__nome_de_urna")]
        public string VerbaGabineteNomeDeUrna { get; set; }

        [JsonPropertyName("verba__mes")]
        public int VerbaMes { get; set; }

        [JsonPropertyName("verba__ano")]
        public string VerbaAno { get; set; }

        [JsonPropertyName("verba__id")]
        public int VerbaId { get; set; }

        [JsonPropertyName("categoria_verba__nome")]
        public string CategoriaVerbaNome { get; set; }

        [JsonPropertyName("tipo_verba__descricao")]
        public string TipoVerbaDescricao { get; set; }

        [JsonPropertyName("total_despesas")]
        public string TotalDespesas { get; set; }

        [JsonPropertyName("total_recomendado")]
        public string TotalRecomendado { get; set; }

        [JsonPropertyName("total_pago")]
        public string TotalPago { get; set; }
    }
}
