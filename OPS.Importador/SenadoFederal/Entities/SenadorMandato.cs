namespace OPS.Importador.Senado.Entities;

public class LegislaturaDoMandato
{
    public string NumeroLegislatura { get; set; }
    public string DataInicio { get; set; }
    public string DataFim { get; set; }
}

public class SenadorMandato
{
    public MandatoParlamentar MandatoParlamentar { get; set; }
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

