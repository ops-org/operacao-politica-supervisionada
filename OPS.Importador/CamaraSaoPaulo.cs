using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
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

                var dc = new Dictionary<string, UInt32>();
                using (var dReader = banco.ExecuteReader(
                    $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano}12"))
                    while (dReader.Read())
                    {
                        var hex = Convert.ToHexString((byte[])dReader["hash"]);
                        if (!dc.ContainsKey(hex))
                            dc.Add(hex, (UInt32)dReader["id"]);
                    }

                CarregaDados(banco, caminhoArquivo, ano, dc);

                if (!completo && dc.Values.Any())
                {
                    logger.LogInformation("{Total} despesas removidas!", dc.Values.Count);

                    foreach (var id in dc.Values)
                    {
                        banco.AddParameter("id", id);
                        banco.ExecuteNonQuery("delete from cf_despesa where id=@id");
                    }
                }

                AjustarDados();
                InsereTipoDespesaFaltante();
                InsereDeputadoFaltante();
                InsereFornecedorFaltante();
                InsereDespesaFinal();
                LimpaDespesaTemporaria();

                if (ano == DateTime.Now.Year)
                {
                    //AtualizaCampeoesGastos(banco);
                    //AtualizaResumoMensal(banco);
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

                    banco.AddParameter("Nome", deputado);
                    banco.AddParameter("CPF", matricula);
                    banco.AddParameter("Empresa", fornecedor);
                    banco.AddParameter("CNPJ_CPF", cnpj);
                    banco.AddParameter("DataEmissao", new DateTime(ano, Convert.ToInt32(mes), 1));
                    banco.AddParameter("Valor", !string.IsNullOrEmpty(valor) ? (object)Convert.ToDouble(valor, cultureInfo) : 0);
                    banco.AddParameter("TipoDespesa", tipoDespesa);
                    banco.AddParameter("Ano", ano);

                    byte[] hash = banco.ParametersHash();
                    var key = Convert.ToHexString(hash);
                    if (lstHash.Remove(key))
                    {
                        banco.ClearParameters();
                        continue;
                    }

                    banco.AddParameter("hash", hash);

                    banco.ExecuteNonQuery(
                        @"INSERT INTO ops_tmp.cl_despesa_temp (
								nome, cpf, empresa, cnpj_cpf, data_emissao, valor, despesa_tipo, ano, hash
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Valor, @TipoDespesa, @Ano, @hash
							)");

                }
            }
        }

    }
}
