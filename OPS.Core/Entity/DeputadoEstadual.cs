using System;
using Dapper;

namespace OPS.Core.Entity
{

    [Table("cl_deputado")]
    public class DeputadoEstadual
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("matricula")]
        public uint? Matricula { get; set; }

        [Column("gabinete")]
        public uint? Gabinete { get; set; }

        [Column("id_partido")]
        public ushort? IdPartido { get; set; }

        [Column("id_estado")]
        public ushort IdEstado { get; set; }

        [Column("cpf")]
        public string CPF { get; set; }

        [Column("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [Column("nome_civil")]
        public string NomeCivil { get; set; }

        [ReadOnly(true)]
        [Column("nome_importacao")]
        public string NomeImportacao { get; set; }

        [Column("nascimento")]
        public DateOnly? Nascimento { get; set; }

        [Column("sexo")]
        public string Sexo { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("naturalidade")]
        public string Naturalidade { get; set; }

        [Column("escolaridade")]
        public string Escolaridade { get; set; }

        [Column("profissao")]
        public string Profissao { get; set; }

        [Column("telefone")]
        public string Telefone { get; set; }

        [Column("site")]
        public string Site { get; set; }

        [NotMapped]
        [Column("instagram")]
        public string Instagram { get; set; }

        [NotMapped]
        [Column("facebook")]
        public string Facebook { get; set; }

        [NotMapped]
        [Column("twitter")]
        public string Twitter { get; set; }

        [NotMapped]
        [Column("youtube")]
        public string YouTube { get; set; }

        [NotMapped]
        [Column("tiktok")]
        public string Tiktok { get; set; }

        [Column("perfil")]
        public string UrlPerfil { get; set; }

        [Column("foto")]
        public string UrlFoto { get; set; }

        [Column("valor_total_ceap")]
        public decimal ValorTotalCEAP { get; set; }
    }
}
