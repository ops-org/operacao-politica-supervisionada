using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor")]
    public class Fornecedor
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cnpj_cpf")]
        [StringLength(15)]
        public string? CnpjCpf { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("categoria")]
        [StringLength(2)]
        public string? Categoria { get; set; }

        [Column("doador")]
        public bool Doador { get; set; }

        [Column("controle")]
        public short? Controle { get; set; }

        [Column("mensagem")]
        [StringLength(8000)]
        public string? Mensagem { get; set; }

        [Column("valor_total_ceap_camara", TypeName = "decimal(16,2)")]
        public decimal ValorTotalCeapCamara { get; set; }

        [Column("valor_total_ceap_senado", TypeName = "decimal(16,2)")]
        public decimal ValorTotalCeapSenado { get; set; }

        [Column("valor_total_ceap_assembleias", TypeName = "decimal(16,2)")]
        public decimal ValorTotalCeapAssembleias { get; set; }

        // Navigation properties
        public virtual FornecedorInfo? FornecedorInfo { get; set; }
        public virtual ICollection<FornecedorSocio> FornecedorSocios { get; set; } = new List<FornecedorSocio>();
        public virtual ICollection<FornecedorAtividadeSecundaria> FornecedorAtividadeSecundarias { get; set; } = new List<FornecedorAtividadeSecundaria>();

        public virtual ICollection<AssembleiasLegislativas.DespesaAssembleias> DespesasAssembleias { get; set; } = new List<AssembleiasLegislativas.DespesaAssembleias>();
        //public virtual ICollection<CamaraFederal.DespesaCamara> DespesasCamara { get; set; } = new List<CamaraFederal.DespesaCamara>();
        //public virtual ICollection<SenadoFederal.DespesaSenado> DespesasSenado { get; set; } = new List<SenadoFederal.DespesaSenado>();
    }
}
