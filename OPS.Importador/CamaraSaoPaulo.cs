using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    public class CamaraSaoPaulo : ImportadorCotaParlamentarBase
    {
        public CamaraSaoPaulo(ILogger<CamaraSaoPaulo> logger, IConfiguration configuration, IDbConnection connection)
            : base("SP", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var doc = new XmlDocument();
            doc.Load(@"https://www.al.sp.gov.br/repositorioDados/deputados/deputados.xml");
            XmlNode deputados = doc.DocumentElement;

            var deputadoXml = deputados.SelectNodes("Deputado");
            var sqlFields = new StringBuilder();
            var sqlValues = new StringBuilder();

            using (var db = new AppDb())
            {
                foreach (XmlNode fileNode in deputadoXml)
                {
                    var deputado = new DeputadoEstadual();
                    deputado.IdEstado = (ushort)idEstado;

                    deputado.NomeParlamentar = fileNode.SelectSingleNode("NomeParlamentar").InnerText.Trim();
                    var partido = fileNode.SelectSingleNode("Partido").InnerText.Trim();

                    deputado.Matricula = Convert.ToUInt32(fileNode.SelectSingleNode("Matricula").InnerText.Trim());
                    deputado.Telefone = fileNode.SelectSingleNode("Telefone")?.InnerText.Trim();
                    deputado.Email = fileNode.SelectSingleNode("Email")?.InnerText?.Trim();
                    //var idDeputado = fileNode.SelectSingleNode("IdDeputado").InnerText.Trim();
                    //var gabinete = fileNode.SelectSingleNode("Sala").InnerText.Trim();
                    deputado.UrlFoto = fileNode.SelectSingleNode("PathFoto").InnerText.Trim();

                    deputado.UrlPerfil = $"https://www.al.sp.gov.br/deputado/?matricula={deputado.Matricula}";

                    var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA") }).FirstOrDefault()?.Id;
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

        /// <summary>
        /// Dados a partir de 2013
        /// </summary>
        /// <param name="ano"></param>
        /// <returns></returns>
        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();
            string urlOrigem, caminhoArquivo;

            urlOrigem = string.Format("https://www.al.sp.gov.br/repositorioDados/deputados/despesas_gabinetes_{0}.xml", ano);
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
                        Ano = (short)ano,
                        Cpf = matricula,
                        Empresa = fornecedor,
                        CnpjCpf = cnpj,
                        TipoDespesa = tipoDespesa,
                        Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor, cultureInfo) : 0,
                        DataEmissao = new DateTime(ano, Convert.ToInt32(mes), 1)
                    };

                    if (RegistroExistente(objCamaraEstadualDespesaTemp, lstHash))
                        continue;

                    connection.Insert(objCamaraEstadualDespesaTemp);
                }
            }
        }

    }
}
