using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Exceptions;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.CamaraFederal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OPS.Importador.CamaraFederal
{
    public class ImportacaoCamaraFederal
    {
        protected readonly ILogger<ImportacaoCamaraFederal> logger;
        protected readonly IServiceProvider serviceProvider;
        protected readonly AppDbContext dbContext;
        public HttpClient httpClient { get; }

        public CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        public readonly int NumeroThreads = 10;

        public ImportacaoCamaraFederal(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            logger = serviceProvider.GetRequiredService<ILogger<ImportacaoCamaraFederal>>();
            httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ResilientClient");
        }

        public void ColetaDadosDeputados()
        {
            DefineColunasRemuneracaoSecretarios();

            var sqlDeputados = @"
SELECT DISTINCT cd.id as Id, nome_parlamentar as NomeParlamentar
FROM camara.cf_deputado cd 
JOIN camara.cf_mandato m ON m.id_cf_deputado = cd.id
WHERE (id_legislatura = 57 OR cd.situacao = 'Exercício')";
            var dcDeputado = dbContext.Database.GetDbConnection().Query<(int Id, string NomeParlamentar)>(sqlDeputados).ToList();
            ConcurrentQueue<(int Id, string NomeParlamentar)> queue = new ConcurrentQueue<(int Id, string NomeParlamentar)>(dcDeputado);

            Task[] tasks = new Task[NumeroThreads];
            for (int i = 0; i < NumeroThreads; ++i)
            {
                tasks[i] = ProcessaFilaColetaDadosDeputados(queue);
            }

            Task.WaitAll(tasks);
        }

        private readonly string[] meses = new string[] { "JAN", "FEV", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OUT", "NOV", "DEZ" };

        private async Task ProcessaFilaColetaDadosDeputados(ConcurrentQueue<(int Id, string NomeParlamentar)> queue)
        {
            var anoCorte = DateTime.Today.Year - 1; // 2008;
            var cultureInfoBR = CultureInfo.CreateSpecificCulture("pt-BR");
            var cultureInfoUS = CultureInfo.CreateSpecificCulture("en-US");

            var context = httpClient.CreateAngleSharpContext();

            while (queue.TryDequeue(out (int Id, string NomeParlamentar) deputado))
            {
                using var scope = serviceProvider.CreateScope();
                var localDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = deputado.NomeParlamentar }))
                {
                    var idDeputado = deputado.Id;
                    bool possuiPassaporteDiplimatico = false;
                    int qtdSecretarios = 0;
                    var processado = true;

                    try
                    {
                        var address = $"https://www.camara.leg.br/deputados/{idDeputado}";
                        var document = await context.OpenAsyncAutoRetry(address);
                        if (document.StatusCode != HttpStatusCode.OK)
                        {
                            logger.LogError($"{address} {document.StatusCode}");
                        }
                        else
                        {
                            var anosmandatos = document
                                .QuerySelectorAll(".linha-tempo li")
                                .Select(x => Convert.ToInt32(x.TextContent.Trim()))
                                .Where(x => x >= anoCorte);
                            if (anosmandatos.Any())
                            {
                                foreach (var ano in anosmandatos)
                                {
                                    using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
                                    {
                                        await ColetaSalarioDeputado(context, idDeputado, ano, localDbContext);

                                        address = $"https://www.camara.leg.br/deputados/{idDeputado}?ano={ano}";
                                        document = await context.OpenAsyncAutoRetry(address);
                                        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                                        {
                                            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
                                            continue;
                                        }

                                        var lstInfo = document.QuerySelectorAll(".recursos-beneficios-deputado-container .beneficio");
                                        if (!lstInfo.Any())
                                        {
                                            logger.LogError("Nenhuma informação de beneficio: {address}", address);
                                            continue;
                                        }

                                        var verbasGabineteExistentes = localDbContext.DeputadoVerbasGabinete
                                            .AsNoTracking()
                                            .Where(x => x.IdDeputado == idDeputado && x.Ano == (short)ano)
                                            .Select(x => x.Mes)
                                            .ToList();

                                        var verbaGabineteMensal = document.QuerySelectorAll("#gastomensalverbagabinete tbody tr");
                                        foreach (var item in verbaGabineteMensal)
                                        {
                                            var colunas = item.QuerySelectorAll("td");

                                            var mes = (short)(Array.IndexOf(meses, colunas[0].TextContent) + 1);
                                            if (verbasGabineteExistentes.Contains(mes)) continue;

                                            var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                                            var percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""), cultureInfoUS);

                                            localDbContext.DeputadoVerbasGabinete.Add(new DeputadoVerbaGabinete()
                                            {
                                                IdDeputado = idDeputado,
                                                Ano = (short)ano,
                                                Mes = mes,
                                                Valor = valor,
                                                Percentual = percentual
                                            });
                                        }

                                        var cotasParlamentaresExistentes = localDbContext.DeputadoCotaParlamentares
                                            .AsNoTracking()
                                            .Where(x => x.IdDeputado == idDeputado && x.Ano == (short)ano)
                                            .Select(x => x.Mes)
                                            .ToList();

                                        var cotaParlamentarMensal = document.QuerySelectorAll("#gastomensalcotaparlamentar tbody tr");
                                        foreach (var item in cotaParlamentarMensal)
                                        {
                                            var colunas = item.QuerySelectorAll("td");

                                            var mes = (short)(Array.IndexOf(meses, colunas[0].TextContent) + 1);
                                            if (cotasParlamentaresExistentes.Contains(mes)) continue;

                                            var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                                            decimal? percentual = null;
                                            if (!string.IsNullOrEmpty(colunas[2].TextContent.Replace("%", "")))
                                                percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""), cultureInfoUS);

                                            localDbContext.DeputadoCotaParlamentares.Add(new DeputadoCotaParlamentar()
                                            {
                                                IdDeputado = idDeputado,
                                                Ano = (short)ano,
                                                Mes = mes,
                                                Valor = valor,
                                                Percentual = percentual
                                            });
                                        }

                                        foreach (var info in lstInfo)
                                        {
                                            try
                                            {
                                                var titulo = info.QuerySelector(".beneficio__titulo")?.TextContent?.Replace(" ?", "")?.Trim();
                                                var valor = info.QuerySelector(".beneficio__info")?.TextContent?.Trim(); //Replace \n

                                                switch (titulo)
                                                {
                                                    case "Pessoal de gabinete":
                                                        if (valor != "0 pessoas")
                                                        {
                                                            if ((ano == DateTime.Today.Year))
                                                                qtdSecretarios = Convert.ToInt32(valor.Split(' ')[0]);

                                                            await ColetaPessoalGabinete(context, idDeputado, ano, localDbContext);
                                                        }
                                                        break;
                                                    case "Salário mensal bruto":
                                                        // Nada: Deve Consultar por ano
                                                        break;
                                                    case "Imóvel funcional":
                                                        if (!valor.StartsWith("Não fez") && !valor.StartsWith("Não faz") && !valor.StartsWith("Não há dado"))
                                                        {
                                                            var lstImovelFuncional = valor.Split("\n");

                                                            var imoveisExistentes = localDbContext.DeputadoImoveisFuncionais
                                                                .AsNoTracking()
                                                                .Where(x => x.IdDeputado == idDeputado)
                                                                .Select(x => x.UsoDe)
                                                                .ToList();

                                                            foreach (var item in lstImovelFuncional)
                                                            {
                                                                var periodos = item.Trim().Split(" ");

                                                                var dataInicial = DateOnly.Parse(periodos[3]);
                                                                if (imoveisExistentes.Contains(dataInicial)) continue;

                                                                DateOnly? dataFinal = null;
                                                                int? dias = null;

                                                                if (!item.Contains("desde"))
                                                                {
                                                                    dataFinal = DateOnly.Parse(periodos[5]);
                                                                    dias = (dataFinal.Value.ToDateTime(TimeOnly.MinValue) - dataInicial.ToDateTime(TimeOnly.MinValue)).Days;
                                                                }

                                                                localDbContext.DeputadoImoveisFuncionais.Add(new DeputadoImovelFuncional()
                                                                {
                                                                    IdDeputado = idDeputado,
                                                                    UsoDe = dataInicial,
                                                                    UsoAte = dataFinal,
                                                                    TotalDias = dias
                                                                });
                                                            }
                                                        }
                                                        break;
                                                    case "Auxílio-moradia":
                                                        if (valor != "Não recebe o auxílio")
                                                        {
                                                            await ColetaAuxilioMoradia(context, idDeputado, ano, localDbContext);
                                                        }
                                                        break;
                                                    case "Viagens em missão oficial":
                                                        if (valor != "0")
                                                        {
                                                            await ColetaMissaoOficial(context, idDeputado, ano, localDbContext);
                                                        }
                                                        break;
                                                    case "Passaporte diplomático":
                                                        if (valor != "Não possui")
                                                        {
                                                            possuiPassaporteDiplimatico = true;
                                                        }
                                                        break;
                                                    default:
                                                        throw new NotImplementedException($"Vefificar beneficios do parlamentar {idDeputado} para o ano {ano}.");
                                                }

                                                // Console.WriteLine($"{titulo}: {valor}");
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.LogError(ex, ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                            //else
                            //{
                            //    processado = 2; // Não coletar novamente
                            //}
                        }
                    }
                    catch (BusinessException)
                    {
                        //processado = 3; // Verificar
                        processado = false;
                    }
                    catch (Exception ex)
                    {
                        processado = false;
                        logger.LogError(ex, ex.Message);
                    }

                    localDbContext.DeputadosFederais.Where(x => x.Id == idDeputado)
                         .ExecuteUpdate(setters => setters
                            .SetProperty(x => x.PassaporteDiplomatico, possuiPassaporteDiplimatico)
                            .SetProperty(x => x.SecretariosAtivos, (short)qtdSecretarios)
                            .SetProperty(x => x.Processado, processado)
                        );

                    localDbContext.SaveChanges();
                }
            }
        }

        private async Task ColetaSalarioDeputado(IBrowsingContext context, int idDeputado, int ano, AppDbContext dbContext)
        {
            var salarios = dbContext.DeputadoRemuneracoes
                .AsNoTracking()
                .Where(x => x.IdDeputado == idDeputado && x.Ano == ano)
                .ToList();

            var address = $"https://www.camara.leg.br/deputados/{idDeputado}/remuneracao?ano={ano}";
            var document = await context.OpenAsyncAutoRetry(address);
            if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
            {
                logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
                return;
            }

            var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
            if (tabelas.Length == 0)
            {
                logger.LogError("Nenhuma tabela encontrada! Url: {Url}", address);
                return;
            }

            foreach (var tabela in tabelas)
            {
                var linhas = tabela.QuerySelectorAll("tbody tr");
                if (linhas.Length == 0)
                {
                    logger.LogError("Nenhuma linha encontrada! Url: {Url}", address);
                    return;
                }

                foreach (var linha in linhas)
                {
                    var colunas = linha.QuerySelectorAll("td");

                    var mes = Convert.ToInt16(colunas[0].TextContent.Trim());
                    var valor = Convert.ToDecimal(colunas[1].TextContent.Trim(), cultureInfo);
                    var link = (colunas[1].FirstElementChild as IHtmlAnchorElement).Href; // todo?

                    if (salarios.Any(x => x.Mes == mes))
                    {
                        logger.LogDebug("Remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao} já existe, ignorando", mes, ano, idDeputado);
                        await ColetaSalarioDeputadoDetalhado(context, idDeputado, ano, mes, dbContext);
                        continue;
                    }

                    // Não considera gratificação natalina e outros adicionais
                    var salario = new DeputadoRemuneracao()
                    {
                        IdDeputado = (int)idDeputado,
                        Ano = (short)ano,
                        Mes = mes,
                        Valor = valor
                    };

                    try
                    {
                        dbContext.DeputadoRemuneracoes.Add(salario);
                        logger.LogDebug("Inserida remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes, ano, idDeputado);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Duplicate") && !ex.Message.Contains("Unable to cast object of type 'System.UInt32' to type 'System.Nullable`1[System.Int32]"))
                            throw;
                        else
                            logger.LogWarning("Registro duplicado ignorado: {@Salario}", salario);
                    }

                    await ColetaSalarioDeputadoDetalhado(context, idDeputado, ano, mes, dbContext);
                }
            }

            dbContext.SaveChanges();
        }

        private async Task ColetaSalarioDeputadoDetalhado(IBrowsingContext context, int idDeputado, int ano, short mes, AppDbContext dbContext)
        {
            var address = $"https://www.camara.leg.br/deputados/{idDeputado}/remuneracao-deputado-detalhado?mesAno={mes:00}{ano}";
            var document = await context.OpenAsyncAutoRetry(address);
            if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
            {
                logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
                return;
            }

            var folhasPagamento = document.QuerySelectorAll(".container--pagina-interna table");
            if (!folhasPagamento.Any())
            {
                logger.LogWarning("Dados indisponiveis: " + address);
                return; // Erro no funcionario, abortar coleta dele
            }

            string categoriaFuncional = null, dataExercicio = null, cargo = null;
            foreach (var folhaPagamento in folhasPagamento)
            {
                if (folhaPagamento.Attributes.GetNamedItem("summary")?.TextContent == "Dados do Servidor")
                {
                    var colunas = folhaPagamento.QuerySelectorAll("tr td");
                    categoriaFuncional = colunas[0].TextContent.Split(":")[1].Trim();
                    dataExercicio = colunas[1].TextContent.Split(":")[1].Trim();
                    cargo = colunas[2].TextContent.Split(":")[1].Trim();

                    continue; // Ignorar dados do servidor, não tem informações de remuneração
                }
                    

                var titulo = folhaPagamento.QuerySelector("caption").TextContent.Split("\n")?[1]?.Trim();

                using (logger.BeginScope(new Dictionary<string, object> { ["Folha"] = titulo }))
                {
                    if (titulo.Split("-")[0].Trim() != $"{mes:00}/{ano}") // todo:
                    {
                        throw new NotImplementedException("Algo esta errado!");
                    }

                    var folha = new DeputadoFederalRemuneracaoDetalhes();
                    IEnumerable<PropertyInfo> props = folha.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    var linhas = folhaPagamento.QuerySelectorAll("tbody tr");
                    foreach (var linha in linhas)
                    {
                        var colunas = linha.QuerySelectorAll("td");
                        if (colunas.Count() != 2) continue;

                        var descricao = colunas[0].TextContent.Trim();
                        var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

                        var propInfo = folha.GetType().GetProperty(colunasBanco[descricao]);
                        propInfo.SetValue(folha, valor, null);
                    }

                    short tipo = 0;
                    switch (titulo.Split("-")[1].Trim()) // TODO
                    {
                        case "FOLHA NORMAL":
                            tipo = 1;
                            break;
                        case "FOLHA COMPLEMENTAR":
                            tipo = 2;
                            break;
                        case "FOLHA COMPLEMENTAR ESPECIAL":
                            tipo = 3;
                            break;
                        case "FOLHA DE ADIANTAMENTO GRATIFICAÇÃO NATALINA":
                            tipo = 4;
                            break;
                        case "FOLHA DE GRATIFICAÇÃO NATALINA":
                            tipo = 5;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    folha.IdDeputado = idDeputado;
                    folha.Tipo = tipo;
                    folha.Referencia = new DateTime(ano, mes, 1);

                    if (!string.IsNullOrEmpty(dataExercicio))
                        folha.Contratacao = DateOnly.Parse(dataExercicio, cultureInfo);

                    folha.ValorBruto =
                        folha.RemuneracaoFixa +
                        folha.VantagensNaturezaPessoal +
                        folha.FuncaoOuCargoEmComissao +
                        folha.GratificacaoNatalina +
                        folha.Ferias +
                        folha.OutrasRemuneracoes +
                        folha.AbonoPermanencia;

                    var dataContratacao = Convert.ToDateTime(dataExercicio, cultureInfo);
                    folha.ValorOutros = folha.ValorDiarias + folha.ValorAuxilios + folha.ValorVantagens;
                    folha.ValorTotal = folha.ValorBruto + folha.ValorOutros;

                    try
                    {
                        dbContext.DeputadoFederalRemuneracaoDetalhes.Add(folha);

                        logger.LogDebug("Inserida remuneração Detalhada de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes, ano, titulo);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Duplicate"))
                            throw;
                        else
                            logger.LogWarning("Registro duplicado ignorado: {@Folha}", folha);
                    }
                }
            }

            dbContext.SaveChanges();
        }

        private async Task ColetaPessoalGabinete(IBrowsingContext context, int idDeputado, int ano, AppDbContext dbContext)
        {
            var address = $"https://www.camara.leg.br/deputados/{idDeputado}/pessoal-gabinete?ano={ano}";
            var document = await context.OpenAsyncAutoRetry(address);
            if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
            {
                logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
                return;
            }

            var deputado = document.QuerySelector(".titulo-internal").TextContent;
            var anoColeta = document.QuerySelector(".subtitulo-internal").TextContent;
            logger.LogDebug("{Ano} para o deputado {Deputado}", anoColeta, deputado);

            var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
            if (tabelas.Length == 0)
            {
                logger.LogError("Nenhuma tabela encontrada! Url: {Url}", address);
                return;
            }

            foreach (var tabela in tabelas)
            {
                var linhas = tabela.QuerySelectorAll("tbody tr");
                if (linhas.Length == 0)
                {
                    logger.LogError("Nenhuma linha encontrada! Url: {Url}", address);
                    return;
                }

                foreach (var linha in linhas)
                {
                    var colunas = linha.QuerySelectorAll("td");

                    var link = (colunas[4].FirstElementChild as IHtmlAnchorElement).Href; // todo?
                    var chave = link.Replace("https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/", "");

                    // Check if already tracked or in database to avoid duplicate key error
                    if (dbContext.CamaraFederalDeputadoFuncionarioTemps.Local.Any(x => x.Chave == chave) ||
                        dbContext.CamaraFederalDeputadoFuncionarioTemps.Any(x => x.Chave == chave))
                    {
                        continue;
                    }

                    var periodo = colunas[3].TextContent.Trim();
                    var periodoPartes = periodo.Split(" ");
                    DateTime dataInicial = Convert.ToDateTime(periodoPartes[1]);
                    DateTime? dataFinal = periodoPartes.Length == 4 ? Convert.ToDateTime(periodoPartes[3]) : null;

                    var funcionario = new CamaraFederalDeputadoFuncionarioTemp()
                    {
                        IdDeputado = idDeputado,
                        Chave = chave,
                        Nome = colunas[0].TextContent.Trim(),
                        GrupoFuncional = colunas[1].TextContent.Trim(),
                        Nivel = colunas[2].TextContent.Trim(),
                        PeriodoDe = dataInicial,
                        PeriodoAte = dataFinal,
                    };

                    dbContext.CamaraFederalDeputadoFuncionarioTemps.Add(funcionario);
                }
            }

            dbContext.SaveChanges();
        }

        //    //    private async Task ColetaPessoalGabinete(IBrowsingContext context, int idDeputado, int ano)
        //    //    {
        //    //        var dataControle = DateTime.Today.ToString("yyyy-MM-dd");
        //    //        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/pessoal-gabinete?ano={ano}";
        //    //        var document = await context.OpenAsyncAutoRetry(address);
        //    //        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        //    //        {
        //    //            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
        //    //            return;
        //    //        };

        //    //        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        //    //        foreach (var tabela in tabelas)
        //    //        {
        //    //            var linhas = tabela.QuerySelectorAll("tbody tr");
        //    //            foreach (var linha in linhas)
        //    //            {
        //    //                int idSecretario;
        //    //                var colunas = linha.QuerySelectorAll("td");
        //    //                var nome = colunas[0].TextContent.Trim();
        //    //                var grupo = colunas[1].TextContent.Trim();
        //    //                var cargo = colunas[2].TextContent.Trim();
        //    //                var periodo = colunas[3].TextContent.Trim();
        //    //                var link = (colunas[4].FirstElementChild as IHtmlAnchorElement).Href; // todo?
        //    //                var chave = link.Replace("https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/", "");

        //    //                var periodoPartes = periodo.Split(" ");
        //    //                DateTime? dataInicial = Convert.ToDateTime(periodoPartes[1]);
        //    //                DateTime? dataFinal = periodoPartes.Length == 4 ? Convert.ToDateTime(periodoPartes[3]) : null;

        //    //                var sqlSelectSecretario = "select id FROM camara.cf_funcionario where chave = @chave";

        //    //                var objId = ExecuteScalar(sqlSelectSecretario);
        //    //                if (objId is not null)
        //    //                {
        //    //                    idSecretario = Convert.ToInt32(objId);


        //    //                    connection.Execute($"UPDATE cf_funcionario SET controle='{dataControle}', processado=0 WHERE id=@id;");
        //    //                }
        //    //                else
        //    //                {
        //    //                    var sqlInsertSecretario = "insert into cf_funcionario (chave, nome, processado, controle) values (@chave, @nome, 0, @controle);";



        //    //                    connection.Execute(sqlInsertSecretario);

        //    //                    idSecretario = Convert.ToInt32(ExecuteScalar("select LAST_INSERT_ID()"));
        //    //                }

        //    //                var sqlInsertSecretarioContratacao = @"
        //    //insert IGNORE into cf_funcionario_contratacao (id_cf_funcionario, id_cf_deputado, id_cf_funcionario_grupo_funcional, id_cf_funcionario_nivel, periodo_de, periodo_ate) 
        //    //values (@id_cf_funcionario, @id_cf_deputado, (select id FROM camara.cf_funcionario_grupo_funcional where nome ILIKE @grupo), (select id FROM camara.cf_funcionario_nivel where nome ILIKE @nivel), @periodo_de, @periodo_ate)
        //    //-- ON DUPLICATE KEY UPDATE periodo_ate = @periodo_ate";








        //    //                connection.Execute(sqlInsertSecretarioContratacao);

        //    //                //if (objId is not null && (dataFinal == null || dataFinal >= new DateTime(2021, 6, 1)))
        //    //                //{
        //    //                //    var sqlUpdateSecretario = "UPDATE cf_funcionario set processado = 0 where id = @idSecretario;";

        //    //                //    connection.Execute(sqlUpdateSecretario);
        //    //                //}
        //    //            }
        //    //        }
        //    //    }

        private async Task ColetaAuxilioMoradia(IBrowsingContext context, int idDeputado, int ano, AppDbContext dbContext)
        {
            var auxilios = dbContext.DeputadoAuxilioMoradias
                .AsNoTracking()
                .Where(x => x.IdDeputado == idDeputado && x.Ano == ano)
                .ToList();

            var address = $"https://www.camara.leg.br/deputados/{idDeputado}/auxilio-moradia/?ano={ano}";
            var document = await context.OpenAsyncAutoRetry(address);
            if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
            {
                Console.WriteLine(document.StatusCode + ":" + document.Url);
                return;
            }

            var novosAuxilios = new List<DeputadoAuxilioMoradia>();
            var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
            foreach (var tabela in tabelas)
            {
                var linhas = tabela.QuerySelectorAll("tbody tr");
                foreach (var linha in linhas)
                {
                    var colunas = linha.QuerySelectorAll("td");
                    var mes = Convert.ToInt32(colunas[0].TextContent.Trim());
                    var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

                    if (auxilios.Any(x => x.Mes == mes))
                        continue;

                    // https://www.camara.leg.br/deputados/143942/auxilio-moradia/?ano=2025
                    // Há meses com mais de 1 valor, vamos soma-los
                    if (novosAuxilios.Any(x => x.Mes == mes))
                    {
                        var auxilioExistente = novosAuxilios.First(x => x.Mes == mes);
                        auxilioExistente.Valor += valor;
                        continue;
                    }

                    novosAuxilios.Add(new DeputadoAuxilioMoradia()
                    {
                        IdDeputado = idDeputado,
                        Ano = (short)ano,
                        Mes = (short)mes,
                        Valor = valor
                    });
                }
            }

            dbContext.DeputadoAuxilioMoradias.AddRange(novosAuxilios);
            dbContext.SaveChanges();
        }

        private async Task ColetaMissaoOficial(IBrowsingContext context, int idDeputado, int ano, AppDbContext dbContext)
        {
            var missoes = dbContext.DeputadoMissoesOficiais
                .AsNoTracking()
                .Where(x => x.IdDeputado == idDeputado)
                .ToList();

            var address = $"https://www.camara.leg.br/deputados/{idDeputado}/missao-oficial/?ano={ano}";
            var document = await context.OpenAsyncAutoRetry(address);
            if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
            {
                Console.WriteLine(document.StatusCode + ":" + document.Url);
                return;
            }

            var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
            foreach (var tabela in tabelas)
            {
                var linhas = tabela.QuerySelectorAll("tbody tr");
                foreach (var linha in linhas)
                {
                    var colunas = linha.QuerySelectorAll("td");
                    var periodo = colunas[0].TextContent.Trim();
                    var assunto = colunas[1].TextContent.Trim();
                    var destino = colunas[2].TextContent.Trim();
                    var passagens = Convert.ToDecimal(colunas[3].TextContent.Trim());
                    var diarias = Convert.ToDecimal(colunas[4].TextContent.Trim());
                    var relatorio = (colunas[5].FirstElementChild as IHtmlAnchorElement)?.Href;

                    if (missoes.Any(x => x.Periodo == periodo))
                        continue;

                    // Temos linhas duplicadas na Câmara: https://www.camara.leg.br/deputados/164360/missao-oficial/?ano=2021
                    if (dbContext.DeputadoMissoesOficiais.Local.Any(x => x.IdDeputado == idDeputado && x.Periodo == periodo))
                        continue;

                    dbContext.DeputadoMissoesOficiais.Add(new DeputadoMissaoOficial()
                    {
                        IdDeputado = idDeputado,
                        Periodo = periodo,
                        Assunto = assunto,
                        Destino = destino,
                        Relatorio = relatorio,
                        Passagens = passagens,
                        Diarias = diarias
                    });
                }
            }

            dbContext.SaveChanges();
        }

        private Dictionary<string, string> colunasBanco = new Dictionary<string, string>();
        public void DefineColunasRemuneracaoSecretarios()
        {
            colunasBanco = new Dictionary<string, string>();
            // 1 - Remuneração Básica
            colunasBanco.Add("a - Remuneração Fixa", nameof(DeputadoFederalRemuneracaoDetalhes.RemuneracaoFixa));
            colunasBanco.Add("b - Vantagens de Natureza Pessoal", nameof(DeputadoFederalRemuneracaoDetalhes.VantagensNaturezaPessoal));
            // 2 - Remuneração Eventual/Provisória
            colunasBanco.Add("a - Função ou Cargo em Comissão", nameof(DeputadoFederalRemuneracaoDetalhes.FuncaoOuCargoEmComissao));
            colunasBanco.Add("b - Gratificação Natalina", nameof(DeputadoFederalRemuneracaoDetalhes.GratificacaoNatalina));
            colunasBanco.Add("c - Férias (1/3 Constitucional)", nameof(DeputadoFederalRemuneracaoDetalhes.Ferias));
            colunasBanco.Add("d - Outras Remunerações Eventuais/Provisórias(*)", nameof(DeputadoFederalRemuneracaoDetalhes.OutrasRemuneracoes));
            // 3 - Abono Permanência
            colunasBanco.Add("a - Abono Permanência", nameof(DeputadoFederalRemuneracaoDetalhes.AbonoPermanencia));
            // 4 - Descontos Obrigatórios(-)
            colunasBanco.Add("a - Redutor Constitucional", nameof(DeputadoFederalRemuneracaoDetalhes.RedutorConstitucional));
            colunasBanco.Add("b - Contribuição Previdenciária", nameof(DeputadoFederalRemuneracaoDetalhes.ContribuicaoPrevidenciaria));
            colunasBanco.Add("c - Imposto de Renda", nameof(DeputadoFederalRemuneracaoDetalhes.ImpostoRenda));
            // 5 - Remuneração após Descontos Obrigatórios
            colunasBanco.Add("a - Remuneração após Descontos Obrigatórios", nameof(DeputadoFederalRemuneracaoDetalhes.ValorLiquido));
            // 6 - Outros
            colunasBanco.Add("a - Diárias", nameof(DeputadoFederalRemuneracaoDetalhes.ValorDiarias));
            colunasBanco.Add("b - Auxílios", nameof(DeputadoFederalRemuneracaoDetalhes.ValorAuxilios));
            colunasBanco.Add("c - Vantagens Indenizatórias", nameof(DeputadoFederalRemuneracaoDetalhes.ValorVantagens));
            //}
        }
    }
}
