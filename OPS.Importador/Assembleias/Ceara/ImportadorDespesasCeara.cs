using System.Globalization;
using System.Text;
using System.Threading;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsvHelper;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Ceara;

public class ImportadorDespesasCeara : ImportadorDespesasArquivo
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasCeara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.ce.gov.br/despesas/verba-desempenho-parlamentar",
            Estado = Estados.Ceara,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        Dictionary<string, string> arquivos = new();

        for (int mes = 1; mes <= 12; mes++)
        {
            if (ano == DateTime.Now.Year && mes > DateTime.Today.Month) break;

            string urlOrigem, caminhoArquivo;

            urlOrigem = $"{config.BaseAddress}/csv?mes={mes:00}&ano={ano}";
            caminhoArquivo = Path.Combine(tempFolder, $"CLCE-{ano}-{mes}.csv");

            //if (DateTime.Now.AddMonths(-1).Year >= ano && File.Exists(caminhoArquivo)) File.Delete(caminhoArquivo);

            arquivos.Add(urlOrigem, caminhoArquivo);
        }

        return arquivos;
    }

    public override void ImportarDespesas(string caminhoArquivo, int ano, int? mes = null)
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        int indice = 0;
        int Empenho = indice++;
        int Descricao = indice++;
        int Cnpj = indice++;
        int Credor = indice++;
        int Valor = indice++;
        int ValorTotal = indice++;

        bool linhasTitulo = true;
        string nomeParlamentar = string.Empty;

        // Nota: Títulos estão em UTF-8, mas o conteúdo em ISO-8859-1.
        using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
        using (var csv = new CsvReader(reader, cultureInfo))
        {
            while (csv.Read())
            {
                var linha = csv[Empenho].ToString().Trim();

                if (string.IsNullOrEmpty(linha)) continue; //Linha vazia

                if (linhasTitulo)
                {
                    if (linha.StartsWith("Mes/Ano:")) continue; // Linha 1, informação do mês e ano

                    if (linha.StartsWith("DEP")) // Linha 2, Nome do Parlamentar
                    {
                        nomeParlamentar = linha;
                        continue;
                    }

                    if (linha.Equals("EMPENHO")) // Linha 3, títulos das colunas
                    {

                        if (
                            csv[Empenho] != "EMPENHO" ||
                            csv[Cnpj] != "CNPJ" ||
                            csv[Credor] != "CREDOR" ||
                            csv[Valor] != "VALOR"
                        )
                            throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Ceara");

                        linhasTitulo = false;
                        continue;
                    }

                    throw new NotImplementedException();
                }

                if (linha.StartsWith("TOTAL GERAL")) // Linha de totalização
                {
                    linhasTitulo = true;
                    continue;
                }

                if (string.IsNullOrEmpty(nomeParlamentar))
                {
                    logger.LogWarning("Parlamentar não identificado. Empenho: {Empenho}. Valor: R$ {Valor}. Descrição: {Descricao}", linha, csv[Valor].ToString(), csv[Descricao].ToString());
                    continue;
                }

                CamaraEstadualDespesaTemp despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Documento = csv[Empenho].ToString(),
                    Observacao = csv[Descricao].ToString(),
                    CnpjCpf = csv[Cnpj].ToString().Trim(),
                    NomeFornecedor = csv[Credor].ToString(),
                    Valor = Convert.ToDecimal(csv[Valor].ToString(), cultureInfo),
                    Ano = (short)ano,
                    Mes = (short)mes,
                    DataEmissao = new DateOnly(ano, mes.Value, 1),
                    TipoDespesa = ObterTipoDespesa(csv[Descricao].ToString()),
                    Nome = CorrigeNomeParlamentar(nomeParlamentar)
                };

                InserirDespesaTemp(despesaTemp);
            }

        }
    }

    private string CorrigeNomeParlamentar(string nome)
    {
        nome = nome.Replace("DEP", "")
            //.Replace(".", "")
            //.Replace("Por Solicitacao do Utado", "")
            //.Replace("Por Solicitacao da Utada", "")
            //.Split("-")[0]
            .Trim();

        nome = nome.ToUpper() switch
        {
            "PLANO DE SAUDE" => string.Empty,
            "225/03" => string.Empty, // TODO: Arquivo 2023-07 R$ 650, verificar
            _ => nome
        };

        return nome.ToTitleCase();
    }

    private string ObterTipoDespesa(string tipo)
    {
        switch (tipo)
        {
            case "ALIMENTAÇÃO": return "Alimentação";
            case "ASSESSORIA": return "Assessoria e Consultoria";
            case "COMBUSTÍVEIS": return "Combustíveis";
            case "CONSULTORIA": return "Assessoria e Consultoria";
            case "DIVULGAÇÃO DAS ATIVIDADES PARLAMENTARES": return "Divulgação das Atividades Parlamentares";
            case "INTERNET": return "Internet e TV";
            case "LOCAÇÃO DO VEÍCULO": return "Locação de veículos";
            case "LOCAÇÃO DOS VEÍCULOS": return "Locação de veículos";
            case "MANUTECAO DE SITE": return "Hospedagem, Atualização e Manutenção de Sites";
            case "PASSAGENS AÉREAS": return "Passagens Aéreas";
            case "PASSAGENS TERRESTRES": return "Passagens Terrestres";
            case "PESQUISA DE OPINIÃO": return "Pesquisa de Opinião Pública";
            case "PLANO DE SAÚDE": return "Plano de Saúde";
            case "PLANO DE SAUDE": return "Plano de Saúde";
            case "SEGURO DE VIDA": return "Seguro de Vida";
            case "SERVIÇOS DE HOSPEDAGEM": return "Serviços de Hospedagem";
            case "SERVIÇOS GRÁFICOS": return "Serviços Gráficos";
            case "SERVIÇOS POSTAIS": return "Serviços Postais";
            case "TELEFONIA": return "Telefonia";
            case "TV": return "Internet e TV";
        }

        switch (tipo)
        {
            case string t when t.Contains("REFEICAO") ||
                t.Contains("REFEIÇÃO") ||
                t.Contains("ALIMENTACAO") ||
                t.Contains("ALIMENTAÇÃO"):
                return "Alimentação";

            case string t when t.Contains("ABASTECIMENTO DE COMBUSTIVEIS") ||
                t.Contains("ABASTECIMENTO DE COMBUSTÍVEIS") ||
                t.Contains("COMBIUSTIVEIS"):
                return "Combustíveis";

            case string t when t.Contains("CONSULTORIA") ||
                t.Contains("ASSESSORIA") ||
                t.Contains("ASSESORIA") ||
                t.Contains("ACOMPANHAMENTO") ||
                t.Contains("RECURSOS CONSIGNADOS NO ORCAMENTO"):
                return "Assessoria e Consultoria";

            case string t when t.Contains("TELEFONIA") ||
                t.Contains("TELECOMUNICAÇÕES") ||
                t.Contains("TELECOMUNICACOES") ||
                t.Contains("INTERNET") ||
                t.Contains("INTERTNET") ||
                t.Contains("BANDA LARGA") ||
                t.Contains("TV") ||
                t.Contains("MULTIMIDIA") ||
                t.Contains("MULTIMÍDIA") ||
                t.Contains("ASSINATURA"):
                return "Internet e TV";

            case string t when t.Contains("FRETAMENTO DA AERONAVE") ||
                t.Contains("FRETAMENTO DE HELICOPTERO"):
                return "Fretamento de Aeronaves";

            case string t when t.Contains("HOSPEDAGEM") ||
                t.Contains("HOSPEDAGENS"):
                return "Serviços de Hospedagem";

            case string t when t.Contains("PASSAGENS TERRESTRES") ||
                t.Contains("VALE TRANSPORTE"):
                return "Passagens Terrestres";

            case string t when t.Contains("PASSAGENS AEREAS") ||
                t.Contains("PASSAGENS AÉREAS") ||
                t.Contains("PASSAGEM AEREA") ||
                t.Contains("PASSAGEM AÉREA") ||
                t.Contains("PASSAGEM ÁEREA"):
                return "Passagens Aéreas";

            case string t when t.Contains("GRAFICOS") ||
                t.Contains("GRÁFICOS"):
                return "Serviços Gráficos";

            case string t when t.Contains("ATIVIDADES PARLAMENTARES") ||
                t.Contains("ATIVIDADE PARLAMENTAR") ||
                t.Contains("ATIVIDADES ARLAMENTARES") ||
                t.Contains("VEICULAÇÃO DE MÍDIAS") ||
                t.Contains("DIVULGACAO") ||
                t.Contains("DIVULGAÇÃO") ||
                t.Contains("PUBLICAÇÃO") ||
                t.Contains("PUBLICACAO") ||
                t.Contains("COMUNICACAO") ||
                t.Contains("COMUNICAÇÃO"):
                return "Divulgação das Atividades Parlamentares";

            case string t when t.Contains("PLANO DE SAUDE") ||
                t.Contains("PLANO DE SAÚDE") ||
                t.Contains("PLANO DE SDAUDE"):
                return "Plano de Saúde";

            case string t when t.Contains("SEGURO DE VIDA") ||
                t.Contains("SEGURO DEVIDA"):
                return "Seguro de Vida";

            case string t when t.Contains("SITE") ||
                t.Contains("SITIO") ||
                t.Contains("MANUTENÇÃO DO SÍTIO"):
                return "Hospedagem, Atualização e Manutenção de Sites";

            case string t when t.Contains("PESQUISA") ||
                t.Contains("OPINIAO PUBLICA") ||
                t.Contains("OPINIÃO PÚBLICA"):
                return "Pesquisa de Opinião Pública";

            case string t when t.Contains("POSTAIS"): return "Serviços Postais";

            case string t when t.Contains("VEICULO") ||
                t.Contains("VEÍCULO") ||
                t.Contains("LOCACAO") ||
                t.Contains("LOCAÇÃO"):
                return "Locação de veículos";
        }

        return "Indenizações e Restituições";
    }
}
