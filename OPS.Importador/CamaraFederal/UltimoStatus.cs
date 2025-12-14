namespace OPS.Importador.CamaraFederal;

public class UltimoStatus
{
    public string condicaoEleitoral { get; set; }
    public string data { get; set; } // Datetime
    public string descricaoStatus { get; set; }
    public Gabinete gabinete { get; set; }
    public int id { get; set; }
    public int idLegislatura { get; set; }
    public string nome { get; set; }
    public string nomeEleitoral { get; set; }
    public string siglaPartido { get; set; }
    public string siglaUf { get; set; }
    public string situacao { get; set; }
    public string uri { get; set; }
    public string uriPartido { get; set; }
    public string urlFoto { get; set; }
}
