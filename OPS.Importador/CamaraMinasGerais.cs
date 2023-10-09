using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Xml;
using AngleSharp;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    public class CamaraMinasGerais : ImportadorCotaParlamentarBase
    {
        public CamaraMinasGerais(ILogger<CamaraMinasGerais> logger, IConfiguration configuration, IDbConnection connection)
            : base("MG", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            using (var db = new AppDb())
            {
                foreach (var situacao in new[] { "em_exercicio", "que_exerceram_mandato", "que_renunciaram", "que_se_afastaram", "que_perderam_mandato" })
                {
                    var doc = new XmlDocument();
                    try
                    {
                        doc.Load(@$"http://dadosabertos.almg.gov.br/ws/deputados/{situacao}");
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message == "Response status code does not indicate success: 429 (Too Many Requests).")
                            throw;

                        Thread.Sleep(1000);
                        doc.Load(@$"http://dadosabertos.almg.gov.br/ws/deputados/{situacao}");
                    }

                    XmlNode deputados = doc.DocumentElement;

                    var deputadoXml = deputados.SelectNodes("deputado");
                    var sqlFields = new StringBuilder();
                    var sqlValues = new StringBuilder();


                    foreach (XmlNode fileNode in deputadoXml)
                    {
                        var deputado = new DeputadoEstadual();
                        deputado.IdEstado = (ushort)idEstado;

                        deputado.NomeParlamentar = fileNode.SelectSingleNode("nome").InnerText;
                        var partido = fileNode.SelectSingleNode("partido").InnerText;
                        deputado.Matricula = Convert.ToUInt32(fileNode.SelectSingleNode("id").InnerText);
                        deputado.UrlPerfil = $"https://www.almg.gov.br/deputados/conheca_deputados/deputados-info.html?idDep={deputado.Matricula}&leg=19";

                        Thread.Sleep(TimeSpan.FromSeconds(1));

                        var docPerfil = new XmlDocument();
                        docPerfil.Load(@$"http://dadosabertos.almg.gov.br/ws/deputados/{deputado.Matricula}");
                        XmlNode detalhes = docPerfil.DocumentElement;

                        deputado.NomeCivil = detalhes.SelectSingleNode("nomeServidor").InnerText;
                        deputado.Naturalidade = detalhes.SelectSingleNode("naturalidadeMunicipio").InnerText;
                        deputado.Nascimento = DateOnly.Parse(detalhes.SelectSingleNode("dataNascimento").InnerText, cultureInfo);
                        deputado.Profissao = detalhes.SelectSingleNode("atividadeProfissional")?.InnerText;
                        deputado.Sexo = detalhes.SelectSingleNode("sexo").InnerText;

                        var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = partido.Replace("PATRI", "PATRIOTA") }).FirstOrDefault()?.Id;
                        if (IdPartido == null)
                        {
                            IdPartido = connection.GetList<Core.Entity.Partido>(new { Nome = partido }).FirstOrDefault()?.Id;
                            if (IdPartido == null)
                                throw new Exception("Partido Inexistenete");
                        }

                        deputado.IdPartido = IdPartido.Value;

                        var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, matricula = deputado.Matricula }).FirstOrDefault()?.Id;
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
        }

        public override void ImportarArquivoDespesas(int ano)
        {
            ProcessarDespesas(null, ano);
        }

        protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

            using (var db = new AppDb())
            {
                var lstHash = new Dictionary<string, UInt32>();
                using (var dReader = db.ExecuteReader(
                    $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano}12"))
                    while (dReader.Read())
                    {
                        var hex = Convert.ToHexString((byte[])dReader["hash"]);
                        if (!lstHash.ContainsKey(hex))
                            lstHash.Add(hex, (UInt32)dReader["id"]);
                    }

                var dc = db.ExecuteDict($"select id, matricula, nome_parlamentar from cl_deputado where id_estado = {idEstado}");
                foreach (var item in dc)
                {
                    var matricula = item["matricula"].ToString();

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    var doc = new XmlDocument();
                    doc.Load($"https://dadosabertos.almg.gov.br/ws/prestacao_contas/verbas_indenizatorias/legislatura_atual/deputados/{matricula}/datas");
                    XmlNode deputados = doc.DocumentElement;

                    var datas = deputados.SelectNodes("fechamentoVerba");

                    foreach (XmlNode data in datas)
                    {
                        var dataReferencia = Convert.ToDateTime(data.SelectSingleNode("dataReferencia ").InnerText);
                        //if (dataReferencia.Year != ano) continue;

                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        var docInner = new XmlDocument();
                        docInner.Load($"https://dadosabertos.almg.gov.br/ws/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/{dataReferencia.Year}/{dataReferencia.Month}");
                        var despesas = docInner.DocumentElement.SelectNodes("resumoVerba/listaDetalheVerba/detalheVerba");

                        var sqlFields = new StringBuilder();
                        var sqlValues = new StringBuilder();

                        foreach (XmlNode despesa in despesas)
                        {
                            var tipoDespesa = despesa.SelectSingleNode("descTipoDespesa").InnerText;
                            var dataEmissao = Convert.ToDateTime(despesa.SelectSingleNode("dataEmissao").InnerText);
                            var cnpj = despesa.SelectSingleNode("cpfCnpj")?.InnerText;
                            var fornecedor = despesa.SelectSingleNode("nomeEmitente").InnerText;
                            var valor = Convert.ToDecimal(despesa.SelectSingleNode("valorReembolsado").InnerText, cultureInfo);
                            var documento = despesa.SelectSingleNode("descDocumento")?.InnerText;

                            db.AddParameter("Nome", item["nome_parlamentar"]);
                            db.AddParameter("CPF", matricula);
                            db.AddParameter("Empresa", fornecedor);
                            db.AddParameter("CNPJ_CPF", cnpj);
                            db.AddParameter("DataEmissao", dataEmissao);
                            db.AddParameter("Valor", valor);
                            db.AddParameter("TipoDespesa", tipoDespesa);
                            db.AddParameter("documento", documento);
                            db.AddParameter("Ano", ano);

                            byte[] hash = db.ParametersHash();
                            var key = Convert.ToHexString(hash);
                            if (lstHash.Remove(key))
                            {
                                db.ClearParameters();
                                continue;
                            }

                            db.AddParameter("hash", hash);

                            db.ExecuteNonQuery(
                                @"INSERT INTO ops_tmp.cl_despesa_temp (
								nome, cpf, empresa, cnpj_cpf, data_emissao, valor, despesa_tipo, ano, documento, hash
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Valor, @TipoDespesa, @Ano, @documento, @hash
							)");

                        }
                    }

                    foreach (var id in lstHash.Values)
                    {
                        db.AddParameter("id", id);
                        db.ExecuteNonQuery("delete from cf_despesa where id=@id");
                    }
                }
            }
        }
    }
}
