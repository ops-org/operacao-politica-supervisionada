using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Bahia
{
    public class ImportadorDespesasBahia : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasBahia(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.ba.gov.br/",
                Estado = Estado.Bahia,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano)
        {
            var pagina = 0;
            var despesasReferencia = new List<string>();
            while (true)
            {
                logger.LogDebug("Consultando Ano {Ano} e pagina {Pagina}!", ano, pagina);
                // Pagina começa em 0 - HUE
                var address = $"{config.BaseAddress}transparencia/verbas-idenizatorias?ano={ano}&categoria=&page={pagina++}&size=1000";
                var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
                var despesas = document.QuerySelectorAll(".tabela-cab tbody tr");
                foreach (var despesa in despesas)
                {
                    // N° PROCESSO	N° NF	MÊS/ANO	DEPUTADO (A)	CATEGORIA	VALOR (R$)
                    var colunas = despesa.QuerySelectorAll("td");
                    if (!colunas.Any()) continue;

                    var elProcesso = colunas[0].QuerySelector("span") ?? colunas[0];
                    var processo = elProcesso.TextContent.Trim();
                    if (despesasReferencia.Contains(processo)) continue;
                    despesasReferencia.Add(processo);

                    string linkDetalhes = ((colunas[0].QuerySelector("a") ?? colunas[6].QuerySelector("a")) as IHtmlAnchorElement).Href;
                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Ano = (short)ano,
                        //Documento = colunas[0].QuerySelector("span").TextContent.Trim() + "/" + colunas[1].TextContent.Trim(),
                        DataEmissao = Convert.ToDateTime("01/" + colunas[2].TextContent.Trim(), cultureInfo),
                        Nome = colunas[3].TextContent.Trim().ToTitleCase(),
                        //TipoDespesa = colunas[4].TextContent.Trim()
                    };

                    using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano, ["Mes"] = despesaTemp.DataEmissao.Month, ["Parlamentar"] = despesaTemp.Nome }))
                    {
                        try
                        {
                            var documentDetalhes = context.OpenAsyncAutoRetry(linkDetalhes).GetAwaiter().GetResult();
                            var despesasDetalhes = documentDetalhes.QuerySelectorAll(".tabela-cab tbody tr");
                            foreach (var detalhes in despesasDetalhes)
                            {
                                // CATEGORIA	Nº NOTA/RECIBO	CPF/CNPJ	NOME DO FORNECEDOR	VALOR ANEXO NF
                                var colunasDetalhes = detalhes.QuerySelectorAll("td");

                                despesaTemp.TipoDespesa = colunasDetalhes[0].TextContent.Trim();
                                despesaTemp.Documento = processo + "/" + colunasDetalhes[1].TextContent.Trim();
                                despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(colunasDetalhes[2].TextContent.Trim());
                                despesaTemp.Empresa = colunasDetalhes[3].TextContent.Trim();
                                despesaTemp.Valor = Convert.ToDecimal(colunasDetalhes[4].TextContent.Replace("R$", "").Trim(), cultureInfo);

                                if (colunasDetalhes[5].Children.Any())
                                    despesaTemp.Observacao = (colunasDetalhes[5].Children[0] as IHtmlAnchorElement).Href;
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO? Revisar
                            using (var reader = connection.ExecuteReader($"SELECT distinct hash FROM cl_despesa WHERE numero_documento LIKE '{processo}/%'"))
                            {
                                while (reader.Read())
                                {
                                    var key = Convert.ToHexString((byte[])reader["hash"]);
                                    lstHash.Remove(key);
                                }
                            }

                            logger.LogCritical(ex, ex.Message);
                            continue;
                        }
                    }

                    InserirDespesaTemp(despesaTemp);
                }

                if (document.QuerySelector(".paginate-button-next")?.ClassList.Contains("disabled") ?? true) break;
            }
        }
    }
}
