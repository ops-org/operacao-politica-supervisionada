using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.CamaraFederal;
using OPS.Infraestrutura.Entities.SenadoFederal;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor")]
    public class Fornecedor
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("cnpj_cpf")]
        [StringLength(15)]
        public string? CnpjCpf { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("doador")]
        public byte Doador { get; set; }

        [Column("controle")]
        public sbyte? Controle { get; set; }

        [Column("mensagem")]
        [StringLength(8000)]
        public string? Mensagem { get; set; }

        // Navigation properties
        public virtual FornecedorInfo? FornecedorInfo { get; set; }
        public virtual ICollection<FornecedorSocio> FornecedorSocios { get; set; } = new List<FornecedorSocio>();
        public virtual ICollection<AssembleiasLegislativas.Despesa> DespesasAssembleias { get; set; } = new List<AssembleiasLegislativas.Despesa>();
        public virtual ICollection<CamaraFederal.DespesaCamara> DespesasCamara { get; set; } = new List<CamaraFederal.DespesaCamara>();
        public virtual ICollection<SenadoFederal.DespesaSenado> DespesasSenado { get; set; } = new List<SenadoFederal.DespesaSenado>();
    }
}
