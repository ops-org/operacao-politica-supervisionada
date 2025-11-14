using Dapper;

namespace OPS.Core.Entity
{

    [Table("cl_deputado_de_para", Schema = "ops_tmp")]
    public class DeputadoEstadualDepara
    {
        [Key, Required]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_estado")]
        public ushort IdEstado { get; set; }

        [Column("nome")]
        public string Nome { get; set; }
    }
}
