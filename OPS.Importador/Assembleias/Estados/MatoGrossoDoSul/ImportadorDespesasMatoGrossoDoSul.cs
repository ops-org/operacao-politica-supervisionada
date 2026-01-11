using System.Globalization;
using System.Net;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Estados.MatoGrossoDoSul.Entities;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.MatoGrossoDoSul;

public class ImportadorDespesasMatoGrossoDoSul : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasMatoGrossoDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "http://consulta.transparencia.al.ms.gov.br/ceap/",
            Estado = Estado.MatoGrossoDoSul,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    /// <summary>
    /// Dados a partir de 2011
    /// http://consulta.transparencia.al.ms.gov.br/ceap/
    /// </summary>
    /// <param name="ano"></param>
    /// <returns></returns>
    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        var pagina = 0;

        var address = $"http://consulta.transparencia.al.ms.gov.br/ceap/";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

        Thread.Sleep(TimeSpan.FromSeconds(3));
        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");

        var dcForm = new Dictionary<string, string>();
        dcForm.Add("script_case_init", (form.Elements["script_case_init"] as IHtmlInputElement).Value);
        dcForm.Add("nmgp_opcao", "busca");
        dcForm.Add("deputados_nome_cond", "qp");
        dcForm.Add("deputados_nome", "");
        dcForm.Add("categoriadespesas_descricao_cond", "qp");
        dcForm.Add("categoriadespesas_descricao", "");
        dcForm.Add("verbaindenizatoria_mes_referencia_cond", "qp");
        dcForm.Add("verbaindenizatoria_mes_referencia", "");
        dcForm.Add("verbaindenizatoria_ano_referencia_cond", "bw");
        dcForm.Add("verbaindenizatoria_ano_referencia", ano.ToString());
        dcForm.Add("verbaindenizatoria_ano_referencia_autocomp", ano.ToString());
        dcForm.Add("verbaindenizatoria_ano_referencia_input_2", ano.ToString());
        dcForm.Add("NM_operador", "and");
        dcForm.Add("nmgp_tab_label", "deputados_nome%3F%23%3FNome%3F%40%3Fcategoriadespesas_descricao%3F%23%3FCategoria%2FDespesa%3F%40%3Fverbaindenizatoria_mes_referencia%3F%23%3FM%EAs%3F%40%3Fverbaindenizatoria_ano_referencia%3F%23%3FAno%3F%40%3F");
        dcForm.Add("bprocessa", "pesq");
        dcForm.Add("nmgp_save_name_bot", "");
        dcForm.Add("NM_filters_del_bot", "");
        dcForm.Add("form_condicao", "3");
        document = form.SubmitAsyncAutoRetry(dcForm, true).GetAwaiter().GetResult();

        form = document.QuerySelector<IHtmlFormElement>("form");
        document = form.SubmitAsync().GetAwaiter().GetResult();

        if (document.QuerySelector("#sc_grid_body").TextContent.Trim() == "Registros não encontrados") return;

        var ultimaPaginaTemp = document.QuerySelector("#last_bot").NextElementSibling.TextContent; // [101 a 200 de 15495]
        var ultimaPagina = Convert.ToInt32(ultimaPaginaTemp.Split(" ")[4].Split("]")[0]);

        string nomeParlamentar = "";
        string tipoDespesa = "";
        short anoDespesa = 0;
        short mesDespesa = 0;
        decimal valorTotalControleMensal = 0;
        decimal valorTotalControleGeral = 0;
        var despesasDeputado = 0;

        while (true)
        {
            //var paginaAtual = document.QuerySelector(".scGridToolbarNavOpen").TextContent;
            logger.LogDebug("Consultando pagina {Pagina}!", ++pagina);

            var despesas = document.QuerySelector("#sc_grid_body");
            if (despesas.TextContent.Trim() == "Registros não encontrados")
            {
                logger.LogWarning("Registros indisponiveis! Detalhes: {Mensagem}", despesas.TextContent);
            }

            var linhas = document.QuerySelectorAll("#sc_grid_body>table.scGridTabela>tbody");

            foreach (var linha in linhas)
            {
                if (linha.Id is null || !linha.Id.StartsWith("tbody_search_ceap_")) continue;

                var elementoTitulo = linha.QuerySelectorAll(".scGridBlockFont td");
                string titulo = null;
                if (elementoTitulo.Any())
                    titulo = elementoTitulo[0].TextContent;

                switch (titulo)
                {
                    case "Nome":
                        var nomeTemp = linha.QuerySelectorAll(".scGridBlockFont td")[2].TextContent.Replace("Dep.", "").Trim().ToTitleCase();
                        if (nomeTemp != nomeParlamentar)
                        {
                            nomeParlamentar = nomeTemp;
                            //logger.LogInformation($"Deputado {nomeParlamentar}");
                            valorTotalControleGeral = 0;
                            valorTotalControleMensal = 0;
                            despesasDeputado = 0;
                        }
                        break;

                    case "Ano":
                        anoDespesa = Convert.ToInt16(linha.QuerySelectorAll(".scGridBlockFont td")[2].TextContent.Trim());
                        //logger.LogInformation($"Ano {anoDespesa}");
                        break;

                    case "Mês":
                        var mesTemp = Convert.ToInt16(linha.QuerySelectorAll(".scGridBlockFont td")[2].TextContent.Trim());
                        if (mesTemp != mesDespesa)
                        {
                            mesDespesa = mesTemp;
                            //logger.LogInformation($"Mês {mesDespesa}");
                            valorTotalControleMensal = 0;
                        }
                        break;

                    default: // Despesas

                        var despesasTipo = linha.QuerySelectorAll(".css_categoriadespesas_descricao_grid_line");
                        var despesasTabelas = linha.QuerySelectorAll(".scGridTabela[id^=sc-ui-grid-body]");

                        for (int i = 0; i < despesasTipo.Length; i++)
                        {
                            tipoDespesa = despesasTipo[i].TextContent.Trim();
                            var linhaDetelhes = despesasTabelas[i].QuerySelectorAll("tr");

                            for (int j = 0; j < linhaDetelhes.Length; j++)
                            {
                                if (linhaDetelhes[j].ClassList.Contains("scGridTotal") || linhaDetelhes[j].ClassList.Contains("sc-ui-grid-header-row")) continue;

                                var detalhes = linhaDetelhes[j].QuerySelectorAll("td");

                                var despesaTemp = new CamaraEstadualDespesaTemp();
                                despesaTemp.Nome = nomeParlamentar;
                                despesaTemp.Ano = anoDespesa;
                                despesaTemp.Mes = mesDespesa;
                                despesaTemp.TipoDespesa = tipoDespesa;
                                despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(detalhes[1].TextContent.Trim());
                                despesaTemp.Empresa = detalhes[2].TextContent.Trim();
                                despesaTemp.Documento = detalhes[3].TextContent.Trim();
                                despesaTemp.DataEmissao = Convert.ToDateTime(detalhes[4].TextContent.Trim(), cultureInfo);
                                despesaTemp.Valor = Convert.ToDecimal(detalhes[5].TextContent.Replace("R$", "").Trim(), cultureInfo);
                                despesaTemp.Observacao = (detalhes[6].QuerySelector("a") as IHtmlAnchorElement)?.Href?.Replace("http://www.transparencia.al.ms.gov.br/arquivo/", "");

                                InserirDespesaTemp(despesaTemp);
                                valorTotalControleMensal += despesaTemp.Valor;
                                valorTotalControleGeral += despesaTemp.Valor;
                                despesasDeputado++;
                            }
                        }

                        if (linha.QuerySelector(".scGridSubtotal") != null)
                        {
                            var valorTotalDeputado = Convert.ToDecimal(linha.QuerySelector(".scGridSubtotal .css_total_S_sub_tot").TextContent.Replace("R$", "").Trim(), cultureInfo);

                            if (linha.QuerySelector(".scGridSubtotal").ParentElement.ChildElementCount > 1)
                            {
                                // Validar valor total mensal.
                                if (valorTotalControleMensal != valorTotalDeputado)
                                    logger.LogError("Valor Divergente Mensal! Parlamentar {Parlamentar}! Valor Encontrado: {Encontrado}; esperado: {Esperado}", nomeParlamentar, valorTotalControleMensal, valorTotalDeputado);
                            }
                            else
                            {
                                // Validar valor total geral. OBS: Existem linhas orfãs que na verdade são somatorias mensais.
                                if (valorTotalControleGeral != valorTotalDeputado && valorTotalControleMensal != valorTotalDeputado)
                                    logger.LogError("Valor Divergente Geral! Parlamentar {Parlamentar}! Valor Encontrado: {Encontrado}; esperado: {Esperado}", nomeParlamentar, valorTotalControleGeral, valorTotalDeputado);
                            }
                        }

                        //logger.LogInformation($"Importando {despesasDeputado} despesas do Parlamentar {nomeParlamentar}");

                        //controle = 0;
                        break;
                }
            }


            //if (document.QuerySelector("#forward_bot")?.ClassList.Contains("disabled") ?? true) break;
            //else
            //{
            int offset = pagina * 100 + 1;
            if (offset > ultimaPagina) break;

            var dcFormPaginacao = new Dictionary<string, string>();
            dcFormPaginacao.Add("script_case_init", (form.Elements["script_case_init"] as IHtmlInputElement).Value);
            dcFormPaginacao.Add("nmgp_opcao", "ajax_navigate");
            dcFormPaginacao.Add("opc", "rec");
            dcFormPaginacao.Add("parm", offset.ToString());
            document = form.SubmitAsyncAutoRetry(dcFormPaginacao, true).GetAwaiter().GetResult();
            var htmlZoado = document.Body.InnerHtml.Replace("</script></a></td></tr></tbody></table>", "");
            var json = "{\"setValue\":[{\"field\": \"sc_grid_body" + htmlZoado.Split("sc_grid_body")[1];
            Rootobject parsed = JsonSerializer.Deserialize<Rootobject>(json);
            var html = WebUtility.HtmlDecode(parsed.setValue.FirstOrDefault(x => x.field == "sc_grid_body").value);

            document = context.OpenAsync(req => req.Content("<div id='sc_grid_body'>" + html + "</div>")).GetAwaiter().GetResult();
            //logger.LogDebug("Consultando pagina {Pagina}!", ++pagina);

            //}
        }
    }

    //    public override void InsereDeputadoFaltante()
    //    {
    //        int affected = connection.Execute(@$"
    //INSERT INTO cl_deputado (nome_parlamentar, matricula, id_estado)
    //select distinct Nome, cpf, {idEstado}
    //from temp.cl_despesa_temp
    //where nome not in (
    //    select nome_parlamentar 
    //    FROM assembleias.cl_deputado 
    //    WHERE id_estado = {idEstado}
    //);
    //                ");

    //        if (affected > 0)
    //        {
    //            logger.LogInformation("{Itens} parlamentares incluidos!", affected);
    //        }
    //    }

    public override void InsereDespesaFinal(int ano)
    {
        var affected = connection.Execute(@$"
INSERT IGNORE INTO cl_despesa (
	id_cl_deputado,
    id_cl_despesa_tipo,
    id_cl_despesa_especificacao,
	id_fornecedor,
	data_emissao,
	ano_mes,
	numero_documento,
	valor_liquido,
    favorecido,
    hash
)
SELECT 
	p.id AS id_cl_deputado,
    dts.id_cl_despesa_tipo,
    dts.id,
    f.id AS id_fornecedor,
    d.data_emissao,
    concat(d.ano, LPAD(d.mes, 2, '0')) AS ano_mes,
    d.documento AS numero_documento,
    d.valor AS valor,
    CASE WHEN f.id IS NULL THEN d.empresa else null END AS observacao,
    d.hash
FROM temp.cl_despesa_temp d
inner join cl_deputado p on id_estado = {idEstado} and p.nome_parlamentar = d.nome
left join cl_despesa_especificacao dts on dts.descricao = d.despesa_tipo
LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
ORDER BY d.id;
			", 3600);

        if (affected > 0)
        {
            logger.LogInformation("{Itens} despesas incluidas!", affected);
        }

    }
}
