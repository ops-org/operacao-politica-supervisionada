﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.ALE.Comum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado de Santa Catarina
/// https://transparencia.alesc.sc.gov.br/
/// </summary>
public class SantaCatarina : ImportadorBase
{
    public SantaCatarina(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarSantaCatarina(serviceProvider);
        importadorDespesas = new ImportadorDespesasSantaCatarina(serviceProvider);
    }
}

public class ImportadorDespesasSantaCatarina : ImportadorDespesasArquivo
{
    public ImportadorDespesasSantaCatarina(IServiceProvider serviceProvider) : base(serviceProvider)
    {

        base.config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://sapl.al.ac.leg.br/parlamentar/",
            Estado = Estado.SantaCatarina,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }


    /// <summary>
    /// Dados a partir de 2011
    /// https://transparencia.alesc.sc.gov.br/gabinetes_dados_abertos.php
    /// </summary>
    /// <param name="ano"></param>
    /// <returns></returns>
    public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        Dictionary<string, string> arquivos = new();

        // https://transparencia.alesc.sc.gov.br/gabinetes_dados_abertos.php
        // Arquivos disponiveis anualmente a partir de 2011
        var urlOrigem = $"https://transparencia.alesc.sc.gov.br/gabinetes_csv.php?ano={ano}";
        var caminhoArquivo = $"{tempPath}/CLSC-{ano}.csv";

        //if (DateTime.Now.AddMonths(-1).Year >= ano && File.Exists(caminhoArquivo)) File.Delete(caminhoArquivo);

        arquivos.Add(urlOrigem, caminhoArquivo);
        return arquivos;
    }

    public override void ImportarDespesas(string caminhoArquivo, int ano)
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        int indice = 0;
        int Verba = indice++;
        int Descricao = indice++;
        int Conta = indice++;
        int Favorecido = indice++;
        int Trecho = indice++;
        int Vencimento = indice++;
        int Valor = indice++;

        using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
        using (var csv = new CsvReader(reader, cultureInfo))
        {
            var linhasProcessadasAno = 0;
            while (csv.Read())
            {
                linhasProcessadasAno++;

                if (linhasProcessadasAno == 1)
                {

                    if (
                        csv[Verba] != "Verba" ||
                        csv[Descricao] != "Descrição" ||
                        csv[Conta] != "Conta" ||
                        csv[Favorecido] != "Favorecido" ||
                        csv[Trecho] != "Trecho" ||
                        csv[Vencimento] != "Vencimento" ||
                        csv[Valor] != "Valor"
                    )
                        throw new Exception("Mudança de integração detectada para o Câmara Legislativa de Santa Catarina");

                    // Pular linha de titulo
                    continue;
                }

                if (string.IsNullOrEmpty(csv[Verba])) continue; //Linha vazia

                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = csv[Conta].Split("(")[0].Trim().ToTitleCase(),
                    Ano = (short)ano,
                    TipoVerba = csv[Verba],
                    TipoDespesa = csv[Descricao],
                    Valor = Convert.ToDecimal(csv[Valor], cultureInfo),
                    DataEmissao = DateTime.Parse(csv[Vencimento]),
                    Favorecido = csv[Favorecido],
                    Observacao = csv[Trecho]
                };

                InserirDespesaTemp(despesaTemp);
            }

            // Ignorar linha de titulo
            logger.LogDebug("{Linhas} linhas processadas!", --linhasProcessadasAno);
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

    public override void InsereFornecedorFaltante()
    {
        // Faz nada.
    }

//    public override void InsereDeputadoFaltante()
//    {
//        var affected = connection.Execute(@$"
//INSERT INTO cl_deputado (nome_parlamentar, matricula, id_estado)
//select distinct Nome, cpf, {idEstado}
//from ops_tmp.cl_despesa_temp
//where nome not in (
//    select nome_parlamentar from cl_deputado where id_estado =  {idEstado} 
//    UNION all
//    select nome_civil from cl_deputado where id_estado =  {idEstado} 
//);
//                ");

//        if (affected > 0)
//        {
//            logger.LogInformation("{Itens} parlamentares incluidos!", affected);
//        }
//    }

    public override void InsereDespesaFinal(int ano)
    {
        base.InsereDespesaFinal(ano);

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
    }
}

public class ImportadorParlamentarSantaCatarina : ImportadorParlamentarCrawler
{
    public ImportadorParlamentarSantaCatarina(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig
        {
            Estado = Estado.SantaCatarina,
            BaseAddress = "https://www.alesc.sc.gov.br/todos-deputados",
            SeletorListaParlamentares = "table.views-table tbody tr"
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        var itens = document.QuerySelectorAll("td");
        var colunaNome = itens[0].QuerySelector("a");

        var nomeparlamentar = colunaNome.TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (colunaNome as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(itens[1].TextContent.Trim());
        deputado.Email = itens[2].QuerySelector("a").TextContent.Trim();

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var detalhes = subDocument.QuerySelectorAll(".deputado-resumo");
        deputado.Nascimento = DateOnly.FromDateTime(DateTime.Parse(detalhes[0].QuerySelector("span.date-display-single").Attributes["content"].Value));
        deputado.Escolaridade = detalhes[1].TextContent.Replace("Escolaridade:", "").Trim();
        deputado.Naturalidade = detalhes[2].TextContent.Replace("Origem:", "").Split('/')[0].Trim();
        //var gabinete = detalhes[3].TextContent.Trim();
        deputado.Telefone = detalhes[4].TextContent.Replace("Contatos:", "").Trim();
    }
}
