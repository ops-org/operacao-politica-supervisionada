using System.Collections.Generic;

namespace OPS.Core.Entity
{
    public class LegislaturaDoMandato
    {
        public string NumeroLegislatura { get; set; }
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
    }

    public class Suplente
    {
        public string DescricaoParticipacao { get; set; }
        public string CodigoParlamentar { get; set; }
        public string NomeParlamentar { get; set; }
    }

    public class Suplentes
    {
        public Suplente[] Suplente { get; set; }
    }

    public class Exercicio
    {
        public string CodigoExercicio { get; set; }
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
        public string SiglaCausaAfastamento { get; set; }
        public string DescricaoCausaAfastamento { get; set; }
        public string DataLeitura { get; set; }
    }

    public class Exercicios
    {
        public Exercicio[] Exercicio { get; set; }
    }

    public class Mandato
    {
        public string CodigoMandato { get; set; }
        public string UfParlamentar { get; set; }
        public LegislaturaDoMandato PrimeiraLegislaturaDoMandato { get; set; }
        public LegislaturaDoMandato SegundaLegislaturaDoMandato { get; set; }
        public string DescricaoParticipacao { get; set; }
        public Suplentes Suplentes { get; set; }
        public Exercicios Exercicios { get; set; }
    }

    public class Mandatos
    {
        public List<Mandato> Mandato { get; set; }
    }

    public class SenadorMandatoParlamentar
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public Mandatos Mandatos { get; set; }
    }

    public class MandatoParlamentar
    {
        public SenadorMandatoParlamentar Parlamentar { get; set; }
    }

    public class SenadorMandato
    {
        public MandatoParlamentar MandatoParlamentar { get; set; }
    }

}
