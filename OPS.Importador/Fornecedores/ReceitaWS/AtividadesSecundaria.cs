using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.ReceitaWS
{
    #region ReceitaWS DTO

    public class AtividadesSecundaria
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }


    #endregion ReceitaWS DTO
}