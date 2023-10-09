using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    public class CamaraPernambuco : ImportadorCotaParlamentarBase
    {
        public CamaraPernambuco(ILogger<CamaraSaoPaulo> logger, IConfiguration configuration, IDbConnection connection)
            : base("PE", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            using (var db = new AppDb())
            {
                var address = $"https://www.alepe.pe.gov.br/parlamentares/";
                var task = context.OpenAsync(address);
                task.Wait();
                var document = task.Result;
                if (document.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{address} {document.StatusCode}");
                };

                var parlamentares = document.QuerySelectorAll("#parlamentares-modo-fotos ul li");
                foreach (var item in parlamentares)
                {
                    var deputado = new DeputadoEstadual();
                    deputado.IdEstado = (ushort)idEstado;

                    deputado.UrlPerfil = (item.QuerySelector("a") as IHtmlAnchorElement).Href;
                    var taskSub = context.OpenAsync(deputado.UrlPerfil);
                    taskSub.Wait();
                    var subDocument = taskSub.Result;
                    if (document.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"{deputado.UrlPerfil} {subDocument.StatusCode}");
                        continue;
                    };

                    var cabecalho = subDocument.QuerySelector(".parlamentares-view-header");
                    deputado.NomeParlamentar = cabecalho.QuerySelector(".text .title").TextContent.Trim();
                    var partido = cabecalho.QuerySelector(".text .subtitle").TextContent.Trim();

                    var info1 = subDocument.QuerySelectorAll(".parlamentares-view-info dl.first dd");
                    deputado.NomeCivil = info1[0].TextContent.Trim();
                    deputado.Naturalidade = info1[1].TextContent.Trim();
                    deputado.Email = info1[2].TextContent.Trim();
                    deputado.Site = info1[3].TextContent.Trim();
                    //var redesSociais = info1[4].TextContent.Trim();

                    var info2 = subDocument.QuerySelectorAll(".parlamentares-view-info dl.last dd");
                    //var aniversario = info2[0].TextContent.Trim();
                    deputado.Profissao = info2[1].TextContent.Trim();
                    deputado.Telefone = info2[2].TextContent.Trim();
                    //var gabinete = info2[3].TextContent.Trim();

                    var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = partido }).FirstOrDefault()?.Id;
                    if (IdPartido == null)
                    {
                        IdPartido = connection.GetList<Core.Entity.Partido>(new { Nome = partido }).FirstOrDefault()?.Id;
                        if (IdPartido == null)
                            throw new Exception("Partido Inexistenete");
                    }

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
        }

        /// <summary>
        /// Dados a partir de 2013
        /// </summary>
        /// <param name="ano"></param>
        /// <returns></returns>
        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();
            string urlOrigem, caminhoArquivo;

            urlOrigem = string.Format(" https://www.al.sp.gov.br/repositorioDados/deputados/despesas_gabinetes_{0}.xml", ano);
            caminhoArquivo = $"{tempPath}/CLSP-{ano}.xml";

            arquivos.Add(urlOrigem, caminhoArquivo);
            return arquivos;
        }

        protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        {
            using (var banco = new AppDb())
            {
                LimpaDespesaTemporaria();
                Dictionary<string, uint> lstHash = ObterHashes(ano);

                CarregaDados(banco, caminhoArquivo, ano, lstHash);

                SincronizarHashes(lstHash);
                AjustarDados();
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

        private void AjustarDados()
        {
            connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '00000000000008' WHERE empresa = 'PEDÁGIO';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '00000000000009' WHERE empresa = 'TAXI';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04645433000155' WHERE empresa = 'ACACIA PRANDI ZANIN';
        ");
        }

        private void CarregaDados(AppDb banco, string file, int ano, Dictionary<string, UInt32> lstHash)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

            var doc = new XmlDocument();
            doc.Load(file);
            XmlNode deputados = doc.DocumentElement;

            var despesas = deputados.SelectNodes("despesa");
            var sqlFields = new StringBuilder();
            var sqlValues = new StringBuilder();

            using (var db = new AppDb())
            {
                foreach (XmlNode fileNode in despesas)
                {
                    var matricula = fileNode.SelectSingleNode("Matricula").InnerText.Trim();
                    var deputado = fileNode.SelectSingleNode("Deputado").InnerText.Trim();
                    var tipoDespesa = fileNode.SelectSingleNode("Tipo").InnerText.Trim();
                    //var ano = fileNode.SelectSingleNode("Ano").InnerText.Trim();
                    var mes = fileNode.SelectSingleNode("Mes").InnerText.Trim();
                    var cnpj = fileNode.SelectSingleNode("CNPJ")?.InnerText?.Trim();
                    var fornecedor = fileNode.SelectSingleNode("Fornecedor").InnerText.Trim();
                    var valor = fileNode.SelectSingleNode("Valor").InnerText.Trim();

                    var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = deputado,
                        Cpf = matricula,
                        Ano = (short)ano,
                        TipoDespesa = tipoDespesa,
                        Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor, cultureInfo) : 0,
                        DataEmissao = new DateTime(ano, Convert.ToInt32(mes), 1),
                        CnpjCpf = cnpj,
                        Empresa = fornecedor
                    };

                    if (RegistroExistente(objCamaraEstadualDespesaTemp, lstHash))
                        continue;

                    connection.Insert(objCamaraEstadualDespesaTemp);
                }
            }
        }
    }
}
