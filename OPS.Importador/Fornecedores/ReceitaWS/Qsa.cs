using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.ReceitaWS
{
    #region ReceitaWS DTO

    public class Qsa
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("qual")]
        public string Qual { get; set; }

        [JsonPropertyName("pais_origem")]
        public string PaisOrigem { get; set; }

        [JsonPropertyName("nome_rep_legal")]
        public string NomeRepLegal { get; set; }

        [JsonPropertyName("qual_rep_legal")]
        public string QualRepLegal { get; set; }
    }


    #endregion ReceitaWS DTO
}