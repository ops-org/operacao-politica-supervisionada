using System.Text.Json.Serialization;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("fornecedor_atividade")]
    public class FornecedorAtividade
    {

        [Key]
        [Column("id")]
        [JsonPropertyName("codigo")]
        public int Id { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; }

        [Column("descricao")]
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }
    }
}
