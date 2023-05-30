using AngleSharp;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Markup;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa da Bahia
    /// 
    /// </summary>
    public class CamaraCeara : ImportadorCotaParlamentarBase
    {
        public CamaraCeara(ILogger<CamaraCeara> logger, IConfiguration configuration, IDbConnection connection) :
            base("CE", logger, configuration, connection)
        {
        }

        //public override void ImportarParlamentares()
        //{
        //    var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        //    var config = Configuration.Default.WithDefaultLoader();
        //    var context = BrowsingContext.New(config);

        //    var address = $"https://www.al.ba.gov.br/deputados/legislatura-atual";
        //    var task = context.OpenAsync(address);
        //    task.Wait();
        //    var document = task.Result;
        //    if (document.StatusCode != HttpStatusCode.OK)
        //    {
        //        Console.WriteLine($"{address} {document.StatusCode}");
        //    };

        //    var parlamentares = document.QuerySelectorAll(".fe-div-table>div");
        //    foreach (var parlamentar in parlamentares)
        //    {
        //        var deputado = new DeputadoEstadual();
        //        deputado.IdEstado = (ushort)idEstado;

        //        deputado.UrlPerfil = (parlamentar.QuerySelector(".list-item a") as IHtmlAnchorElement).Href;
        //        deputado.UrlFoto = (parlamentar.QuerySelector(".list-item img") as IHtmlImageElement)?.Source;
        //        deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());

        //        deputado.NomeParlamentar = parlamentar.QuerySelector(".deputado-nome span").TextContent.Trim();
        //        var partido = parlamentar.QuerySelector(".partido-nome").TextContent.Trim();

        //        //Thread.Sleep(TimeSpan.FromSeconds(15));
        //        var taskSub = context.OpenAsync(deputado.UrlPerfil);
        //        taskSub.Wait();
        //        var subDocument = taskSub.Result;
        //        if (subDocument.StatusCode != HttpStatusCode.OK)
        //        {
        //            Console.WriteLine($"{deputado.UrlPerfil} {subDocument.StatusCode}");
        //            continue;
        //        };

        //        var detalhes = subDocument.QuerySelectorAll(".dados-deputado p");
        //        deputado.NomeCivil = detalhes[0].QuerySelector("span").TextContent.Trim();

        //        var profissaoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("PROFISSÃO", StringComparison.InvariantCultureIgnoreCase));
        //        if (profissaoElemento is not null)
        //        {
        //            deputado.Profissao = profissaoElemento.QuerySelector("span").TextContent.Trim();
        //        }

        //        var nascimentoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("NASCIMENTO", StringComparison.InvariantCultureIgnoreCase));
        //        if (nascimentoElemento is not null)
        //        {
        //            var nascimentoComNaturalidade = nascimentoElemento.QuerySelector("span").TextContent.Split(',');
        //            deputado.Nascimento = DateOnly.Parse(nascimentoComNaturalidade[0].Trim(), cultureInfo);
        //            deputado.Naturalidade = nascimentoComNaturalidade[1].Trim();
        //        }

        //        var sexoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("SEXO", StringComparison.InvariantCultureIgnoreCase));
        //        if (sexoElemento is not null)
        //            deputado.Sexo = sexoElemento.QuerySelector("span").TextContent.Trim()[0].ToString();

        //        var contatos = subDocument.QuerySelectorAll(".fe-dep-dados-ajsut-mobile .linha-cv strong").First(x => x.TextContent.Trim() == "Contato").ParentElement;
        //        var telefoneElemento = contatos.QuerySelectorAll("p").FirstOrDefault(x => x.TextContent.StartsWith("Tel"));
        //        if (telefoneElemento != null)
        //            deputado.Telefone = String.Join(" ", telefoneElemento.ParentElement.QuerySelectorAll("span").Select(x => x.TextContent.Trim()));

        //        deputado.Email = contatos.QuerySelector("p a span").TextContent?.Trim();

        //        var IdPartido = connection.GetList<Partido>(new { sigla = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA").Replace("REPUB", "REPUBLICANOS") }).FirstOrDefault()?.Id;
        //        if (IdPartido == null)
        //            throw new Exception("Partido Inexistenete");

        //        deputado.IdPartido = IdPartido.Value;

        //        var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, nome_parlamentar = deputado.NomeParlamentar }).FirstOrDefault()?.Id;
        //        if (IdDeputado == null)
        //            connection.Insert(deputado);
        //        else
        //        {
        //            deputado.Id = IdDeputado.Value;
        //            connection.Update(deputado);
        //        }
        //    }
        //}


        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();

            // https://transparencia.al.ce.gov.br/index.php/despesas/verba-de-desempenho-parlamentar#resultado
            // Arquivos disponiveis anualmente a partir de 2021

            for (int mes = 1; mes <= 12; mes++)
            {
                if (DateTime.Today.Year == ano && DateTime.Today.Month > mes) break;

                var base64 = Utils.EncodeTo64($"{mes:00}|{ano}|");
                var _urlOrigem = $"https://transparencia.al.ce.gov.br/includes/verba_de_desempenho_parlamentar_csv.php?codigo={base64}";
                var _caminhoArquivo = $"{tempPath}/CLCE-{ano}-{mes}.csv";

                arquivos.Add(_urlOrigem, _caminhoArquivo);
            }

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

            var linha = 0;
            var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp();
            foreach (string line in System.IO.File.ReadLines(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
            {
                if (line.StartsWith("TOTAL GERAL"))
                {
                    linha = 0;
                    continue;
                }
                linha++;

                if (linha == 1)
                {
                    objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = line,
                        Ano = (short)ano
                    };

                    continue;
                }

                if (linha == 2)
                {
                    objCamaraEstadualDespesaTemp.DataEmissao = Convert.ToDateTime(line.Replace("Mes/Ano: ", ""), cultureInfo);
                    continue;
                }

                if (linha == 3)
                    continue;

                var colunas = line.Split(';');
                if (colunas.Length != 6) // Finaliza com ;
                    throw new Exception("Linha Invalida" + line);

                objCamaraEstadualDespesaTemp.Id = 0;
                objCamaraEstadualDespesaTemp.TipoDespesa = ObterTipoDespesa(colunas[1].Trim());
                objCamaraEstadualDespesaTemp.Documento = colunas[0].Trim();
                objCamaraEstadualDespesaTemp.Observacao = colunas[1].Trim();
                objCamaraEstadualDespesaTemp.CnpjCpf = colunas[2].Trim();
                objCamaraEstadualDespesaTemp.Empresa = colunas[3].Trim();
                objCamaraEstadualDespesaTemp.Valor = Convert.ToDecimal(colunas[4], cultureInfo);

                if (RegistroExistente(objCamaraEstadualDespesaTemp, lstHash))
                    continue;

                connection.Insert(objCamaraEstadualDespesaTemp);
            }

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
                case "SERVIÇOS POSTAIS": return "Serviços POstais";
                case "TELEFONIA": return "Telefonia";
                case "TV": return "Internet e TV";
            }

            return null;
        }
    }

}
