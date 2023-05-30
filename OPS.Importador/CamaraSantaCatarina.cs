using AngleSharp;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa do Estado de Santa Catarina
    /// https://transparencia.alesc.sc.gov.br/
    /// </summary>
    public class CamaraSantaCatarina : ImportadorCotaParlamentarBase
    {
        public CamaraSantaCatarina(ILogger<CamaraSantaCatarina> logger, IConfiguration configuration, IDbConnection connection) :
            base("SC", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            using (var db = new AppDb())
            {
                var address = $"https://www.alesc.sc.gov.br/todos-deputados";
                var task = context.OpenAsync(address);
                task.Wait();
                var document = task.Result;
                if (document.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{address} {document.StatusCode}");
                };

                var parlamentares = document.QuerySelectorAll("table.views-table tbody tr");
                foreach (var linha in parlamentares)
                {
                    var itens = linha.QuerySelectorAll("td");
                    var deputado = new DeputadoEstadual();
                    deputado.IdEstado = (ushort)idEstado;

                    var colunaNome = itens[0].QuerySelector("a");
                    deputado.UrlPerfil = (colunaNome as IHtmlAnchorElement).Href;
                    deputado.NomeParlamentar = colunaNome.TextContent.Trim();
                    var partido = itens[1].TextContent.Trim();
                    deputado.Email = itens[2].QuerySelector("a").TextContent.Trim();

                    var taskSub = context.OpenAsync(deputado.UrlPerfil);
                    taskSub.Wait();
                    var subDocument = taskSub.Result;
                    if (document.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"{deputado.UrlPerfil} {subDocument.StatusCode}");
                        continue;
                    };

                    var detalhes = subDocument.QuerySelectorAll(".deputado-resumo");
                    deputado.Nascimento = DateOnly.FromDateTime(DateTime.Parse(detalhes[0].QuerySelector("span.date-display-single").Attributes["content"].Value));
                    deputado.Escolaridade = detalhes[1].TextContent.Replace("Escolaridade:", "").Trim();
                    deputado.Naturalidade = detalhes[2].TextContent.Replace("Origem:", "").Split('/')[0].Trim();
                    //var gabinete = detalhes[3].TextContent.Trim();
                    deputado.Telefone = detalhes[4].TextContent.Replace("Contatos:", "").Trim();

                    var IdPartido = connection.GetList<Core.Entity.Partido>(new { Sigla = partido }).FirstOrDefault()?.Id;
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


        //public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        //{
        //    Dictionary<string, string> arquivos = new();

        //https://transparencia.alesc.sc.gov.br/pagamentos_dados_abertos.php
        //    Arquivos disponiveis anualmente a partir de 2010
        //    var _urlOrigem = $"https://transparencia.alesc.sc.gov.br/pagamentos_csv.php?ano={ano}";
        //    var _caminhoArquivo = $"{tempPath}/CLSC-{ano}.csv";

        //    arquivos.Add(_urlOrigem, _caminhoArquivo);
        //    return arquivos;
        //}

        //protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        //{
        //    var cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR");

        //    CPF / CNPJ   CREDOR EMPENHO_NUMERO  EMPENHO_MODALIDADE EMPENHO_UG  EMPENHO_SUBFUNCAO EMPENHO_PROGRAMA    EMPENHO_ACAO EMPENHO_SUBACAO EMPENHO_FONTE_RECURSO EMPENHO_NATUREZA    SUBELEMENTO EMPENHO_CDCOMPLEMENTO   EMPENHO_COMPLEMENTO EMPENHO_REFERENCIA  EMPENHO_VALOR LIQUIDACAO_NUMERO   LIQUIDACAO_EMISSAO LIQUIDACAO_DOCUMENTO    LIQUIDACAO_VALOR PAGAMENTO_NUMERO    PAGAMENTO_OB PAGAMENTO_PAGAMENTO PAGAMENTO_RENTECAO PAGAMENTO_VALOR_PAGO
        //    int CnpjCpfCredor = 0; // CPF/CNPJ
        //    int NomeCredor = 1; // CREDOR
        //    int NumeroEmpenho = 2; // EMPENHO_NUMERO
        //    int ModalidadeEmpenho = 3; // EMPENHO_MODALIDADE
        //    int Verba = 12; // SUBELEMENTO
        //    int CodigoFavorecido = 13; // EMPENHO_CDCOMPLEMENTO
        //    int NomeFavorecido = 14; // EMPENHO_COMPLEMENTO
        //    int DataReferencia = 15; // EMPENHO_REFERENCIA
        //    int DataEmissao = 18; // LIQUIDACAO_EMISSAO
        //    int Documento = 19; // LIQUIDACAO_DOCUMENTO
        //    int Valor = 20; // LIQUIDACAO_VALOR

        //    using (var banco = new AppDb())
        //    {
        //        var dc = new Dictionary<string, UInt32>();
        //        using (var dReader = banco.ExecuteReader(
        //            $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano}12"))
        //            while (dReader.Read())
        //            {
        //                var hex = Convert.ToHexString((byte[])dReader["hash"]);
        //                if (!dc.ContainsKey(hex))
        //                    dc.Add(hex, (UInt32)dReader["id"]);
        //            }

        //        using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
        //        {
        //            short count = 0;

        //            using (var csv = new CsvReader(reader, cultureInfo))
        //            {

        //                while (csv.Read())
        //                {
        //                    count++;

        //                    if (count == 1)
        //                    {

        //                        if (
        //                            (csv[CnpjCpfCredor] != "CPF/CNPJ") ||
        //                            (csv[NomeCredor] != "CREDOR") ||
        //                            (csv[NumeroEmpenho] != "EMPENHO_NUMERO") ||
        //                            (csv[Verba] != "SUBELEMENTO") ||
        //                            (csv[CodigoFavorecido] != "EMPENHO_CDCOMPLEMENTO") ||
        //                            (csv[NomeFavorecido] != "EMPENHO_COMPLEMENTO") ||
        //                            (csv[DataEmissao] != "LIQUIDACAO_EMISSAO") ||
        //                            (csv[Documento] != "LIQUIDACAO_DOCUMENTO") ||
        //                            (csv[Valor] != "LIQUIDACAO_VALOR")
        //                        )
        //                        {
        //                            throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
        //                        }

        //                        Pular linha de titulo
        //                        continue;
        //                    }

        //                    if (string.IsNullOrEmpty(csv[Verba])) continue; //Linha vazia

        //                    DateTime dataEmissao;
        //                    try
        //                    {
        //                        dataEmissao = DateTime.Parse(csv[DataEmissao]);
        //                    }
        //                    catch (Exception)
        //                    {
        //                        dataEmissao = DateTime.Parse(csv[DataReferencia]);
        //                    }

        //                    var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
        //                    {
        //                        Nome = csv[NomeFavorecido],
        //                        Cpf = csv[CodigoFavorecido],
        //                        Ano = (short)ano,
        //                        TipoDespesa = csv[Verba],
        //                        Valor = Convert.ToDecimal(csv[Valor], cultureInfo),
        //                        DataEmissao = dataEmissao,
        //                        CnpjCpf = Utils.RemoveCaracteresNaoNumericos(csv[CnpjCpfCredor]),
        //                        Empresa = csv[NomeCredor],
        //                        Documento = csv[Documento],
        //                        Observacao = $"Empenho: {csv[NumeroEmpenho]} ({csv[ModalidadeEmpenho]})"
        //                    };

        //                    connection.Insert(objCamaraEstadualDespesaTemp);
        //                    byte[] hash = banco.ParametersHash();
        //                    var key = Convert.ToHexString(hash);
        //                }
        //            }
        //        }

        //        if (!completo && dc.Values.Any())
        //        {
        //            logger.LogInformation("{Total} despesas removidas!", dc.Values.Count);

        //            foreach (var id in dc.Values)
        //            {
        //                banco.AddParameter("id", id);
        //                banco.ExecuteNonQuery("delete from cf_despesa where id=@id");
        //            }
        //        }

        //        ProcessarDespesasTemp();

        //        if (ano == DateTime.Now.Year)
        //        {
        //            AtualizaParlamentarValores();
        //        }
        //    }
        //}

        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();

            // https://transparencia.alesc.sc.gov.br/gabinetes_dados_abertos.php
            // Arquivos disponiveis anualmente a partir de 2011
            var _urlOrigem = $"http://transparencia.alesc.sc.gov.br/gabinetes_csv.php?ano={ano}";
            var _caminhoArquivo = $"{tempPath}/CLSC-{ano}.csv";

            arquivos.Add(_urlOrigem, _caminhoArquivo);
            return arquivos;
        }

        protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        {
            var cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR");

            int indice = 0;
            int Verba = indice++;
            int Descricao = indice++;
            int Conta = indice++;
            int Favorecido = indice++;
            int Trecho = indice++;
            int Vencimento = indice++;
            int Valor = indice++;

            LimpaDespesaTemporaria();
            Dictionary<string, uint> lstHash = ObterHashes(ano);

            using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
            {
                short count = 0;

                using (var csv = new CsvReader(reader, cultureInfo))
                {

                    while (csv.Read())
                    {
                        count++;

                        if (count == 1)
                        {

                            if (
                                (csv[Verba] != "Verba") ||
                                (csv[Descricao] != "Descrição") ||
                                (csv[Conta] != "Conta") ||
                                (csv[Favorecido] != "Favorecido") ||
                                (csv[Trecho] != "Trecho") ||
                                (csv[Vencimento] != "Vencimento") ||
                                (csv[Valor] != "Valor")
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        if (string.IsNullOrEmpty(csv[Verba])) continue; //Linha vazia


                        var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = csv[Conta],
                            Ano = (short)ano,
                            TipoVerba = csv[Verba],
                            TipoDespesa = csv[Descricao],
                            Valor = Convert.ToDecimal(csv[Valor], cultureInfo),
                            DataEmissao = DateTime.Parse(csv[Vencimento]),
                            Favorecido = csv[Favorecido],
                            Observacao = csv[Trecho]
                        };

                        if (RegistroExistente(objCamaraEstadualDespesaTemp, lstHash))
                            continue;

                        connection.Insert(objCamaraEstadualDespesaTemp);
                    }
                }
            }

            SincronizarHashes(lstHash);
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

        public override void AtualizaParlamentarValores()
        {
            connection.Execute(@"
        	        UPDATE cl_deputado dp SET
                        valor_total_ceap = IFNULL((
                            SELECT SUM(ds.valor_liquido) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
                        ), 0);");
        }

        private void ProcessarDespesasTemp()
        {

            connection.Execute(@"
update ops_tmp.cl_despesa_temp set nome = 'VICENTE AUGUSTO CAROPRESO' where nome like 'Vicente Caropreso';
update ops_tmp.cl_despesa_temp set nome = 'Ana Paula da Silva' where nome like 'Ana Paula da Silva (Paulinha)';
update ops_tmp.cl_despesa_temp set nome = 'CARLOS HENRIQUE DE LIMA' where nome like 'Carlos Henrique Lima';
update ops_tmp.cl_despesa_temp set nome = 'LAÉRCIO DEMERVAL SCHUSTER JUNIOR' where nome like 'Laércio D. S. Juinior';");

            InsereTipoDespesaFaltante();
            InsereDeputadoFaltante();
            //InsereFornecedorFaltante();
            InsereDespesaFinal();
            LimpaDespesaTemporaria();
        }


        public override void InsereFornecedorFaltante()
        {
            // Faz nada.
        }

        public override void InsereDeputadoFaltante()
        {
            var affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, matricula, id_estado)
select distinct Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where nome not in (
    select nome_parlamentar from cl_deputado where id_estado =  {idEstado} 
    UNION all
    select nome_civil from cl_deputado where id_estado =  {idEstado} 
);
                ");

            if (affected > 0)
            {
                logger.LogInformation("{Itens} parlamentares incluidos!", affected);


            }
        }

        public override void InsereDespesaFinal()
        {
            var affected = connection.Execute(@$"
INSERT INTO cl_despesa (
	id_cl_deputado,
    id_cl_despesa_tipo,
    id_cl_despesa_especificacao,
	id_fornecedor,
	data_emissao,
	ano_mes,
	numero_documento,
	valor_liquido,
    favorecido,
    observacao,
    hash
)
SELECT 
	p.id AS id_cl_deputado,
    dts.id_cl_despesa_tipo,
    dts.id,
    f.id AS id_fornecedor,
    d.data_emissao,
    concat(year(d.data_emissao), LPAD(month(d.data_emissao), 2, '0')) AS ano_mes,
    d.documento AS numero_documento,
    d.valor AS valor,
    d.favorecido,
    d.observacao AS observacao,
    d.hash
FROM ops_tmp.cl_despesa_temp d
inner join cl_deputado p on (p.nome_parlamentar like d.nome or p.nome_civil like d.nome) and id_estado = {idEstado}
left join cl_despesa_especificacao dts on dts.descricao = d.despesa_tipo
LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
ORDER BY d.id;
			", 3600);

            connection.Execute(@"
UPDATE cl_despesa SET id_fornecedor = 89481, favorecido = null WHERE id_fornecedor IS NULL AND favorecido LIKE 'Brasil Telecom%';
UPDATE cl_despesa SET id_fornecedor = 458, favorecido = null WHERE id_fornecedor IS NULL AND (favorecido LIKE 'Oi S%' OR favorecido LIKE 'Oi Fixo%' OR favorecido LIKE 'Oi');
UPDATE cl_despesa SET id_fornecedor = 301, favorecido = null WHERE id_fornecedor IS NULL AND (favorecido LIKE 'Global Village Telecom%' OR favorecido LIKE 'GVT');
UPDATE cl_despesa SET id_fornecedor = 8688, favorecido = null WHERE id_fornecedor IS NULL AND favorecido LIKE 'Claro';
UPDATE cl_despesa SET id_fornecedor = 4163, favorecido = null WHERE id_fornecedor IS NULL AND favorecido LIKE 'NET';
UPDATE cl_despesa SET id_fornecedor = 19411 WHERE id_fornecedor IS NULL AND id_cl_despesa_especificacao = 21; -- Restaurante da AFALESC
UPDATE cl_despesa SET id_fornecedor = 663 WHERE id_fornecedor IS NULL AND id_cl_despesa_especificacao IN(28, 58); -- Energia Elétrica ( Escritório de Apoio ) & Escritório de Apoio - Energia Elétrica
UPDATE cl_despesa SET id_fornecedor = 1316 WHERE id_fornecedor IS NULL and id_cl_despesa_especificacao IN(24, 56); -- Água (Escritório de Apoio) & Escritório de Apoio - Água
UPDATE cl_despesa SET id_fornecedor = 1163 WHERE id_fornecedor IS NULL and id_cl_despesa_especificacao = 52; -- Assinatura de TV a Cabo
UPDATE cl_despesa SET id_fornecedor = 42634 WHERE id_fornecedor IS NULL and id_cl_despesa_especificacao = 6; -- Correspondência / Telegrama
UPDATE cl_despesa SET id_fornecedor = 47839 WHERE id_fornecedor IS NULL and id_cl_despesa_especificacao = 63; -- Locação de Veículo (Contrato)
");

            if (affected > 0)
            {
                logger.LogInformation("{Itens} despesas incluidas!", affected);
            }
        }

    }
}
