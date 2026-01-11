using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OPS.Infraestrutura.Entities.Temp
{
    [Keyless]
    public class CamaraFederalDeputadoFuncionarioTemp
    {
        [Column("id")]
        public int? Id { get; set; }

        [Column("id_cf_deputado")]
        public int? IdDeputado { get; set; }

        [Required]
        [Column("chave")]
        public string Chave { get; set; } = null!;

        [Required]
        [Column("nome")]
        public string Nome { get; set; } = null!;

        [Required]
        [Column("grupo_funcional")]
        public string GrupoFuncional { get; set; } = null!;

        [Column("nivel")]
        public string? Nivel { get; set; }

        [Required]
        [Column("periodo_de")]
        public DateTime PeriodoDe { get; set; }

        [Column("periodo_ate")]
        public DateTime? PeriodoAte { get; set; }
    }
}
