using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    /// <summary>
    /// Match de funcionários - apenas registros com match confirmado
    /// </summary>
    [Table("cf_deputado_funcionario_match_temp", Schema = "temp")]
    public class CamaraFederalFuncionarioMatchTemp
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Chave original dos dados brutos (fonte: site da Câmara)
        /// </summary>
        [Required]
        [Column("chave_origem")]
        [StringLength(45)]
        public string ChaveOrigem { get; set; } = null!;

        /// <summary>
        /// Chave do funcionário existente no banco final
        /// Sempre preenchida (só existe registro se tiver match)
        /// </summary>
        [Required]
        [Column("chave_existente")]
        [StringLength(45)]
        public string ChaveExistente { get; set; } = null!;

        /// <summary>
        /// Nome do funcionário
        /// </summary>
        [Required]
        [Column("nome")]
        [StringLength(100)]
        public string Nome { get; set; } = null!;

        /// <summary>
        /// Regra aplicada: 1=Chave exata, 2=Período+Deputado+Nome, 3=Histórico
        /// </summary>
        [Column("match_regra")]
        public int MatchRegra { get; set; }

        /// <summary>
        /// Data da coleta/importação
        /// </summary>
        [Column("data_coleta")]
        public DateOnly DataColeta { get; set; }
    }

    /// <summary>
    /// Regras de matching disponíveis
    /// </summary>
    public enum MatchRegra
    {
        NaoDefinido = 0,
        ChaveExata = 1,           // Confiança 100
        PeriodoDeputadoNome = 2,  // Confiança 80
        HistoricoNomeDeputado = 3 // Confiança 60
    }
}
