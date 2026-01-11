namespace OPS.Importador.SenadoFederal.Entities;

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
    public List<Exercicio> Exercicio { get; set; }
}
