using System.Collections.Generic;

namespace OPS.Importador.CamaraFederal;

public class Dados
{
    public string cpf { get; set; }
    public string dataFalecimento { get; set; } //DateTime?
    public string dataNascimento { get; set; } //DateTime?
    public string escolaridade { get; set; }
    public int id { get; set; }
    public string municipioNascimento { get; set; }
    public string nomeCivil { get; set; }
    public List<string> redeSocial { get; set; }
    public string sexo { get; set; }
    public string ufNascimento { get; set; }
    public UltimoStatus ultimoStatus { get; set; }
    public string uri { get; set; }
    public string urlWebsite { get; set; }
}
