using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
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
        Estadual = 9, 

        [Display(Name = "Nacional")]
        Nacional = 39,

        [Display(Name = "Internacional")]
        Internacional = 2,

        [Display(Name = "Internacional com valor e efeito de Nacional")]
        InternacionalComValorEfeitoDeNacional = 86,

        // Stela Beatriz Farias Lopes 05/2025
        [Display(Name = "Estadual")]
        EstadualCaxias = 1,

        //[Display(Name = "Interestadual")]
        //Interestadual = 2,

        // Edivilson Meurer Brum em 11/2023
        [Display(Name = "Internacional")]
        Internacional87 = 87,

        // Bruna Liege da Silva Rodrigues 09/2023
        [Display(Name = "Nacional")]
        Nacional37 = 37,

        // Bruna Liege da Silva Rodrigues 02/2023
        [Display(Name = "Internacional")]
        Internacional10 = 10,

    }
}
