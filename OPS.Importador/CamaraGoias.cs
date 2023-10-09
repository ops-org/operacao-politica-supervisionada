using AngleSharp;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OPS.Core;
using OPS.Core.Entity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    public class CamaraGoias : ImportadorCotaParlamentarBase
    {
        public CamaraGoias(ILogger<CamaraSantaCatarina> logger, IConfiguration configuration, IDbConnection connection)
            : base("GO", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            var address = $"https://portal.al.go.leg.br/";
            var task = context.OpenAsync(address);
            task.Wait();
            var document = task.Result;
            if (document.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"{address} {document.StatusCode}");
            };

            var parlamentares = document.QuerySelectorAll(".deputados-e-comissoes__lista a");
            foreach (var parlamentar in parlamentares)
            {
                var deputado = new DeputadoEstadual();
                deputado.IdEstado = (ushort)idEstado;

                deputado.UrlPerfil = (parlamentar as IHtmlAnchorElement).Href;

                //Thread.Sleep(TimeSpan.FromSeconds(15));
                var taskSub = context.OpenAsync(deputado.UrlPerfil.Replace("portal.al.go.leg.br/legado", "portal-legado.al.go.leg.br"));
                taskSub.Wait();
                var subDocument = taskSub.Result;
                if (subDocument.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{deputado.UrlPerfil} {subDocument.StatusCode}");
                    continue;
                };

                deputado.UrlFoto = (subDocument.QuerySelector(".deputado__foto img") as IHtmlImageElement)?.Source;
                deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());

                var detalhes = subDocument.QuerySelector(".deputado__dados");
                if (detalhes is null) continue;

                deputado.NomeParlamentar = detalhes.QuerySelector("h1").TextContent.Trim();
                if (deputado.NomeParlamentar.Contains("Perfil biográfico de"))
                {
                    Console.WriteLine($"Pulando {deputado.NomeParlamentar}");
                    continue;
                }

                var partido = detalhes.QuerySelector("h2").TextContent.Trim();
                if (partido.Contains("("))
                    partido = partido.Split(new[] { '(', ')' })[1];

                var contatos = detalhes.QuerySelectorAll(".deputado__contatos .cpi-dados__desc");

                deputado.Telefone = contatos[1].QuerySelector("h2").TextContent.Trim();
                deputado.Email = contatos[2].QuerySelector("a").TextContent.Trim();

                //TODO: https://portal-legado.al.go.leg.br/deputado/perfil/deputado/2133
                // carregar Naturalidade, Nascimento, Partido da eleição

                var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = partido }).FirstOrDefault()?.Id;

                if (IdPartido == null)
                {
                    IdPartido = connection.GetList<Core.Entity.Partido>(new { nome = partido }).FirstOrDefault()?.Id;

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

        public override void ImportarArquivoDespesas(int ano)
        {
            LimpaDespesaTemporaria();

            var today = DateTime.Today;
            for (int mes = 1; mes <= 12; mes++)
            {
                if (ano == today.Year && mes > today.Month) break;

                ImportarArquivoDespesas(ano, mes);
            }
        }

        public void ImportarArquivoDespesas(int ano, int mes)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            var address = $"https://transparencia.al.go.leg.br/api/transparencia/verbas_indenizatorias?ano={ano}&mes={mes}&por_pagina=100";
            var restClient = new RestClient();
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var request = new RestRequest(address, Method.GET);
            request.AddHeader("Accept", "application/json");

            IRestResponse resParlamentares = restClient.ExecuteWithAutoRetry(request);
            List<DeputadoGoias> lstDeputadosRS = JsonSerializer.Deserialize<List<DeputadoGoias>>(resParlamentares.Content);

            foreach (DeputadoGoias item in lstDeputadosRS)
            {
                var id = item.Deputado.Id;


                address = $"https://transparencia.al.go.leg.br/api/transparencia/verbas_indenizatorias/exibir?ano=ano&deputado_id={id}&mes={mes}";
                request.Resource = address;

                IRestResponse resDespesa = restClient.ExecuteWithAutoRetry(request);
                DespesasGoias despesa = JsonSerializer.Deserialize<DespesasGoias>(resDespesa.Content);
                if (despesa.Grupos is null) continue;

                foreach (var grupo in despesa.Grupos)
                {
                    foreach (var sub in grupo.Subgrupos)
                    {
                        foreach (var lancamento in sub.Lancamentos)
                        {
                            var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
                            {
                                Nome = despesa.Deputado.Nome,
                                Cpf = id.ToString(),
                                Ano = (short)ano,
                                TipoDespesa = grupo.Descricao,
                                Valor = Convert.ToDecimal(lancamento.Fornecedor.ValorIndenizado, cultureInfo),
                                DataEmissao = lancamento.Fornecedor.Data,
                                CnpjCpf = Utils.RemoveCaracteresNaoNumericos(lancamento.Fornecedor.CnpjCpf),
                                Empresa = lancamento.Fornecedor.Nome,
                                Documento = lancamento.Fornecedor.Numero
                            };

                            connection.Insert(objCamaraEstadualDespesaTemp);
                        }
                    }
                }
            }
        }


        //public class Deputado
        //{
        //    [JsonPropertyName("id")]
        //    public int Id { get; set; }

        //    [JsonPropertyName("nome")]
        //    public string Nome { get; set; }
        //}

        public class DeputadoGoias
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("ano")]
            public int Ano { get; set; }

            [JsonPropertyName("mes")]
            public int Mes { get; set; }

            [JsonPropertyName("valor_apresentado")]
            public string ValorApresentado { get; set; }

            [JsonPropertyName("valor_indenizado")]
            public string ValorIndenizado { get; set; }

            // Apenas Id e Nome
            [JsonPropertyName("deputado")]
            public Deputado Deputado { get; set; }
        }

        // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
        public class Deputado
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("partido")]
            public string Partido { get; set; }

            [JsonPropertyName("foto")]
            public string Foto { get; set; }

            [JsonPropertyName("twitter")]
            public string Twitter { get; set; }

            [JsonPropertyName("facebook")]
            public string Facebook { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("sala")]
            public string Sala { get; set; }
        }

        public class Fornecedor
        {
            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("cnpj_cpf")]
            public string CnpjCpf { get; set; }

            [JsonPropertyName("data")]
            public DateTime Data { get; set; }

            [JsonPropertyName("numero")]
            public string Numero { get; set; }

            [JsonPropertyName("valor_apresentado")]
            public string ValorApresentado { get; set; }

            [JsonPropertyName("valor_indenizado")]
            public string ValorIndenizado { get; set; }
        }

        public class Grupo
        {
            [JsonPropertyName("descricao")]
            public string Descricao { get; set; }

            //[JsonPropertyName("valor_apresentado")]
            //public decimal ValorApresentado { get; set; }

            //[JsonPropertyName("valor_indenizado")]
            //public decimal ValorIndenizado { get; set; }

            [JsonPropertyName("subgrupos")]
            public List<Subgrupo> Subgrupos { get; set; }
        }

        public class Lancamento
        {
            [JsonPropertyName("fornecedor")]
            public Fornecedor Fornecedor { get; set; }
        }

        public class DespesasGoias
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("ano")]
            public int Ano { get; set; }

            [JsonPropertyName("mes")]
            public int Mes { get; set; }

            [JsonPropertyName("deputado")]
            public Deputado Deputado { get; set; }

            //[JsonPropertyName("valor_apresentado")]
            //public decimal ValorApresentado { get; set; }

            //[JsonPropertyName("valor_indenizado")]
            //public decimal ValorIndenizado { get; set; }

            [JsonPropertyName("grupos")]
            public List<Grupo> Grupos { get; set; }
        }

        public class Subgrupo
        {
            [JsonPropertyName("descricao")]
            public string Descricao { get; set; }

            //[JsonPropertyName("valor_apresentado")]
            //public decimal ValorApresentado { get; set; }

            //[JsonPropertyName("valor_indenizado")]
            //public decimal ValorIndenizado { get; set; }

            [JsonPropertyName("lancamentos")]
            public List<Lancamento> Lancamentos { get; set; }
        }



    }
}
