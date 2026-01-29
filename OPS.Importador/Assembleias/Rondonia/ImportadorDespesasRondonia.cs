using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Rondonia;

/// <summary>
/// https://transparencia.al.ro.leg.br/Deputados/VerbaIndenizatoria/
/// </summary>
public class ImportadorDespesasRondonia : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
    private readonly List<DeputadoEstadual> deputados;

    public ImportadorDespesasRondonia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://sicavi.al.ro.leg.br/",
            Estado = Estados.Rondonia,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };

        deputados = dbContext.DeputadosEstaduais.Where(x => x.IdEstado == config.Estado.GetHashCode()).ToList();
    }

    public override async Task ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        await ImportarCotaParlamentar(context, ano, mes);
        await ImportarDiarias(context, ano, mes);
    }

    private async Task ImportarDiarias(IBrowsingContext context, int ano, int mes)
    {
        var address = $"https://transparencia.al.ro.leg.br/Deputados/Diarias/?nome=&ano={ano}&mes={mes}";
        var document = await context.OpenAsyncAutoRetry(address);

        var indice = 0;
        var idxCredor = indice++;
        var idxCargo = indice++;
        var idxDestino = indice++;
        var idxFinalidade = indice++;
        var idxQuantidade = indice++;
        var idxValor = indice++;
        var idxMeioTransporte = indice++;
        var idxAcoes = indice++;

        var diarias = document.QuerySelectorAll("#tabela tbody tr");
        foreach (var diaria in diarias)
        {
            var colunas = diaria.QuerySelectorAll("td");
            var link = (colunas[idxAcoes].QuerySelector("a") as IHtmlAnchorElement).Href;

            var nomeDeputado = colunas[idxCredor].TextContent;
            if (nomeDeputado == "Luiz Eduardo Schincaglia")
                nomeDeputado = "Luis Eduardo Schincaglia";

            using (logger.BeginScope(new Dictionary<string, object> { ["Tipo"] = "VerbaIndenizatoria", ["Parlamentar"] = nomeDeputado }))
            {
                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = colunas[idxCredor].TextContent,
                    Ano = (short)ano,
                    Mes = (short)mes,
                    TipoDespesa = "Diárias",
                    DataEmissao = new DateOnly(ano, mes, 1),
                    Valor = Convert.ToDecimal(colunas[idxValor].TextContent, cultureInfo),
                    Observacao = $"Diárias: {colunas[idxQuantidade].TextContent}; Trecho: {colunas[idxDestino].TextContent}; Transporte: {colunas[idxMeioTransporte].TextContent}; Link: {link}",
                };


                InserirDespesaTemp(despesaTemp);
            }
        }
    }

    private async Task ImportarCotaParlamentar(IBrowsingContext context, int ano, int mes)
    {
        var address = $"https://transparencia.al.ro.leg.br/Deputados/VerbaIndenizatoria/";
        var document = await context.OpenAsyncAutoRetry(address);
        var gabinetes = document.QuerySelectorAll("#gabinete option").ToList();

        foreach (var item in gabinetes)
        {
            var gabinete = item as IHtmlOptionElement;
            if (string.IsNullOrEmpty(gabinete.Value)) continue;

            using (logger.BeginScope(new Dictionary<string, object> { ["Tipo"] = "VerbaIndenizatoria", ["Parlamentar"] = gabinete.Text }))
            {
                var deputado = deputados.Find(x => gabinete.Value.Contains(x.Gabinete.ToString()));

                if (deputado == null)
                {
                    deputado = deputados.Find(x => gabinete.Text.Split('-')[0].Trim().Equals(x.NomeCivil, StringComparison.InvariantCultureIgnoreCase));
                    if (deputado != null)
                    {
                        deputado.Gabinete = Convert.ToInt32(gabinete.Value);
                        dbContext.SaveChanges();
                    }
                    else if (gabinete.Value != "54") // STI DA SILVA - DEPUTADO TESTE STI
                    {
                        logger.LogError("Parlamentar {Gabinete}: {Parlamentar} não existe", gabinete.Value, gabinete.Text);
                    }
                }

                IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form#form_busca_verba");
                var dcForm = new Dictionary<string, string>();
                dcForm.Add("ano", ano.ToString());
                dcForm.Add("mes", mes.ToString());
                dcForm.Add("gabinete", gabinete.Value);
                var subDocument = await form.SubmitAsyncAutoRetry(dcForm);
                var mensagem = subDocument.QuerySelector("#tabela .dataTables_empty");
                if (mensagem != null)
                {
                    logger.LogWarning("Parlamentar {Gabinete}: {Parlamentar} sem gastos no mês! Detalhes: {Mensagem}", gabinete.Value, gabinete.Text, mensagem.TextContent);
                }

                // Há CPF com mascara cagada: Prestador: NOME ***.863.572*-** ENDERECO
                var patternPrestador = @"Prestador: (?<prestador>.*?) (?<cnpj>\d{5,20}|(\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2})|(\d{3}\.\d{3}\.\d{3}-\d{2})|(\*{3}\.\d{3}\.\d{3}\*-\*{2})) (?<endereco>.*)"; // Classe: (?<classe>[^|]*) | Data: (?<data>\\d{2}\\/\\d{2}\\/\\d{4}) | Valor R\\$ (?<valor>[\\d.,]*) | (.*)
                decimal valorTotalCalculado = 0;
                decimal valorTotalPagina = 0;
                var despesasIncluidas = 0;

                var despesas = subDocument.QuerySelectorAll("#tabela tbody tr");
                foreach (var despesa in despesas)
                {
                    var titulo = despesa.QuerySelector("td h4");
                    if (titulo != null)
                    {
                        //logger.LogInformation(titulo.TextContent.Trim());
                        valorTotalPagina += Convert.ToDecimal(titulo.TextContent.Split("R$")[1].Trim(), cultureInfo);

                        continue;
                    }

                    var linha = despesa.QuerySelector("td").TextContent.Trim();
                    if (string.IsNullOrEmpty(linha)) continue;

                    var linhaPartes = linha.Trim().Replace("|", "").Split("\n");

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = gabinete.Text.Replace("DEPUTADO ", "").Replace("DEPUTADA ", "").ToTitleCase(),
                        Ano = (short)ano,
                        Mes = (short)mes,
                        Origem = $"{ano}-{mes}-{gabinete.Text}"
                    };

                    if (linhaPartes.Length > 2) // Verbas Gerais
                    {
                        despesaTemp.TipoDespesa = linhaPartes[1].Split(":")[1].Trim().ToTitleCase();
                        despesaTemp.DataEmissao = DateOnly.Parse(linhaPartes[2].Split(":")[1].Trim(), cultureInfo);
                        despesaTemp.Valor = Convert.ToDecimal(linhaPartes[3].Split("R$")[1].Trim(), cultureInfo);

                        if (despesa.QuerySelector("td a") != null)
                            despesaTemp.Observacao = (despesa.QuerySelector("td a") as IHtmlAnchorElement).Href;

                        var maches = Regex.Matches(linhaPartes[0], patternPrestador);
                        if (maches.Any())
                        {
                            Match matchPrestador = maches[0];
                            despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericosExetoAsterisco(matchPrestador.Groups["cnpj"].Value);
                            despesaTemp.NomeFornecedor = matchPrestador.Groups["prestador"].Value.Trim();
                        }
                        else if (linhaPartes[0].Contains("COPIADORA RORIZ LTDA 22. 882.427/0001-01"))
                        {
                            despesaTemp.CnpjCpf = "22882427000101"; // 22.882.427/0001-01
                            despesaTemp.NomeFornecedor = "COPIADORA RORIZ LTDA";
                        }
                        else if (linhaPartes[0].Contains("A.S. DE ALMEIDA ALINHAMENTOS 03770600/0001-27"))
                        {
                            despesaTemp.CnpjCpf = "03770600000127";  // 03.770.600/0001-27
                            despesaTemp.NomeFornecedor = "A.S. DE ALMEIDA ALINHAMENTOS";
                        }
                        else if (linhaPartes[0].Contains("ARNALDO GUIMARAES NETO 704.514.94849"))
                        {
                            despesaTemp.CnpjCpf = "70451494849";  // 704.514.948-49
                            despesaTemp.NomeFornecedor = "ARNALDO GUIMARAES NETO";
                        }
                        else
                        {
                            logger.LogError("Fornecedor invalido: {Fornecedor}", linhaPartes[0]);
                        }
                    }
                    else // Verbas de Saúde
                    {
                        despesaTemp.TipoDespesa = "Verbas de Saúde";
                        despesaTemp.DataEmissao = DateOnly.Parse(linhaPartes[0].Split(":")[1].Trim(), cultureInfo);
                        despesaTemp.Valor = Convert.ToDecimal(linhaPartes[1].Split("R$")[1].Trim(), cultureInfo);
                    }

                    InserirDespesaTemp(despesaTemp);
                    valorTotalCalculado += despesaTemp.Valor;
                    despesasIncluidas++;
                }

                ValidaValorTotal(string.Empty, valorTotalPagina, valorTotalCalculado, despesasIncluidas);
            }
        }
    }

    public override void AjustarDados()
    {
        connection.Execute($@"
UPDATE temp.cl_despesa_temp dt
SET nome = d.nome_parlamentar
FROM assembleias.cl_deputado d 
WHERE (d.nome_civil ILIKE dt.nome_civil OR d.nome_importacao ILIKE dt.nome_civil)
and despesa_tipo = 'Diárias'
and d.id_estado = {idEstado}");
    }
}
