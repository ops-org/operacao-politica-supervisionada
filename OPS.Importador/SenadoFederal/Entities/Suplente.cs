using System.Collections.Generic;

namespace OPS.Importador.Senado.Entities;

public class Suplente
{
    public string DescricaoParticipacao { get; set; }

    public string CodigoParlamentar { get; set; }

    public string NomeParlamentar { get; set; }
}

public class Suplentes
{
    public List<Suplente> Suplente { get; set; }
}