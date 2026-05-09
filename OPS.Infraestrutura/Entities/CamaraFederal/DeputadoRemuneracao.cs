using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_remuneracao")]
    [DebuggerDisplay("Remuneracao {IdDeputado} - {Ano}/{Mes} - R$ {Valor}")]
    public class DeputadoRemuneracao
    {
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("ano")]
        public short Ano { get; set; }

        [Column("mes")]
        public short Mes { get; set; }

        [Column("valor")]
        public decimal Valor { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
