using System.ComponentModel.DataAnnotations;

namespace OPS.Core.Enumerators
{
    public enum Estados : short
    {
        [Display(Name = "Nenhum", ShortName = "NA")]
        Nenhum = 0,

        [Display(Name = "Acre", ShortName = "AC")]
        Acre = 12,

        [Display(Name = "Alagoas", ShortName = "AL")]
        Alagoas = 27,

        [Display(Name = "Amazonas", ShortName = "AM")]
        Amazonas = 13,

        [Display(Name = "Amapá", ShortName = "AP")]
        Amapa = 16,

        [Display(Name = "Bahia", ShortName = "BA")]
        Bahia = 29,

        [Display(Name = "Ceará", ShortName = "CE")]
        Ceara = 23,

        [Display(Name = "Distrito Federal", ShortName = "DF")]
        DistritoFederal = 53,

        [Display(Name = "Espírito Santo", ShortName = "ES")]
        EspiritoSanto = 32,

        [Display(Name = "Goiás", ShortName = "GO")]
        Goias = 52,

        [Display(Name = "Maranhão", ShortName = "MA")]
        Maranhao = 21,

        [Display(Name = "Minas Gerais", ShortName = "MG")]
        MinasGerais = 31,

        [Display(Name = "Mato Grosso do Sul", ShortName = "MS")]
        MatoGrossoDoSul = 50,

        [Display(Name = "Mato Grosso", ShortName = "MT")]
        MatoGrosso = 51,

        [Display(Name = "Pará", ShortName = "PA")]
        Para = 15,

        [Display(Name = "Paraíba", ShortName = "PB")]
        Paraiba = 25,

        [Display(Name = "Pernambuco", ShortName = "PE")]
        Pernambuco = 26,

        [Display(Name = "Piauí", ShortName = "PI")]
        Piaui = 22,

        [Display(Name = "Paraná", ShortName = "PR")]
        Parana = 41,

        [Display(Name = "Rio de Janeiro", ShortName = "RJ")]
        RioDeJaneiro = 33,

        [Display(Name = "Rio Grande do Norte", ShortName = "RN")]
        RioGrandeDoNorte = 24,

        [Display(Name = "Rondônia", ShortName = "RO")]
        Rondonia = 11,

        [Display(Name = "Roraima", ShortName = "RR")]
        Roraima = 14,

        [Display(Name = "Rio Grande do Sul", ShortName = "RS")]
        RioGrandeDoSul = 43,

        [Display(Name = "Santa Catarina", ShortName = "SC")]
        SantaCatarina = 42,

        [Display(Name = "Sergipe", ShortName = "SE")]
        Sergipe = 28,

        [Display(Name = "São Paulo", ShortName = "SP")]
        SaoPaulo = 35,

        [Display(Name = "Tocantins", ShortName = "TO")]
        Tocantins = 17,
    }
}
