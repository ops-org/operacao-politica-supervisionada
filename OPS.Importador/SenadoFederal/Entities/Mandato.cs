using System.Collections.Generic;

namespace OPS.Importador.SenadoFederal.Entities;

public class Mandato
{
    public string CodigoMandato { get; set; }

    public string UfParlamentar { get; set; }

    public LegislaturaDoMandato PrimeiraLegislaturaDoMandato { get; set; }

    public LegislaturaDoMandato SegundaLegislaturaDoMandato { get; set; }

    public string DescricaoParticipacao { get; set; }

    public Titular Titular { get; set; }

    public Suplentes Suplentes { get; set; }

    public Exercicios Exercicios { get; set; }

    public Partidos Partidos { get; set; }
}

public class Mandatos
{
    public List<Mandato> Mandato { get; set; }
}