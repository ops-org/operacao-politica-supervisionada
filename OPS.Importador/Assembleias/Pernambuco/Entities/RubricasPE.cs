using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Pernambuco.Entities
{
    public class RubricasPE
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nome_categoria")]
        public string NomeCategoria { get; set; }

        [JsonPropertyName("data_criacao")]
        public string DataCriacao { get; set; }

        [JsonPropertyName("id_usuario")]
        public string IdUsuario { get; set; }

        [JsonPropertyName("ativo")]
        public string Ativo { get; set; }

        [JsonPropertyName("numero_categoria")]
        public string NumeroCategoria { get; set; }

        [JsonPropertyName("numero_romano")]
        public string NumeroRomano { get; set; }

        [JsonPropertyName("valor_categoria")]
        public string ValorCategoria { get; set; }
    }
}
