using System.Text.Json.Serialization;

namespace OPS.Importador.Empresa.ReceitaWS
{
    #region ReceitaWS DTO
    public class AtividadePrincipal
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }


    #endregion ReceitaWS DTO
}