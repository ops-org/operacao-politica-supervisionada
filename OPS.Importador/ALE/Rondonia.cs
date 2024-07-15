using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using RestSharp;
namespace OPS.Importador.ALE;

public class Rondonia : ImportadorBase
{
    public Rondonia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarRondonia(serviceProvider);
        importadorDespesas = new ImportadorDespesasRondonia(serviceProvider);
    }
}

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
            Estado = Estado.Rondonia,
            ChaveImportacao = ChaveDespesaTemp.Gabinete
        };

        // TODO: Filtrar legislatura atual
        deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        ImportarCotaParlamentar(context, ano, mes);
        ImportarDiarias(context, ano, mes);
    }

    private void ImportarDiarias(IBrowsingContext context, int ano, int mes)
    {
        var address = $"https://transparencia.al.ro.leg.br/Deputados/Diarias/?nome=&ano={ano}&mes={mes}";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

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

            var deputado = deputados.Find(x => (x.NomeImportacao ?? Utils.RemoveAccents(x.NomeCivil)).Equals(nomeDeputado, StringComparison.InvariantCultureIgnoreCase));
            if(deputado == null || deputado.Gabinete == null)
            {
                logger.LogError($"Deputado {colunas[idxCredor].TextContent} não existe ou não possui gabinete relacionado!");
            }

            var despesaTemp = new CamaraEstadualDespesaTemp()
            {
                Nome = colunas[idxCredor].TextContent,
                Cpf = deputado?.Gabinete.ToString(),
                Ano = (short)ano,
                Mes = (short)mes,
                TipoDespesa = "Diárias",
                DataEmissao = new DateTime(ano, mes, 1),
                Valor = Convert.ToDecimal(colunas[idxValor].TextContent, cultureInfo),
                Observacao = $"Diárias: {colunas[idxQuantidade].TextContent}; Trecho: {colunas[idxDestino].TextContent}; Transporte: {colunas[idxMeioTransporte].TextContent}; Link: {link}",
            };


            InserirDespesaTemp(despesaTemp);
        }
    }

    private void ImportarCotaParlamentar(IBrowsingContext context, int ano, int mes)
    {
        var address = $"https://transparencia.al.ro.leg.br/Deputados/VerbaIndenizatoria/";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
        var gabinetes = document.QuerySelectorAll("#gabinete option").ToList();

        foreach (var item in gabinetes)
        {
            var gabinete = item as IHtmlOptionElement;
            if (string.IsNullOrEmpty(gabinete.Value)) continue;

            var deputado = deputados.Find(x => gabinete.Value.Contains(x.Gabinete.ToString()));
            if (deputado == null)
            {
                deputado = deputados.Find(x => gabinete.Text.Split('-')[0].Trim().Equals(x.NomeCivil, StringComparison.InvariantCultureIgnoreCase));
                if (deputado != null)
                {
                    deputado.Gabinete = Convert.ToUInt32(gabinete.Value);
                    connection.Update(deputado);
                }
                else if (gabinete.Value != "54") // STI DA SILVA - DEPUTADO TESTE STI
                {
                    logger.LogError($"Deputado {gabinete.Value}: {gabinete.Text} não existe ou não possui gabinete relacionado!");
                }
            }

            IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form#form_busca_verba");
            var dcForm = new Dictionary<string, string>();
            dcForm.Add("categoria", "1"); // Geral
            dcForm.Add("ano", ano.ToString());
            dcForm.Add("mes", mes.ToString());
            dcForm.Add("gabinete", gabinete.Value);
            var subDocument = form.SubmitAsync(dcForm).GetAwaiter().GetResult();
            var mensagem = subDocument.QuerySelector("#tabela .dataTables_empty");
            if (mensagem != null)
            {
                logger.LogInformation(mensagem.TextContent);
            }

            var patternPrestador = @"Prestador: (?<prestador>.*) (?<cnpj>\d{5,20}|(\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2})|(\d{3}\.\d{3}\.\d{3}-\d{2})) (?<endereco>.*)"; // Classe: (?<classe>[^|]*) | Data: (?<data>\\d{2}\\/\\d{2}\\/\\d{4}) | Valor R\\$ (?<valor>[\\d.,]*) | (.*)
            decimal valorTotalCalculado = 0;
            decimal valorTotalPagina = 0;
            var registrosValidos = 0;

            var despesas = subDocument.QuerySelectorAll("#tabela tbody tr");
            foreach (var despesa in despesas)
            {
                var titulo = despesa.QuerySelector("td h4");
                if (titulo != null)
                {
                    logger.LogInformation(titulo.TextContent.Trim());
                    valorTotalPagina += Convert.ToDecimal(titulo.TextContent.Split("R$")[1].Trim(), cultureInfo);

                    continue;
                }

                var linha = despesa.QuerySelector("td").TextContent.Trim();
                if (string.IsNullOrEmpty(linha)) continue;

                var linhaPartes = linha.Replace("|", "").Split("\n");

                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = gabinete.Text.ToTitleCase(),
                    Cpf = gabinete.Value,
                    Ano = (short)ano,
                    Mes = (short)mes,
                    TipoDespesa = linhaPartes[1].Split(":")[1].Trim().ToTitleCase(),
                    DataEmissao = Convert.ToDateTime(linhaPartes[2].Split(":")[1].Trim(), cultureInfo),
                    Valor = Convert.ToDecimal(linhaPartes[3].Split("R$")[1].Trim(), cultureInfo),
                    Observacao = (despesa.QuerySelector("td a") as IHtmlAnchorElement).Href,
                };

                try
                {
                    Match matchPrestador = Regex.Matches(linhaPartes[0], patternPrestador)[0];
                    despesaTemp.CnpjCpf = Core.Utils.RemoveCaracteresNumericos(matchPrestador.Groups["cnpj"].Value);
                    despesaTemp.Empresa = matchPrestador.Groups["prestador"].Value.Trim();
                }
                catch (Exception)
                {
                    logger.LogError($"Fornecedor invalido: {linhaPartes[0]}");
                }

                InserirDespesaTemp(despesaTemp);
                valorTotalCalculado += despesaTemp.Valor;
                registrosValidos++;
            }

            if (valorTotalCalculado != valorTotalPagina)
            {
                logger.LogError($"Valor Divergente! Esperado: {valorTotalPagina}; Encontrado: {valorTotalCalculado}; Referencia: {mes:00}/{ano} {gabinete.Text}; {registrosValidos} Registros");
            }
        }
    }

    public override void AjustarDados()
    {
        connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '84641331000281' WHERE cnpj_cpf = '8464133101000281';

UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '37829987000161' WHERE cnpj_cpf = '04942645';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '35854166000150' WHERE cnpj_cpf = '723644241';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04707821000113' WHERE cnpj_cpf = '0470782100';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02558157000162' WHERE cnpj_cpf = '255815700162';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05423476000159' WHERE cnpj_cpf = '5423476000159';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02580011000113' WHERE cnpj_cpf = '2580011000113';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04320901000111' WHERE cnpj_cpf = '4320901000111';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08949169000102' WHERE cnpj_cpf = '8949169000102';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05248037000157' WHERE cnpj_cpf = '5248037000157';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05155654000108' WHERE cnpj_cpf = '5155654000108';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03733863000166' WHERE cnpj_cpf = '3733863000166';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02393780000293' WHERE cnpj_cpf = '2393780000293';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09300057000180' WHERE cnpj_cpf = '9300057000180';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09072517000160' WHERE cnpj_cpf = '9072517000160';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08941827000101' WHERE cnpj_cpf = '8941827000101';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08742048000187' WHERE cnpj_cpf = '8742048000187';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05147841000140' WHERE cnpj_cpf = '5147841000140';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02630029000182' WHERE cnpj_cpf = '2630029000182';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05914650000166' WHERE cnpj_cpf = '5914650000166';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08892185000106' WHERE cnpj_cpf = '8892185000106';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09561899000196' WHERE cnpj_cpf = '9561899000196';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09082304000110' WHERE cnpj_cpf = '9082304000110';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04395067000123' WHERE cnpj_cpf = '4395067000123';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05790860000190' WHERE cnpj_cpf = '5790860000190';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02770020000177' WHERE cnpj_cpf = '2770020000177';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02278249000199' WHERE cnpj_cpf = '2278249000199';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05970317000174' WHERE cnpj_cpf = '5970317000174';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03376298000127' WHERE cnpj_cpf = '3376298000127';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03903724000133' WHERE cnpj_cpf = '3903724000133';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02384613000103' WHERE cnpj_cpf = '2384613000103';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04485882000183' WHERE cnpj_cpf = '4485882000183';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01031362000102' WHERE cnpj_cpf = '1031362000102';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05893935000168' WHERE cnpj_cpf = '5893935000168';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05914254000139' WHERE cnpj_cpf = '5914254000139';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09057435000147' WHERE cnpj_cpf = '9057435000147';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04092672000125' WHERE cnpj_cpf = '4092672000125';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06249591000111' WHERE cnpj_cpf = '6249591000111';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04233946000159' WHERE cnpj_cpf = '4233946000159';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03968287000136' WHERE cnpj_cpf = '3968287000136';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01279784000100' WHERE cnpj_cpf = '1279784000100';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05759412000123' WHERE cnpj_cpf = '5759412000123';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04362088000142' WHERE cnpj_cpf = '4362088000142';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07672623000150' WHERE cnpj_cpf = '7672623000150';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '41330680000199' WHERE cnpj_cpf = '4133068000199';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08290508000183' WHERE cnpj_cpf = '8290508000183';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05203855000133' WHERE cnpj_cpf = '5203855000133';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01933030000113' WHERE cnpj_cpf = '1933030000113';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08379006000123' WHERE cnpj_cpf = '8379006000123';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02903099000167' WHERE cnpj_cpf = '2903099000167';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05752260000137' WHERE cnpj_cpf = '5752260000137';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04910840000142' WHERE cnpj_cpf = '4910840000142';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09241652000192' WHERE cnpj_cpf = '9241652000192';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05155992000140' WHERE cnpj_cpf = '5155992000140';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05168110000180' WHERE cnpj_cpf = '5168110000180';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03718553000172' WHERE cnpj_cpf = '3718553000172';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04769344000110' WHERE cnpj_cpf = '4769344000110';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01282343000159' WHERE cnpj_cpf = '1282343000159';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08106716000180' WHERE cnpj_cpf = '8106716000180';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07549414000202' WHERE cnpj_cpf = '7549414000202';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06149919000128' WHERE cnpj_cpf = '6149919000128';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05886460000182' WHERE cnpj_cpf = '5886460000182';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04501714000100' WHERE cnpj_cpf = '4501714000100';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09192278000182' WHERE cnpj_cpf = '9192278000182';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01788375000120' WHERE cnpj_cpf = '1788375000120';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04778369900177' WHERE cnpj_cpf = '4778369900177';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02990016000114' WHERE cnpj_cpf = '2990016000114';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02806741000179' WHERE cnpj_cpf = '2806741000179';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04903852000140' WHERE cnpj_cpf = '4903852000140';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05244225000107' WHERE cnpj_cpf = '5244225000107';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01618554000110' WHERE cnpj_cpf = '1618554000110';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01247049000106' WHERE cnpj_cpf = '1247049000106';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04903852000220' WHERE cnpj_cpf = '4903852000220';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01176195000198' WHERE cnpj_cpf = '1176195000198';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02308792000191' WHERE cnpj_cpf = '2308792000191';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03915997000106' WHERE cnpj_cpf = '3915997000106';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08673662000134' WHERE cnpj_cpf = '8673662000134';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '29971476000151' WHERE cnpj_cpf = '2997147600151';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05357865000123' WHERE cnpj_cpf = '5357865000123';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03566347000194' WHERE cnpj_cpf = '3566347000194';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02745235000138' WHERE cnpj_cpf = '2745235000138';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05215132002360' WHERE cnpj_cpf = '5215132002360';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '33355102000189' WHERE cnpj_cpf = '3335510200089';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07219464000132' WHERE cnpj_cpf = '7219464000132';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01940128000106' WHERE cnpj_cpf = '1940128000106';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02856644000101' WHERE cnpj_cpf = '2856644000101';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05215132000316' WHERE cnpj_cpf = '5215132000316';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05903125000145' WHERE cnpj_cpf = '5903125000145';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '39970021000175' WHERE cnpj_cpf = '3970021000175';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07322489000167' WHERE cnpj_cpf = '7322489000167';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09561157000160' WHERE cnpj_cpf = '9561157000160';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '34005545000102' WHERE cnpj_cpf = '3400554500102';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02375771000199' WHERE cnpj_cpf = '2375771000199';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06251676000134' WHERE cnpj_cpf = '6251676000134';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '14410553000127' WHERE cnpj_cpf = '1410553000127';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07161584000126' WHERE cnpj_cpf = '7161584000126';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07595449000270' WHERE cnpj_cpf = '7595449000270';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07367876000110' WHERE cnpj_cpf = '7367876000110';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03609970000187' WHERE cnpj_cpf = '3609970000187';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08413260000109' WHERE cnpj_cpf = '8413260000109';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09595636000106' WHERE cnpj_cpf = '9595636000106';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08970868000126' WHERE cnpj_cpf = '8970868000126';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02147085000160' WHERE cnpj_cpf = '2147085000160';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '29364590000113' WHERE cnpj_cpf = '2936459000113'; 
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09263023000163' WHERE cnpj_cpf = '9263023000163';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07095712000180' WHERE cnpj_cpf = '7095712000180';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '01903488000120' WHERE cnpj_cpf = '3540049000186';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06864931000114' WHERE cnpj_cpf = '6864931000114';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08293360000211' WHERE cnpj_cpf = '0829336000211';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02580020000104' WHERE cnpj_cpf = '2580020000104';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07258922000142' WHERE cnpj_cpf = '7258922000142';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08714745000124' WHERE cnpj_cpf = '8714745000124';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '00802066800102' WHERE cnpj_cpf = '0802066800102';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02492287000136' WHERE cnpj_cpf = '2492287000136';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08293360000130' WHERE cnpj_cpf = '8293360000130';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02687661000162' WHERE cnpj_cpf = '2687661000162';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07497873000109' WHERE cnpj_cpf = '7497873000109';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02766297000126' WHERE cnpj_cpf = '2766297000126';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02988602000124' WHERE cnpj_cpf = '2988602000124';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03983300000126' WHERE cnpj_cpf = '3983300000126';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '09474264000151' WHERE cnpj_cpf = '9474264000151';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '08379006000123' WHERE cnpj_cpf = '083790006000123';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '26364979000170' WHERE cnpj_cpf = '263649799000170';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04395067000123' WHERE cnpj_cpf = '043950670000123';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '26528188000139' WHERE cnpj_cpf = '265281888000139';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '15181573000136' WHERE cnpj_cpf = '151815730001356';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04956754000170' WHERE cnpj_cpf = '049567540000170';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03983300000983' WHERE cnpj_cpf = '039833000000983';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '26173720000142' WHERE cnpj_cpf = '261737200000142';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '23036550000166' WHERE cnpj_cpf = '023036550000166';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '84641331000281' WHERE cnpj_cpf = '8464133101000281';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '40735595000148' WHERE cnpj_cpf = '40735505595000148';


UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '13153784000130' WHERE cnpj_cpf = '13153748000130';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '30220907000127' WHERE cnpj_cpf = '39220907000127';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '20220198000398' WHERE cnpj_cpf = '20220196000398';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '07558594000108' WHERE cnpj_cpf = '07553594000108';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '10976424000654' WHERE cnpj_cpf = '10975424000654';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '84516368000105' WHERE cnpj_cpf = '10565713000170';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '05537429000217' WHERE cnpj_cpf = '05537729000217';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06863057000362' WHERE cnpj_cpf = '68630570001362';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '32839148000100' WHERE cnpj_cpf = '32839148000137';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '03170027000110' WHERE cnpj_cpf = '28230705000105';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '28823677000149' WHERE cnpj_cpf = '23823677000149';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06038204000106' WHERE cnpj_cpf = '06038201000106';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '49158456000173' WHERE cnpj_cpf = '48158456000173';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '63745558000116' WHERE cnpj_cpf = '61745558000116';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '35506012000258' WHERE cnpj_cpf = '35505012000258';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '06863057000362' WHERE cnpj_cpf = '06863067000443';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '18132816000134' WHERE cnpj_cpf = '18132813000134';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '46684691000190' WHERE cnpj_cpf = '48684691000190';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '21774686000229' WHERE cnpj_cpf = '21744686000229';
");
    }

    public class GrupoDespesaRO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("criado_por__first_name")]
        public string CriadoPorFirstName { get; set; }

        [JsonPropertyName("criado_por__last_name")]
        public string CriadoPorLastName { get; set; }

        [JsonPropertyName("verba__gabinete__nome_de_urna")]
        public string VerbaGabineteNomeDeUrna { get; set; }

        [JsonPropertyName("verba__mes")]
        public int VerbaMes { get; set; }

        [JsonPropertyName("verba__ano")]
        public string VerbaAno { get; set; }

        [JsonPropertyName("verba__id")]
        public int VerbaId { get; set; }

        [JsonPropertyName("categoria_verba__nome")]
        public string CategoriaVerbaNome { get; set; }

        [JsonPropertyName("tipo_verba__descricao")]
        public string TipoVerbaDescricao { get; set; }

        [JsonPropertyName("total_despesas")]
        public string TotalDespesas { get; set; }

        [JsonPropertyName("total_recomendado")]
        public string TotalRecomendado { get; set; }

        [JsonPropertyName("total_pago")]
        public string TotalPago { get; set; }
    }

    public class DespesaRO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("classe_despesa__nome")]
        public string ClasseDespesaNome { get; set; }

        [JsonPropertyName("natureza_despesa__nome")]
        public string NaturezaDespesaNome { get; set; }

        [JsonPropertyName("numero_documento_fiscal")]
        public string NumeroDocumentoFiscal { get; set; }

        [JsonPropertyName("documento_fiscal__descricao")]
        public string DocumentoFiscalDescricao { get; set; }

        [JsonPropertyName("data_documento_fiscal")]
        public string DataDocumentoFiscal { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }

        [JsonPropertyName("situacao")]
        public int Situacao { get; set; }

        [JsonPropertyName("situacao__descricao")]
        public string SituacaoDescricao { get; set; }

        [JsonPropertyName("valor_recomendado_pagamento")]
        public string ValorRecomendadoPagamento { get; set; }

        [JsonPropertyName("valor_pago")]
        public string ValorPago { get; set; }

        [JsonPropertyName("data_pagamento")]
        public string DataPagamento { get; set; }

        [JsonPropertyName("escritorio__nome")]
        public object EscritorioNome { get; set; }

        [JsonPropertyName("escritorio__endereco")]
        public object EscritorioEndereco { get; set; }

        [JsonPropertyName("escritorio__proprietario")]
        public object EscritorioProprietario { get; set; }

        [JsonPropertyName("escritorio__municipio__nome")]
        public object EscritorioMunicipioNome { get; set; }

        [JsonPropertyName("data_vencimento")]
        public object DataVencimento { get; set; }

        [JsonPropertyName("data_certificacao")]
        public object DataCertificacao { get; set; }

        [JsonPropertyName("observacao")]
        public object Observacao { get; set; }

        [JsonPropertyName("fornecedor__razao_social")]
        public string FornecedorRazaoSocial { get; set; }

        [JsonPropertyName("fornecedor__cnpj_cpf")]
        public string FornecedorCnpjCpf { get; set; }

        [JsonPropertyName("fornecedor__endereco")]
        public string FornecedorEndereco { get; set; }

        [JsonPropertyName("fornecedor__cidade__nome")]
        public string FornecedorCidadeNome { get; set; }

        [JsonPropertyName("fornecedor__estado__sigla")]
        public string FornecedorEstadoSigla { get; set; }

        [JsonPropertyName("arquivo_doc_fiscal")]
        public string ArquivoDocFiscal { get; set; }
    }
}

public class ImportadorParlamentarRondonia : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarRondonia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.ro.leg.br/deputados/perfil/",
            SeletorListaParlamentares = "section>.container>.grid>div",
            Estado = Estado.Rondonia,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var nomeparlamentar = parlamentar.QuerySelector("a div").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (parlamentar.QuerySelector("img") as IHtmlImageElement)?.Source;

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var detalhes = subDocument.QuerySelectorAll(".col-span-4 p.text-gray-600");
        deputado.NomeCivil = detalhes[0].TextContent.Trim().ToTitleCase();
        deputado.IdPartido = BuscarIdPartido(detalhes[1].TextContent.Trim());

        if (detalhes.Length > 2)
            deputado.Email = detalhes[2].TextContent.Trim();
    }
}
