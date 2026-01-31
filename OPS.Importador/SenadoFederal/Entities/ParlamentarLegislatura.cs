namespace OPS.Importador.SenadoFederal.Entities;

public class ParlamentarLegislatura
{
    public ListaParlamentarLegislatura ListaParlamentarLegislatura { get; set; }
}

public class ListaParlamentarLegislatura
{
    public string NoNamespaceSchemaLocation { get; set; }

    public Metadados Metadados { get; set; }

    public Parlamentares Parlamentares { get; set; }
}
