using Dapper;

namespace OPS.Core.Entity
{
    [Table("partido")]
    public class Partido
    {
        [Column("id")]
        public ushort Id { get; set; }

        [Column("legenda")]
        public ushort Legenda { get; set; }

        [Column("sigla")]
        public string Sigla { get; set; }

        [Column("nome")]
        public string Nome { get; set; }
    }
}
