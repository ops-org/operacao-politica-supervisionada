namespace OPS.Importador.SenadoFederal.Entities;

public class Parlamentar
{
    public IdentificacaoParlamentar IdentificacaoParlamentar { get; set; }

    public Mandatos Mandatos { get; set; }
}

public class Parlamentares
{
    public List<Parlamentar> Parlamentar { get; set; }
}
