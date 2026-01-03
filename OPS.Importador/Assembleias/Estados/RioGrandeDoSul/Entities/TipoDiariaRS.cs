using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class TipoDiaria
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nomeTipo")]
        public string NomeTipo { get; set; }
    }

    public enum TipoDiariaRS
    {
        [Display(Name = "Estadual")]
        EstadualCaxias = 1,

        [Display(Name = "Interestadual")]
        Interestadual = 2,

        [Display(Name = "Internacional")]
        Internacional = 87
    }
}
