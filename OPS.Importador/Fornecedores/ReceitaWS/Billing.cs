using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.ReceitaWS
{
    #region ReceitaWS DTO

    public class Billing
    {
        [JsonPropertyName("free")]
        public bool Free { get; set; }

        [JsonPropertyName("database")]
        public bool Database { get; set; }
    }


    #endregion ReceitaWS DTO
}