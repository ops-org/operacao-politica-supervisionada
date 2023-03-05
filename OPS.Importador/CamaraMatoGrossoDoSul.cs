using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa da
    /// 
    /// </summary>
    public class CamaraMatoGrossoDoSul : ImportadorCotaParlamentarBase
    {
        public CamaraMatoGrossoDoSul(ILogger<CamaraMatoGrossoDoSul> logger, IConfiguration configuration, IDbConnection connection) :
            base("MS", logger, configuration, connection)
        {
        }

        //public override void ImportarParlamentares()
        //{
        //    var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        //    var config = Configuration.Default.WithDefaultLoader();
        //    var context = BrowsingContext.New(config);

        //    var address = $"https://www.al.ba.gov.br/deputados/legislatura-atual";
        //    var task = context.OpenAsync(address);
        //    task.Wait();
        //    var document = task.Result;
        //    if (document.StatusCode != HttpStatusCode.OK)
        //    {
        //        Console.WriteLine($"{address} {document.StatusCode}");
        //    };

        //    var parlamentares = document.QuerySelectorAll(".fe-div-table>div");
        //    foreach (var parlamentar in parlamentares)
        //    {
        //        var deputado = new DeputadoEstadual();
        //        deputado.IdEstado = (ushort)idEstado;

        //        deputado.UrlPerfil = (parlamentar.QuerySelector(".list-item a") as IHtmlAnchorElement).Href;
        //        deputado.UrlFoto = (parlamentar.QuerySelector(".list-item img") as IHtmlImageElement)?.Source;
        //        deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());

        //        deputado.NomeParlamentar = parlamentar.QuerySelector(".deputado-nome span").TextContent.Trim();
        //        var partido = parlamentar.QuerySelector(".partido-nome").TextContent.Trim();

        //        //Thread.Sleep(TimeSpan.FromSeconds(15));
        //        var taskSub = context.OpenAsync(deputado.UrlPerfil);
        //        taskSub.Wait();
        //        var subDocument = taskSub.Result;
        //        if (subDocument.StatusCode != HttpStatusCode.OK)
        //        {
        //            Console.WriteLine($"{deputado.UrlPerfil} {subDocument.StatusCode}");
        //            continue;
        //        };

        //        var detalhes = subDocument.QuerySelectorAll(".dados-deputado p");
        //        deputado.NomeCivil = detalhes[0].QuerySelector("span").TextContent.Trim();

        //        var profissaoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("PROFISSÃO", StringComparison.InvariantCultureIgnoreCase));
        //        if (profissaoElemento is not null)
        //        {
        //            deputado.Profissao = profissaoElemento.QuerySelector("span").TextContent.Trim();
        //        }

        //        var nascimentoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("NASCIMENTO", StringComparison.InvariantCultureIgnoreCase));
        //        if (nascimentoElemento is not null)
        //        {
        //            var nascimentoComNaturalidade = nascimentoElemento.QuerySelector("span").TextContent.Split(',');
        //            deputado.Nascimento = DateOnly.Parse(nascimentoComNaturalidade[0].Trim(), cultureInfo);
        //            deputado.Naturalidade = nascimentoComNaturalidade[1].Trim();
        //        }

        //        var sexoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("SEXO", StringComparison.InvariantCultureIgnoreCase));
        //        if (sexoElemento is not null)
        //            deputado.Sexo = sexoElemento.QuerySelector("span").TextContent.Trim()[0].ToString();

        //        var contatos = subDocument.QuerySelectorAll(".fe-dep-dados-ajsut-mobile .linha-cv strong").First(x => x.TextContent.Trim() == "Contato").ParentElement;
        //        var telefoneElemento = contatos.QuerySelectorAll("p").FirstOrDefault(x => x.TextContent.StartsWith("Tel"));
        //        if (telefoneElemento != null)
        //            deputado.Telefone = String.Join(" ", telefoneElemento.ParentElement.QuerySelectorAll("span").Select(x => x.TextContent.Trim()));

        //        deputado.Email = contatos.QuerySelector("p a span").TextContent?.Trim();

        //        var IdPartido = connection.GetList<Partido>(new { sigla = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA").Replace("REPUB", "REPUBLICANOS") }).FirstOrDefault()?.Id;
        //        if (IdPartido == null)
        //            throw new Exception("Partido Inexistenete");

        //        deputado.IdPartido = IdPartido.Value;

        //        var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, nome_parlamentar = deputado.NomeParlamentar }).FirstOrDefault()?.Id;
        //        if (IdDeputado == null)
        //            connection.Insert(deputado);
        //        else
        //        {
        //            deputado.Id = IdDeputado.Value;
        //            connection.Update(deputado);
        //        }
        //    }
        //}

        public override void ImportarArquivoDespesas(int ano)
        {
            ImportarArquivoDespesasAsync(ano).Wait();
        }

        public async Task ImportarArquivoDespesasAsync(int ano)
        {
            logger.LogInformation("Consultando ano {Ano}!", ano);
            //LimpaDespesaTemporaria();

            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var pagina = 0;
            var config = Configuration.Default.WithDefaultLoader().WithDefaultCookies();
            var context = BrowsingContext.New(config);

            while (true)
            {
                logger.LogInformation("Consultando pagina {Pagina}!", pagina++);

                var address = $"https://consulta.transparencia.al.ms.gov.br/ceap/";
                var document = await context.OpenAsync(address);
                if (document.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogError($"{address} {document.StatusCode}");

                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    pagina--;
                    continue;
                };

                IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");
                (form.Elements["id_ac_verbaindenizatoria_ano_referencia"] as IHtmlInputElement).Value = ano.ToString();
                (form.Elements["SC_verbaindenizatoria_ano_referencia_input_2"] as IHtmlInputElement).Value = ano.ToString();
                IDocument resultDocument = await form.SubmitAsync();

                var despesas = document.QuerySelectorAll("#sc_grid_body");
                foreach (var despesa in despesas)
                {
                    // N° PROCESSO	N° NF	MÊS/ANO	DEPUTADO (A)	CATEGORIA	VALOR (R$)
                    var colunas = despesa.QuerySelectorAll("td");
                    var linkDetalhes = (colunas[0].Children[0] as IHtmlAnchorElement).Href;
                    var processo = colunas[0].QuerySelector("span").TextContent.Trim();

                    var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Ano = (short)ano,
                        Documento = colunas[0].QuerySelector("span").TextContent.Trim() + "/" + colunas[1].TextContent.Trim(),
                        DataEmissao = Convert.ToDateTime("01/" + colunas[2].TextContent.Trim(), cultureInfo),
                        Nome = colunas[3].TextContent.Trim(),
                        TipoDespesa = colunas[4].TextContent.Trim(),
                    };

                    var documentDetalhes = await context.OpenAsync(linkDetalhes);
                    if (documentDetalhes.StatusCode != HttpStatusCode.OK)
                    {
                        logger.LogError($"{linkDetalhes} {documentDetalhes.StatusCode}");
                    };

                    var despesasDetalhes = documentDetalhes.QuerySelectorAll(".tabela-cab tbody tr");
                    foreach (var detalhes in despesasDetalhes)
                    {
                        // CATEGORIA	Nº NOTA/RECIBO	CPF/CNPJ	NOME DO FORNECEDOR	VALOR
                        var colunasDetalhes = detalhes.QuerySelectorAll("td");

                        objCamaraEstadualDespesaTemp.Documento = processo + "/" + colunasDetalhes[1].TextContent.Trim();
                        objCamaraEstadualDespesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(colunasDetalhes[2].TextContent.Trim());
                        objCamaraEstadualDespesaTemp.Empresa = colunasDetalhes[3].TextContent.Trim();
                        objCamaraEstadualDespesaTemp.Valor = Convert.ToDecimal(colunasDetalhes[4].TextContent.Replace("R$", "").Trim(), cultureInfo);
                    }


                    connection.Insert(objCamaraEstadualDespesaTemp);
                }


                if (document.QuerySelector(".paginate-button-next").ClassList.Contains("disabled")) break;
            }
        }

    }

}
