using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Importador.Utilities;
using Serilog;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa da Bahia
    /// 
    /// </summary>
    public class CamaraBahia : ImportadorCotaParlamentarBase
    {
        public CamaraBahia(ILogger<CamaraBahia> logger, IConfiguration configuration, IDbConnection connection) :
            base("BA", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            var address = $"https://www.al.ba.gov.br/deputados/legislatura-atual";
            var task = context.OpenAsync(address);
            task.Wait();
            var document = task.Result;
            if (document.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"{address} {document.StatusCode}");
            };

            var parlamentares = document.QuerySelectorAll(".fe-div-table>div");
            foreach (var parlamentar in parlamentares)
            {
                var deputado = new DeputadoEstadual();
                deputado.IdEstado = (ushort)idEstado;

                deputado.UrlPerfil = (parlamentar.QuerySelector(".list-item a") as IHtmlAnchorElement).Href;
                deputado.UrlFoto = (parlamentar.QuerySelector(".list-item img") as IHtmlImageElement)?.Source;
                deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());

                deputado.NomeParlamentar = parlamentar.QuerySelector(".deputado-nome span").TextContent.Trim();
                var partido = parlamentar.QuerySelector(".partido-nome").TextContent.Trim();

                //Thread.Sleep(TimeSpan.FromSeconds(15));
                var subDocument = context.OpenAsyncAutoRetry(deputado.UrlPerfil).GetAwaiter().GetResult();

                var detalhes = subDocument.QuerySelectorAll(".dados-deputado p");
                deputado.NomeCivil = detalhes[0].QuerySelector("span").TextContent.Trim();

                var profissaoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("PROFISSÃO", StringComparison.InvariantCultureIgnoreCase));
                if (profissaoElemento is not null)
                {
                    deputado.Profissao = profissaoElemento.QuerySelector("span").TextContent.Trim();
                }

                var nascimentoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("NASCIMENTO", StringComparison.InvariantCultureIgnoreCase));
                if (nascimentoElemento is not null)
                {
                    var nascimentoComNaturalidade = nascimentoElemento.QuerySelector("span").TextContent.Split(',');
                    deputado.Nascimento = DateOnly.Parse(nascimentoComNaturalidade[0].Trim(), cultureInfo);
                    deputado.Naturalidade = nascimentoComNaturalidade[1].Trim();
                }

                var sexoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("SEXO", StringComparison.InvariantCultureIgnoreCase));
                if (sexoElemento is not null)
                    deputado.Sexo = sexoElemento.QuerySelector("span").TextContent.Trim()[0].ToString();

                var contatos = subDocument.QuerySelectorAll(".fe-dep-dados-ajsut-mobile .linha-cv strong").First(x => x.TextContent.Trim() == "Contato").ParentElement;
                var telefoneElemento = contatos.QuerySelectorAll("p").FirstOrDefault(x => x.TextContent.StartsWith("Tel"));
                if (telefoneElemento != null)
                    deputado.Telefone = String.Join(" ", telefoneElemento.ParentElement.QuerySelectorAll("span").Select(x => x.TextContent.Trim()));

                deputado.Email = contatos.QuerySelector("p a span").TextContent?.Trim();

                var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA").Replace("REPUB", "REPUBLICANOS") }).FirstOrDefault()?.Id;
                if (IdPartido == null)
                    throw new Exception("Partido Inexistenete");

                deputado.IdPartido = IdPartido.Value;

                var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, nome_parlamentar = deputado.NomeParlamentar }).FirstOrDefault()?.Id;
                if (IdDeputado == null)
                    connection.Insert(deputado);
                else
                {
                    deputado.Id = IdDeputado.Value;
                    connection.Update(deputado);
                }
            }
        }

        public override void ImportarArquivoDespesas(int ano)
        {
            logger.LogInformation("Consultando ano {Ano}!", ano);
            LimpaDespesaTemporaria();
            Dictionary<string, uint> lstHash = ObterHashes(ano);

            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var pagina = 0;

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            var handler = new DefaultHttpRequester { Timeout = TimeSpan.FromMinutes(5) };
            var config = Configuration.Default.With(handler).WithDefaultLoader();
            var context = BrowsingContext.New(config);

            while (true)
            {
                logger.LogInformation("Consultando pagina {Pagina}!", pagina);

                var address = $"https://www.al.ba.gov.br/transparencia/prestacao-contas?ano={ano}&page={pagina++}&size=100";
                var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
                var despesas = document.QuerySelectorAll(".tabela-cab tbody tr");
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

                    try
                    {
                        var documentDetalhes = context.OpenAsyncAutoRetry(linkDetalhes).GetAwaiter().GetResult();
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

                        if (RegistroExistente(objCamaraEstadualDespesaTemp, lstHash))
                            continue;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, ex.Message);
                        continue;
                    }

                    connection.Insert(objCamaraEstadualDespesaTemp);
                }


                if (document.QuerySelector(".paginate-button-next")?.ClassList.Contains("disabled") ?? true) break;
            }

            SincronizarHashes(lstHash);
            InsereTipoDespesaFaltante();
            InsereDeputadoFaltante();
            InsereFornecedorFaltante();
            InsereDespesaFinal();
            LimpaDespesaTemporaria();

            if (ano == DateTime.Now.Year)
            {
                AtualizaValorTotal();
            }
        }
    }

}
