using System.Collections.Generic;
using System.Xml.Serialization;

namespace OPS.Importador.CamaraFederal.Entities;

[XmlRoot(ElementName = "xml")]
public class DeputadoDetalhes
{
    public Dados dados { get; set; }
    public List<Link> links { get; set; }
}