using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.MinasGerais.Entities;
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

        public async Task ColetaDadosDeputados()
        {
            DefineColunasRemuneracaoSecretarios();
            //await dbContext.Database.GetDbConnection().ExecuteAsync("update camara.cf_deputado set processado=false");

            var sqlDeputados = @"
SELECT DISTINCT cd.id as Id, nome_parlamentar as NomeParlamentar
FROM camara.cf_deputado cd 
JOIN camara.cf_mandato m ON m.id_cf_deputado = cd.id
-- WHERE (id_legislatura IN(57) OR cd.situacao = 'Exercício')
WHERE id_legislatura IN(53,54,55,56,57)
AND processado = false";
            var dcDeputado = dbContext.Database.GetDbConnection().Query<(int Id, string NomeParlamentar)>(sqlDeputados)
                .ToList();
            ConcurrentQueue<(int Id, string NomeParlamentar)> queue =
                new ConcurrentQueue<(int Id, string NomeParlamentar)>(dcDeputado);

            Task[] tasks = new Task[NumeroThreads];
            for (int i = 0; i < NumeroThreads; ++i)
            {
                tasks[i] = ProcessaFilaColetaDadosDeputados(queue);
            }

            await Task.WhenAll(tasks);
        }

        public async Task AtualizaParlamentarValores()
        {
            await dbContext.Database.GetDbConnection().ExecuteAsync(@"
UPDATE camara.cf_deputado d
SET 
  valor_total_salario = COALESCE(s.salario, 0),
	valor_total_auxilio_moradia = COALESCE(s.auxilio, 0),
	valor_total_remuneracao = COALESCE(s.verba, s.rem_func, 0)
FROM (
	SELECT 
		d.id,
		(SELECT SUM(valor) FROM camara.cf_deputado_remuneracao WHERE id_cf_deputado = d.id) as salario,
		(SELECT SUM(valor) FROM camara.cf_deputado_auxilio_moradia WHERE id_cf_deputado = d.id) as auxilio,
		(SELECT SUM(valor) FROM camara.cf_deputado_verba_gabinete WHERE id_cf_deputado = d.id) as verba,
		(SELECT SUM(valor_total) FROM camara.cf_funcionario_remuneracao WHERE id_cf_deputado = d.id) as rem_func
	FROM camara.cf_deputado d
) s
WHERE d.id = s.id;");
        }

        private readonly string[] meses = new string[]
            { "JAN", "FEV", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OUT", "NOV", "DEZ" };

        private async Task ProcessaFilaColetaDadosDeputados(ConcurrentQueue<(int Id, string NomeParlamentar)> queue)
        {
            var anoCorte = 2008;
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

                                        var lstInfo =
                                            document.QuerySelectorAll(
                                                ".recursos-beneficios-deputado-container .beneficio");
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

                                        var verbaGabineteMensal =
                                            document.QuerySelectorAll("#gastomensalverbagabinete tbody tr");
                                        foreach (var item in verbaGabineteMensal)
                                        {
                                            var colunas = item.QuerySelectorAll("td");

                                            var mes = (short)(Array.IndexOf(meses, colunas[0].TextContent) + 1);
                                            if (verbasGabineteExistentes.Contains(mes)) continue;

                                            var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                                            var percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""),
                                                cultureInfoUS);

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

                                        var cotaParlamentarMensal =
                                            document.QuerySelectorAll("#gastomensalcotaparlamentar tbody tr");
                                        foreach (var item in cotaParlamentarMensal)
                                        {
                                            var colunas = item.QuerySelectorAll("td");

                                            var mes = (short)(Array.IndexOf(meses, colunas[0].TextContent) + 1);
                                            if (cotasParlamentaresExistentes.Contains(mes)) continue;

                                            var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                                            decimal? percentual = null;
                                            if (!string.IsNullOrEmpty(colunas[2].TextContent.Replace("%", "")))
                                                percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""),
                                                    cultureInfoUS);

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
                                                var titulo = info.QuerySelector(".beneficio__titulo")?.TextContent
                                                    ?.Replace(" ?", "")?.Trim();
                                                var valor = info.QuerySelector(".beneficio__info")?.TextContent
                                                    ?.Trim(); //Replace \n

                                                switch (titulo)
                                                {
                                                    case "Pessoal de gabinete":
                                                        if (valor != "0 pessoas")
                                                        {
                                                            if ((ano == DateTime.Today.Year))
                                                                qtdSecretarios = Convert.ToInt32(valor.Split(' ')[0]);

                                                            await ColetaPessoalGabinete(context, idDeputado, ano,
                                                                localDbContext);
                                                        }

                                                        break;
                                                    case "Salário mensal bruto":
                                                        // Nada: Deve Consultar por ano
                                                        break;
                                                    case "Imóvel funcional":
                                                        if (!valor.StartsWith("Não fez") &&
                                                            !valor.StartsWith("Não faz") &&
                                                            !valor.StartsWith("Não há dado"))
                                                        {
                                                            var lstImovelFuncional = valor.Split("\n");

                                                            var imoveisExistentes = localDbContext
                                                                .DeputadoImoveisFuncionais
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
                                                                    dias = (dataFinal.Value.ToDateTime(
                                                                                TimeOnly.MinValue) -
                                                                            dataInicial.ToDateTime(TimeOnly.MinValue))
                                                                    .Days;
                                                                }

                                                                localDbContext.DeputadoImoveisFuncionais.Add(
                                                                    new DeputadoImovelFuncional()
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
                                                            await ColetaAuxilioMoradia(context, idDeputado, ano,
                                                                localDbContext);
                                                        }

                                                        break;
                                                    case "Viagens em missão oficial":
                                                        if (valor != "0")
                                                        {
                                                            await ColetaMissaoOficial(context, idDeputado, ano,
                                                                localDbContext);
                                                        }

                                                        break;
                                                    case "Passaporte diplomático":
                                                        if (valor != "Não possui")
                                                        {
                                                            possuiPassaporteDiplimatico = true;
                                                        }

                                                        break;
                                                    default:
                                                        throw new NotImplementedException(
                                                            $"Vefificar beneficios do parlamentar {idDeputado} para o ano {ano}.");
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
                    catch (BusinessException ex)
                    {
                        //processado = 3; // Verificar
                        processado = false;
                        logger.LogWarning(ex, ex.Message);
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

        private async Task ColetaSalarioDeputado(IBrowsingContext context, int idDeputado, int ano,
            AppDbContext dbContext)
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
                        logger.LogDebug("Remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao} já existe, ignorando",
                            mes, ano, idDeputado);
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
                        logger.LogDebug("Inserida remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes, ano,
                            idDeputado);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Duplicate") && !ex.Message.Contains(
                                "Unable to cast object of type 'System.UInt32' to type 'System.Nullable`1[System.Int32]"))
                            throw;
                        else
                            logger.LogWarning("Registro duplicado ignorado: {@Salario}", salario);
                    }

                    await ColetaSalarioDeputadoDetalhado(context, idDeputado, ano, mes, dbContext);
                }
            }

            dbContext.SaveChanges();
        }

        private async Task ColetaSalarioDeputadoDetalhado(IBrowsingContext context, int idDeputado, int ano, short mes,
            AppDbContext dbContext)
        {
            var address =
                $"https://www.camara.leg.br/deputados/{idDeputado}/remuneracao-deputado-detalhado?mesAno={mes:00}{ano}";
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
                    IEnumerable<PropertyInfo> props =
                        folha.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

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

                    folha.ValorOutros = folha.ValorDiarias + folha.ValorAuxilios + folha.ValorVantagens;
                    folha.ValorTotal = folha.ValorBruto + folha.ValorOutros;

                    try
                    {
                        dbContext.DeputadoFederalRemuneracaoDetalhes.Add(folha);

                        logger.LogDebug("Inserida remuneração Detalhada de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes,
                            ano, titulo);
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

        private async Task ColetaPessoalGabinete(IBrowsingContext context, int idDeputado, int ano,
            AppDbContext dbContext)
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
                    var chave = link.Replace("https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/",
                        "");

                    // Check if already tracked or in database to avoid duplicate key error
                    if (dbContext.CamaraFederalDeputadoFuncionarioTemps.Local.Any(x => x.Chave == chave) ||
                        dbContext.CamaraFederalDeputadoFuncionarioTemps.Any(x => x.Chave == chave))
                    {
                        continue;
                    }

                    var periodo = colunas[3].TextContent.Trim();
                    var periodoPartes = periodo.Split(" ");
                    DateOnly dataInicial = DateOnly.Parse(periodoPartes[1]);
                    DateOnly? dataFinal = periodoPartes.Length == 4 ? DateOnly.Parse(periodoPartes[3]) : null;

                    var funcionario = new CamaraFederalDeputadoFuncionarioTemp()
                    {
                        IdDeputado = idDeputado,
                        Chave = chave,
                        Nome = colunas[0].TextContent.ToTitleCase().Trim(),
                        GrupoFuncional = colunas[1].TextContent.ToTitleCase().Trim(),
                        Nivel = colunas[2].TextContent.Trim(), // Cargo
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

        private async Task ColetaAuxilioMoradia(IBrowsingContext context, int idDeputado, int ano,
            AppDbContext dbContext)
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

        private async Task ColetaMissaoOficial(IBrowsingContext context, int idDeputado, int ano,
            AppDbContext dbContext)
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
                    if (dbContext.DeputadoMissoesOficiais.Local.Any(x =>
                            x.IdDeputado == idDeputado && x.Periodo == periodo))
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
            colunasBanco.Add("b - Vantagens de Natureza Pessoal",
                nameof(DeputadoFederalRemuneracaoDetalhes.VantagensNaturezaPessoal));
            // 2 - Remuneração Eventual/Provisória
            colunasBanco.Add("a - Função ou Cargo em Comissão",
                nameof(DeputadoFederalRemuneracaoDetalhes.FuncaoOuCargoEmComissao));
            colunasBanco.Add("b - Gratificação Natalina",
                nameof(DeputadoFederalRemuneracaoDetalhes.GratificacaoNatalina));
            colunasBanco.Add("c - Férias (1/3 Constitucional)", nameof(DeputadoFederalRemuneracaoDetalhes.Ferias));
            colunasBanco.Add("d - Outras Remunerações Eventuais/Provisórias(*)",
                nameof(DeputadoFederalRemuneracaoDetalhes.OutrasRemuneracoes));
            // 3 - Abono Permanência
            colunasBanco.Add("a - Abono Permanência", nameof(DeputadoFederalRemuneracaoDetalhes.AbonoPermanencia));
            // 4 - Descontos Obrigatórios(-)
            colunasBanco.Add("a - Redutor Constitucional",
                nameof(DeputadoFederalRemuneracaoDetalhes.RedutorConstitucional));
            colunasBanco.Add("b - Contribuição Previdenciária",
                nameof(DeputadoFederalRemuneracaoDetalhes.ContribuicaoPrevidenciaria));
            colunasBanco.Add("c - Imposto de Renda", nameof(DeputadoFederalRemuneracaoDetalhes.ImpostoRenda));
            // 5 - Remuneração após Descontos Obrigatórios
            colunasBanco.Add("a - Remuneração após Descontos Obrigatórios",
                nameof(DeputadoFederalRemuneracaoDetalhes.ValorLiquido));
            // 6 - Outros
            colunasBanco.Add("a - Diárias", nameof(DeputadoFederalRemuneracaoDetalhes.ValorDiarias));
            colunasBanco.Add("b - Auxílios", nameof(DeputadoFederalRemuneracaoDetalhes.ValorAuxilios));
            colunasBanco.Add("c - Vantagens Indenizatórias", nameof(DeputadoFederalRemuneracaoDetalhes.ValorVantagens));
            //}
        }

        /// <summary>
        /// Processa funcionários temporários usando o novo fluxo de matching em 2 fases
        /// </summary>
        public async Task ProcessarFuncionarioTemp()
        {
            var dataColeta = DateOnly.FromDateTime(DateTime.Today);

            logger.LogInformation("=== INICIANDO PROCESSAMENTO DE FUNCIONÁRIOS ===");
            logger.LogInformation("Data de coleta: {DataColeta}", dataColeta);

            // FASE 2: Calcular matches
            logger.LogInformation("=== FASE 2: CALCULAR MATCHES ===");
            var totalMatches = await CalcularMatches(dataColeta);

            // FASE 3/4: Processar todos os registros (unificado)
            logger.LogInformation("=== FASE 3/4: PROCESSAR TODOS OS REGISTROS ===");
            var totalFuncionarios = await ProcessarTodosRegistros();

            logger.LogInformation("=== PROCESSAMENTO CONCLUÍDO ===");
            logger.LogInformation("Total matches calculados: {TotalMatches}", totalMatches);
            logger.LogInformation("Total funcionários processados: {TotalFuncionarios}", totalFuncionarios);
        }

        #region Matching de Funcionários - Métodos Auxiliares

        /// <summary>
        /// Compara nomes ignorando acentos, maiúsculas/minúsculas, espaços e hífens
        /// </summary>
        private static bool NomesSaoIguais(string nome1, string nome2)
        {
            var normalizar = (string nome) =>
                nome.Normalize(NormalizationForm.FormD)
                    .Replace("-", "")
                    .Replace(" ", "")
                    .ToLowerInvariant();

            return normalizar(nome1) == normalizar(nome2);
        }

        /// <summary>
        /// Verifica se dois períodos são compatíveis (overlap ou adjacência)
        /// </summary>
        private static bool PeriodosSaoCompativeis(DateOnly? de1, DateOnly? ate1, DateOnly? de2, DateOnly? ate2)
        {
            if (!de1.HasValue || !de2.HasValue)
                return false;

            // Verifica se NÃO há overlap
            if (ate1.HasValue && de2.Value > ate1.Value)
                return false;

            if (ate2.HasValue && de1.Value > ate2.Value)
                return false;

            // Períodos se sobrepõem ou são adjacentes
            return true;
        }

        #endregion

        #region Matching de Funcionários - Fase 2: Calcular Matches

        /// <summary>
        /// FASE 2: Realiza o matching e salva APENAS matches na tabela temp
        /// </summary>
        public async Task<int> CalcularMatches(DateOnly dataColeta)
        {
            // Carrega funcionários com histórico do banco final
            var funcionariosComContratacoes = dbContext.FuncionariosCamaraFederal
                .AsNoTracking()
                .Include(f => f.FuncionarioContratacoes)
                .Where(f => f.FuncionarioContratacoes.Any())
                .ToList();

            // Carrega registros temporários (dados brutos)
            var registrosTemp = await dbContext.CamaraFederalDeputadoFuncionarioTemps
                .AsNoTracking()
                .ToListAsync();

            // Agrupa por chave para lookup rápido
            var tempAgrupadoPorChave = registrosTemp
                .GroupBy(t => t.Chave)
                .ToDictionary(g => g.Key, g => g.ToList());

            var registrosProcessados = new HashSet<string>();
            var matchesParaSalvar = new List<CamaraFederalFuncionarioMatchTemp>();

            logger.LogInformation(
                "Calculando matches: {TotalFuncionarios} funcionários, {TotalTemp} registros temp",
                funcionariosComContratacoes.Count, registrosTemp.Count);

            // ==================== PASSO 1: Match Exato por Chave ====================
            logger.LogInformation("Passo 1: Match exato por chave...");

            foreach (var funcionario in funcionariosComContratacoes)
            {
                if (tempAgrupadoPorChave.TryGetValue(funcionario.Chave, out var registrosMatch))
                {
                    foreach (var registro in registrosMatch)
                    {
                        if (registrosProcessados.Contains(registro.Chave))
                            continue;

                        matchesParaSalvar.Add(new CamaraFederalFuncionarioMatchTemp
                        {
                            ChaveOrigem = registro.Chave,
                            ChaveExistente = funcionario.Chave,
                            Nome = registro.Nome,
                            MatchRegra = (int)MatchRegra.ChaveExata,
                            DataColeta = dataColeta
                        });

                        registrosProcessados.Add(registro.Chave);
                    }
                }
            }

            // ==================== PASSO 2: Match por Período + Deputado + Nome ====================
            logger.LogInformation("Passo 2: Match por período+deputado+nome...");

            foreach (var funcionario in funcionariosComContratacoes)
            {
                var registrosMesmoNome = registrosTemp
                    .Where(t => !registrosProcessados.Contains(t.Chave) &&
                                NomesSaoIguais(t.Nome, funcionario.Nome))
                    .ToList();

                if (!registrosMesmoNome.Any())
                    continue;

                foreach (var registro in registrosMesmoNome)
                {
                    var contratacaoMatch = funcionario.FuncionarioContratacoes
                        .FirstOrDefault(c =>
                            c.IdDeputado == registro.IdDeputado &&
                            PeriodosSaoCompativeis(
                                c.PeriodoDe, c.PeriodoAte,
                                registro.PeriodoDe, registro.PeriodoAte));

                    if (contratacaoMatch != null)
                    {
                        matchesParaSalvar.Add(new CamaraFederalFuncionarioMatchTemp
                        {
                            ChaveOrigem = registro.Chave,
                            ChaveExistente = funcionario.Chave,
                            Nome = registro.Nome,
                            MatchRegra = (int)MatchRegra.PeriodoDeputadoNome,
                            DataColeta = dataColeta
                        });

                        registrosProcessados.Add(registro.Chave);
                    }
                }
            }

            // ==================== PASSO 3: Match por Histórico ====================
            logger.LogInformation("Passo 3: Match por histórico (nome+deputado)...");

            foreach (var funcionario in funcionariosComContratacoes)
            {
                var registrosMesmoNome = registrosTemp
                    .Where(t => !registrosProcessados.Contains(t.Chave) &&
                                NomesSaoIguais(t.Nome, funcionario.Nome))
                    .ToList();

                if (!registrosMesmoNome.Any())
                    continue;

                foreach (var registro in registrosMesmoNome)
                {
                    var historicoMatch = funcionario.FuncionarioContratacoes
                        .Any(c => c.IdDeputado == registro.IdDeputado);

                    if (historicoMatch)
                    {
                        matchesParaSalvar.Add(new CamaraFederalFuncionarioMatchTemp
                        {
                            ChaveOrigem = registro.Chave,
                            ChaveExistente = funcionario.Chave,
                            Nome = registro.Nome,
                            MatchRegra = (int)MatchRegra.HistoricoNomeDeputado,
                            DataColeta = dataColeta
                        });

                        registrosProcessados.Add(registro.Chave);
                    }
                }
            }

            // Salvar todos os matches
            var bulkService = new BulkInsertService<CamaraFederalFuncionarioMatchTemp>();
            bulkService.BulkInsertNoTracking(dbContext, matchesParaSalvar);

            logger.LogInformation(
                "Matching concluído: {TotalMatches} matches ({R1} exato, {R2} período, {R3} histórico)",
                matchesParaSalvar.Count,
                matchesParaSalvar.Count(m => m.MatchRegra == 1),
                matchesParaSalvar.Count(m => m.MatchRegra == 2),
                matchesParaSalvar.Count(m => m.MatchRegra == 3));

            return matchesParaSalvar.Count;
        }

        #endregion

        #region Matching de Funcionários - Fase 3/4 Unificada: Processar Todos

        /// <summary>
        /// FASE UNIFICADA 3/4: Processa todos os registros (com e sem match)
        /// </summary>
        public async Task<int> ProcessarTodosRegistros()
        {
            var dataColeta = DateOnly.FromDateTime(DateTime.Today);

            // ==============================================================
            // PASSO 1: Carregar dados
            // ==============================================================
            logger.LogInformation("Carregando dados para processamento...");

            // Carrega todos os registros da tabela temp
            var registrosTemp = await dbContext.CamaraFederalDeputadoFuncionarioTemps
                .ToListAsync();

            // Carrega todos os matches
            var matches = await dbContext.CamaraFederalFuncionarioMatchTemps
                .Where(m => m.DataColeta == dataColeta)
                .ToListAsync();

            // Cria lookup para acesso rápido: chave_origem → match
            var matchesPorChaveOrigem = matches
                .ToDictionary(m => m.ChaveOrigem, m => m);

            logger.LogInformation(
                "Carregados: {TotalTemp} registros temp, {TotalMatches} matches",
                registrosTemp.Count, matches.Count);

            // ==============================================================
            // PASSO 2: Unificar e agrupar por chave
            // ==============================================================
            logger.LogInformation("Unificando registros por chave...");

            var registrosUnificados = registrosTemp
                .Select(t =>
                {
                    // Se tem match, usa chave_existente; senão, usa chave_origem
                    var chaveUnificada = matchesPorChaveOrigem.TryGetValue(t.Chave, out var match)
                        ? match.ChaveExistente
                        : t.Chave;

                    return new
                    {
                        RegistroTemp = t,
                        Match = match,
                        ChaveUnificada = chaveUnificada
                    };
                })
                .GroupBy(r => r.ChaveUnificada)
                .ToList();

            logger.LogInformation(
                "Agrupados: {TotalGrupos} grupos únicos",
                registrosUnificados.Count);

            // ==============================================================
            // PASSO 3: Processar cada grupo
            // ==============================================================
            var gruposFuncionais = await dbContext.FuncionarioGruposFuncionais.ToListAsync();
            var niveis = await dbContext.FuncionarioNiveis.ToListAsync();

            int totalFuncionariosProcessados = 0;
            int totalFuncionariosCriados = 0;
            int totalContratacoesCriadas = 0;
            int totalContratacoesAtualizadas = 0;
            int totalErros = 0;

            foreach (var grupo in registrosUnificados)
            {
                var chave = grupo.Key;
                var primeiroRegistro = grupo.First();
                var match = primeiroRegistro.Match;

                try
                {
                    // ----------------------------------------------------------
                    // 3.1: Localizar ou criar funcionário
                    // ----------------------------------------------------------
                    var funcionario = dbContext.FuncionariosCamaraFederal
                        .Include(f => f.FuncionarioContratacoes.OrderBy(c => c.PeriodoDe))
                        .FirstOrDefault(f => f.Chave == chave);

                    if (funcionario == null)
                    {
                        // CRIAR NOVO FUNCIONÁRIO
                        funcionario = new Funcionario
                        {
                            Nome = primeiroRegistro.RegistroTemp.Nome,
                            Chave = chave,
                            Processado = false
                        };

                        dbContext.FuncionariosCamaraFederal.Add(funcionario);
                        await dbContext.SaveChangesAsync(); // Para gerar o Id

                        totalFuncionariosCriados++;

                        logger.LogDebug(
                            "Funcionário criado: {Nome} ({Chave})",
                            funcionario.Nome, funcionario.Chave);
                    }
                    else
                    {
                        totalFuncionariosProcessados++;

                        logger.LogDebug(
                            "Funcionário encontrado: {Nome} ({Chave})",
                            funcionario.Nome, funcionario.Chave);
                    }

                    // ----------------------------------------------------------
                    // 3.2: Processar cada contratação do grupo
                    // ----------------------------------------------------------
                    foreach (var item in grupo)
                    {
                        var registro = item.RegistroTemp;

                        // Verificar overlap antes de criar/atualizar
                        var conflito = funcionario.FuncionarioContratacoes
                            .FirstOrDefault(c =>
                                c.IdDeputado == registro.IdDeputado &&
                                (c.PeriodoDe, c.PeriodoAte).OverlapsWith(
                                    registro.PeriodoDe, registro.PeriodoAte));

                        if (conflito != null)
                        {
                            if (conflito.PeriodoDe == registro.PeriodoDe)
                            {
                                if (conflito.PeriodoAte == null)
                                {
                                    conflito.PeriodoAte = registro.PeriodoAte;
                                }
                                else if(conflito.PeriodoAte != registro.PeriodoAte)
                                {
                                    logger.LogWarning(
                                        "Overlap detectado #1: {Nome} ({Chave}) já tem contratação com Deputado {Deputado} no período ({De} - {Ate}). Ignorando registro ({De2} - {Ate2}).",
                                        funcionario.Nome, funcionario.Chave,
                                        registro.IdDeputado,
                                        conflito.PeriodoDe, conflito.PeriodoAte,
                                        registro.PeriodoDe, registro.PeriodoAte);
                                    continue;
                                }
                            }
                            else
                            {
                                logger.LogWarning(
                                    "Overlap detectado #2: {Nome} ({Chave}) já tem contratação com Deputado {Deputado} no período ({De} - {Ate}). Ignorando registro ({De2} - {Ate2}).",
                                    funcionario.Nome, funcionario.Chave,
                                    registro.IdDeputado,
                                    conflito.PeriodoDe, conflito.PeriodoAte,
                                    registro.PeriodoDe, registro.PeriodoAte);
                                continue;
                            }
                        }

                        // Buscar contratação existente (mesmo deputado + mesmo período de início)
                        var contratacaoExistente = funcionario.FuncionarioContratacoes
                            .FirstOrDefault(c =>
                                c.IdDeputado == registro.IdDeputado &&
                                c.PeriodoDe == registro.PeriodoDe);

                        if (contratacaoExistente != null)
                        {
                            // ------------------------------------------------------
                            // ATUALIZAR contratação existente
                            // ------------------------------------------------------
                            bool atualizado = false;

                            // Atualizar PeriodoAte se necessário
                            if (registro.PeriodoAte.HasValue)
                            {
                                if (!contratacaoExistente.PeriodoAte.HasValue ||
                                    registro.PeriodoAte.Value > contratacaoExistente.PeriodoAte.Value)
                                {
                                    contratacaoExistente.PeriodoAte = registro.PeriodoAte;
                                    atualizado = true;
                                }
                            }

                            // Atualizar grupo funcional se necessário
                            if (!string.IsNullOrEmpty(registro.GrupoFuncional))
                            {
                                var grupoFuncional = gruposFuncionais
                                    .FirstOrDefault(g => g.Nome == registro.GrupoFuncional);

                                if (grupoFuncional != null &&
                                    grupoFuncional.Id != contratacaoExistente.IdGrupoFuncional)
                                {
                                    contratacaoExistente.IdGrupoFuncional = grupoFuncional.Id;
                                    atualizado = true;
                                }
                            }

                            // Atualizar nível se necessário
                            if (!string.IsNullOrEmpty(registro.Nivel))
                            {
                                var nivel = niveis
                                    .FirstOrDefault(n => n.Nome == registro.Nivel);

                                if (nivel != null &&
                                    nivel.Id != contratacaoExistente.IdNivel)
                                {
                                    contratacaoExistente.IdNivel = nivel.Id;
                                    atualizado = true;
                                }
                            }

                            if (atualizado)
                            {
                                totalContratacoesAtualizadas++;
                                logger.LogDebug(
                                    "Contratação atualizada: Funcionário {Chave}, Deputado {Deputado}, Período {De}",
                                    funcionario.Chave, registro.IdDeputado, registro.PeriodoDe);
                            }
                        }
                        else
                        {
                            // ------------------------------------------------------
                            // CRIAR NOVA CONTRATAÇÃO
                            // ------------------------------------------------------
                            var novaContratacao = new FuncionarioContratacao
                            {
                                IdFuncionario = funcionario.Id,
                                IdDeputado = registro.IdDeputado,
                                PeriodoDe = registro.PeriodoDe,
                                PeriodoAte = registro.PeriodoAte,
                                IdGrupoFuncional = gruposFuncionais
                                    .FirstOrDefault(g => g.Nome == registro.GrupoFuncional)?.Id,
                                IdNivel = niveis
                                    .FirstOrDefault(n => n.Nome == registro.Nivel)?.Id
                            };

                            dbContext.FuncionarioContratacoes.Add(novaContratacao);
                            totalContratacoesCriadas++;

                            logger.LogDebug(
                                "Contratação criada: Funcionário {Chave}, Deputado {Deputado}, Período ({De} - {Ate})",
                                funcionario.Chave, registro.IdDeputado, registro.PeriodoDe, registro.PeriodoAte);
                        }
                    }
                }
                catch (Exception ex)
                {
                    totalErros++;
                    logger.LogError(ex,
                        "Erro ao processar grupo chave {Chave}: {Message}",
                        chave, ex.Message);
                }
            }

            // ==============================================================
            // PASSO 4: Commit final
            // ==============================================================
            await dbContext.SaveChangesAsync();

            // // ==============================================================
            // // PASSO 5: Limpar tabela de matches (opcional)
            // // ==============================================================
            // var matchesProcessados = dbContext.CamaraFederalFuncionarioMatchTemps
            //     .Where(m => m.DataColeta == dataColeta)
            //     .ToList();
            //
            // if (matchesProcessados.Any())
            // {
            //     dbContext.CamaraFederalFuncionarioMatchTemps.RemoveRange(matchesProcessados);
            //     await dbContext.SaveChangesAsync();
            // }

            // ==============================================================
            // Logging final
            // ==============================================================
            logger.LogInformation(
                "Processamento concluído: " +
                "{TotalFuncionarios} funcionários ({Criados} criados, {Processados} atualizados), " +
                "{ContratacoesCriadas} contratações criadas, {ContratacoesAtualizadas} atualizadas, " +
                "{Erros} erros",
                totalFuncionariosCriados + totalFuncionariosProcessados,
                totalFuncionariosCriados,
                totalFuncionariosProcessados,
                totalContratacoesCriadas,
                totalContratacoesAtualizadas,
                totalErros);

            return totalFuncionariosCriados + totalFuncionariosProcessados;
        }

        #endregion

        public async Task ColetaRemuneracaoSecretarios()
        {
            DefineColunasRemuneracaoSecretarios();

            var sqlDeputados = @"
SELECT f.id as id_cf_funcionario, f.chave 
FROM camara.cf_funcionario f
WHERE f.processado = false
order BY f.id";

            var dcDeputado = dbContext.Database.GetDbConnection().Query<Dictionary<string, object>>(sqlDeputados)
                .ToList();
            var queue = new ConcurrentQueue<Dictionary<string, object>>(dcDeputado);

            Task[] tasks = new Task[NumeroThreads];
            for (int i = 0; i < NumeroThreads; ++i)
            {
                tasks[i] = ProcessaFilaColetaRemuneracaoSecretarios(queue);
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessaFilaColetaRemuneracaoSecretarios(ConcurrentQueue<Dictionary<string, object>> queue)
        {
            Dictionary<string, object> secretario = null;
            var context = httpClient.CreateAngleSharpContext();

            while (queue.TryDequeue(out secretario))
            {
                if (secretario is null) continue;

                logger.LogDebug("Inciando coleta da remuneração para os funcionario {IdFuncionario}",
                    secretario["id_cf_funcionario"]);
                await ConsultarRemuneracaoSecretario(colunasBanco, secretario, context);
            }
        }

        private async Task ConsultarRemuneracaoSecretario(Dictionary<string, string> colunasBanco,
            Dictionary<string, object> secretario, IBrowsingContext context)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var dataCorte = new DateTime(2008, 2, 1);

            using var scope = serviceProvider.CreateScope();
            var localDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using (logger.BeginScope(new Dictionary<string, object>
                       { ["Funcionario"] = Convert.ToInt32(secretario["id_cf_funcionario"]) }))
            {
                try
                {
                    var erroColeta = false;
                    var idFuncionario = Convert.ToInt32(secretario["id_cf_funcionario"]);
                    //var idFuncionarioContratacao = Convert.ToInt32(secretario["id_cf_funcionario_contratacao"]);
                    //var idDeputado = Convert.ToInt32(secretario["id_cf_deputado"]);
                    var chave = secretario["chave"].ToString();

                    var folhas = new List<FuncionarioRemuneracao>();
                    DateTime dtUltimaRemuneracaoColetada;
                    IEnumerable<FuncionarioContratacao> contratacoes =
                        localDbContext.FuncionarioContratacoes.Where(x => x.IdFuncionario == idFuncionario);
                    var objUltimaRemuneracaoColetada = localDbContext.FuncionarioRemuneracoes
                        .Where(x => x.IdFuncionario == idFuncionario).Max(x => x.Referencia);
                    dtUltimaRemuneracaoColetada = !Convert.IsDBNull(objUltimaRemuneracaoColetada)
                        ? Convert.ToDateTime(objUltimaRemuneracaoColetada)
                        : DateTime.MinValue;

                    var address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{chave}";
                    var document = await context.OpenAsyncAutoRetry(address);
                    if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                    {
                        logger.LogError(document.StatusCode + ":" + address);
                        return;
                    }

                    int anoSelecionado = 0, mesSelecionado = 0;
                    try
                    {
                        Regex myRegex = new Regex(@"ano=(\d{4})&mes=(\d{1,2})", RegexOptions.Singleline);
                        // window.location.href = 'https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/BDEdGyYrxGdG42MO7egk?ano=2021&mes=7';
                        var linkMatch = myRegex.Match(document.Scripts[1].InnerHtml);
                        if (linkMatch.Length > 0)
                        {
                            anoSelecionado = Convert.ToInt32(linkMatch.Groups[1].Value);
                            mesSelecionado = Convert.ToInt32(linkMatch.Groups[2].Value);

                            var dataColeta = new DateTime(anoSelecionado, mesSelecionado, 1);
                            if (dataColeta < dataCorte)
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (document.QuerySelector(".remuneracao-funcionario") != null)
                            {
                                logger.LogError("Remuneração indisponível! Mensagem: {MensagemErro}",
                                    document.QuerySelector(".remuneracao-funcionario").TextContent.Trim());
                                MarcarComoProcessado(localDbContext, idFuncionario);
                            }

                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(idFuncionario + ":" + ex.Message);
                        if (document.QuerySelector(".remuneracao-funcionario") != null)
                            logger.LogError(document.QuerySelector(".remuneracao-funcionario").TextContent.Trim());

                        //MarcarComoProcessado(localDbContext, idFuncionario);
                        return;
                    }

                    var addressFull = address + $"?ano={anoSelecionado}&mes={mesSelecionado}";
                    if (document.Location.Href != addressFull)
                    {
                        document = await context.OpenAsyncAutoRetry(addressFull);
                        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                        {
                            logger.LogError(document.StatusCode + ":" + addressFull);
                            return;
                        }
                    }

                    var anos = (document.GetElementById("anoRemuneracao") as IHtmlSelectElement).Options
                        .OrderByDescending(x => Convert.ToInt32(x.Text));
                    if (!anos.Any())
                    {
                        logger.LogError("Anos indisponiveis:" + document.Location.Href);
                        return;
                    }


                    foreach (var ano in anos)
                    {
                        using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano.Value }))
                        {
                            if (ano.OuterHtml.Contains("display: none;")) continue;
                            if (Convert.ToInt32(ano.InnerHtml) < dtUltimaRemuneracaoColetada.Year) continue;

                            //Console.WriteLine();
                            //Console.WriteLine($"Ano: {ano.Text}");
                            address =
                                $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{ano.Value}";
                            if (document.Location.Href != address)
                            {
                                document = await context.OpenAsyncAutoRetry(address);

                                if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                                {
                                    logger.LogError(document.StatusCode + ":" + address);
                                    erroColeta = true;
                                    continue;
                                }
                            }

                            var calendario = document.QuerySelectorAll(".linha-tempo li");
                            var meses = (document.GetElementById("mesRemuneracao") as IHtmlSelectElement).Options
                                .Select(x => Convert.ToInt32(x.Value))
                                .OrderByDescending(x => x);

                            if (!meses.Any())
                            {
                                logger.LogError("Meses indisponiveis:" + document.Location.Href);
                                erroColeta = true;
                                continue;
                            }

                            foreach (var mes in meses)
                            {
                                using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes }))
                                {
                                    var dataReferencia = new DateTime(Convert.ToInt32(ano.Text), mes, 1);
                                    if (dataReferencia < dataCorte)
                                    {
                                        logger.LogDebug(
                                            "Data de coleta {DataColeta:yyyy-MM-dd} fora do pediodo de corte da coleta {DataCorte:yyyy-MM-dd}",
                                            dataReferencia, dataCorte);
                                        continue;
                                    }

                                    var elMesSelecionadoNoCalendario = (calendario[mes - 1] as IHtmlListItemElement);
                                    if (elMesSelecionadoNoCalendario.IsHidden ||
                                        elMesSelecionadoNoCalendario.OuterHtml.Contains("display: none;") ||
                                        mes > mesSelecionado)
                                    {
                                        logger.LogDebug(
                                            "Não há remuneração para {MesRemuneracaoExtenso}[{MesRemuneracao:00}]/{AnoRemuneracao} #1",
                                            elMesSelecionadoNoCalendario.TextContent.Trim().Split(" ")[1], mes,
                                            ano.Text);
                                        continue;
                                    }

                                    if (new DateTime(Convert.ToInt32(ano.InnerHtml), mes, 1) <=
                                        dtUltimaRemuneracaoColetada)
                                    {
                                        logger.LogDebug(
                                            "Remuneração ja coletada para {MesRemuneracao}/{AnoRemuneracao}", mes,
                                            ano.Text);
                                        continue;
                                    }

                                    address =
                                        $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{chave}?ano={ano.Text}&mes={mes}";
                                    if (document.Location.Href != address)
                                    {
                                        document = await context.OpenAsyncAutoRetry(address);
                                        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                                        {
                                            logger.LogError(document.StatusCode + ":" + address);
                                            erroColeta = true;
                                            continue;
                                        }
                                    }

                                    var dadosIndisponiveis = document.QuerySelectorAll(".remuneracao-funcionario");
                                    if (dadosIndisponiveis.Any() && dadosIndisponiveis[0].TextContent.Trim() ==
                                        "Ainda não há dados disponíveis.")
                                    {
                                        logger.LogDebug(
                                            "Não há remuneração para {MesRemuneracaoExtenso}[{MesRemuneracao:00}]/{AnoRemuneracao} #2",
                                            elMesSelecionadoNoCalendario.TextContent.Trim().Split(" ")[1], mes,
                                            ano.Text);
                                        continue;
                                    }

                                    var folhasPagamento = document.QuerySelectorAll(".remuneracao-funcionario__info");
                                    if (!folhasPagamento.Any())
                                    {
                                        logger.LogWarning("Dados indisponiveis: " + address);
                                        erroColeta = true;
                                        return; // Erro no funcionario, abortar coleta dele
                                    }

                                    foreach (var folhaPagamento in folhasPagamento)
                                    {
                                        var titulo = folhaPagamento.QuerySelector(".remuneracao-funcionario__mes-ano")
                                            .TextContent;

                                        using (logger.BeginScope(new Dictionary<string, object> { ["Folha"] = titulo }))
                                        {
                                            if (titulo.Split("Permanência")[0].Trim() != $"{mes:00}{ano.Text}")
                                            {
                                                throw new NotImplementedException("Algo esta errado!");
                                            }

                                            var folha = new FuncionarioRemuneracao();
                                            IEnumerable<PropertyInfo> props = folha.GetType()
                                                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                                            var cabecalho =
                                                folhaPagamento.QuerySelectorAll(".remuneracao-funcionario__info-item");
                                            var categoriaFuncional =
                                                cabecalho.FirstOrDefault(x =>
                                                        x.TextContent.StartsWith("Categoria funcional"))?.TextContent
                                                    .Split(":")[1].Trim();
                                            var dataExercicio =
                                                cabecalho.FirstOrDefault(x =>
                                                        x.TextContent.StartsWith("Data de exercício"))?.TextContent
                                                    .Split(":")[1].Trim();
                                            var cargo = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Cargo"))
                                                ?.TextContent.Split(":")[1].Trim();
                                            var nivel = cabecalho
                                                .FirstOrDefault(x =>
                                                    x.TextContent.StartsWith("Função/cargo em comissão"))?.TextContent
                                                .Split(":")[1].Trim();

                                            var linhas =
                                                folhaPagamento.QuerySelectorAll(
                                                    ".tabela-responsiva--remuneracao-funcionario tbody tr");
                                            foreach (var linha in linhas)
                                            {
                                                var colunas = linha.QuerySelectorAll("td");
                                                if (!colunas.Any()) continue;

                                                var descricao = colunas[0].TextContent.Trim();
                                                var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

                                                var propInfo = folha.GetType().GetProperty(colunasBanco[descricao]);
                                                propInfo.SetValue(folha, valor, null);
                                            }

                                            byte tipo = 0;
                                            switch (titulo.Split("-")[1].Trim())
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

                                            folha.IdFuncionario = idFuncionario;

                                            folha.Tipo = tipo;
                                            folha.Nivel = nivel;
                                            folha.Referencia = dataReferencia;

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

                                            var dataContratacao = DateOnly.Parse(dataExercicio, cultureInfo);
                                            folha.ValorOutros = folha.ValorDiarias + folha.ValorAuxilios +
                                                                folha.ValorVantagens;
                                            folha.ValorTotal = folha.ValorBruto + folha.ValorOutros;

                                            var contratacao =
                                                contratacoes.FirstOrDefault(c => c.PeriodoDe == dataContratacao);
                                            if (contratacao == null)
                                                contratacao = contratacoes.FirstOrDefault(c =>
                                                    (c.PeriodoDe.HasValue && c.PeriodoAte.HasValue) &&
                                                    DateOnly.FromDateTime(dataReferencia).IsBetween(c.PeriodoDe.Value,
                                                        c.PeriodoAte.Value));

                                            if (contratacao != null)
                                            {
                                                folha.IdFuncionarioContratacao = contratacao.Id;
                                                folha.IdDeputado = contratacao.IdDeputado;

                                                if (contratacao.IdCargo == null)
                                                {
                                                    contratacao.IdCargo = localDbContext.FuncionarioCargos
                                                        .First(x => EF.Functions.ILike(x.Nome, cargo)).Id;

                                                    if (contratacao.IdGrupoFuncional == null &&
                                                        !string.IsNullOrEmpty(categoriaFuncional))
                                                        contratacao.IdGrupoFuncional = localDbContext
                                                            .FuncionarioGruposFuncionais.First(x =>
                                                                EF.Functions.ILike(x.Nome, categoriaFuncional)).Id;

                                                    if (contratacao.IdNivel == null && !string.IsNullOrEmpty(nivel))
                                                        contratacao.IdNivel = localDbContext.FuncionarioNiveis
                                                            .First(x => EF.Functions.ILike(x.Nome, categoriaFuncional))
                                                            .Id;

                                                    localDbContext.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                logger.LogWarning(
                                                    "Não foi identificada a contratação do funcionario {IdFuncionario} em {Mes}/{Ano}",
                                                    idFuncionario, mes, ano.Text);
                                            }

                                            folhas.Add(folha);

                                            logger.LogDebug(
                                                "Inserida remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes,
                                                ano.Text, titulo);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!erroColeta)
                    {
                        if (folhas.Any())
                        {
                            foreach (var folha in folhas)
                            {
                                try
                                {
                                    localDbContext.FuncionarioRemuneracoes.Add(folha);
                                    localDbContext.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    if (!ex.Message.Contains("Duplicate"))
                                        throw;
                                    else
                                        logger.LogWarning("Registro duplicado ignorado: {@Folha}", folha);
                                }
                            }

                            logger.LogDebug(
                                "Coleta finalizada para o funcionario {IdFuncionario} com {Registros} registros",
                                idFuncionario, folhas.Count());
                        }
                        else
                        {
                            logger.LogWarning("Coleta finalizada para o funcionario {IdFuncionario} sem registros",
                                idFuncionario);
                        }

                        MarcarComoProcessado(localDbContext, idFuncionario);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(secretario["id_cf_funcionario"].ToString() + ":" + ex.Message);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }

        private void MarcarComoProcessado(AppDbContext localDbContext, int idFuncionario)
        {
            localDbContext.FuncionariosCamaraFederal.Where(x => x.Id == idFuncionario)
                .ExecuteUpdate(setters => setters
                    .SetProperty(x => x.Processado, true)
                );

            localDbContext.SaveChanges();
        }

        public async Task ColetaDadosFuncionarios()
        {
            var addresses = new string[]
            {
                "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&areaDeAtuacao=&categoriaFuncional=&lotacao=&situacao=Em%20exerc%C3%ADcio&pagina=",
                "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Aposentado&pagina=",
                "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Licenciado&pagina=",
                "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Cedido&pagina="
            };

            var sqlInsert = @"
        insert ignore into cf_funcionario_temp (chave, nome, categoria_funcional, area_atuacao, situacao{0})
        values (@chave, @nome, @categoria_funcional, @area_atuacao, @situacao{1})
        ";
            var context = httpClient.CreateAngleSharpContext();

            // Migrated to IDbConnection - removed AppDb usage
            {
                dbContext.Database.GetDbConnection().Execute("truncate table cf_funcionario_temp");

                foreach (var address in addresses)
                {
                    var pagina = 0;
                    while (true)
                    {
                        var document = await context.OpenAsyncAutoRetry(address + (++pagina).ToString());
                        if (document.StatusCode != HttpStatusCode.OK)
                        {
                            Console.WriteLine(document.StatusCode);
                            await Task.Delay(TimeSpan.FromSeconds(30));

                            document = await context.OpenAsyncAutoRetry(address + (++pagina).ToString());
                            if (document.StatusCode != HttpStatusCode.OK)
                            {
                                Console.WriteLine(document.StatusCode);
                                break;
                            }
                        }

                        var linhas =
                            document.QuerySelectorAll(".l-busca__secao--resultados table.tabela-responsiva tbody tr");
                        foreach (var linha in linhas)
                        {
                            string colunasDB = "", valoresDB = "";

                            var colunas = linha.QuerySelectorAll("td");
                            var nome = colunas[0].TextContent.Trim();
                            var categoria = colunas[1].TextContent.Trim();
                            var area = colunas[2].TextContent.Trim();
                            var situacao = colunas[3].TextContent.Trim();

                            var chave = (colunas[0].FirstElementChild as IHtmlAnchorElement)
                                .GetAttribute("data-target");

                            if (categoria.StartsWith("Deputado")) categoria = "Deputado";
                            if (categoria.StartsWith("Ex-deputado")) categoria = "Ex-deputado";
                            if (area == "-") area = null;

                            Console.WriteLine();
                            Console.WriteLine($"Funcionario: {nome}");

                            var lstInfo =
                                document.QuerySelectorAll(".modal--funcionario" + chave + " .lista-funcionario__item");
                            foreach (var info in lstInfo)
                            {
                                try
                                {
                                    var titulo = info.QuerySelector(".lista-funcionario__titulo")?.TextContent.Trim();
                                    var valor = info?.TextContent?.Replace(titulo, "").Trim();

                                    switch (titulo.Replace(":", ""))
                                    {
                                        case "Cargo":
                                            colunasDB += ", cargo";
                                            valoresDB += ", @cargo";

                                            break;
                                        case "Nível":
                                            colunasDB += ", nivel";
                                            valoresDB += ", @nivel";

                                            break;
                                        case "Função comissionada":
                                            colunasDB += ", funcao_comissionada";
                                            valoresDB += ", @funcao_comissionada";

                                            break;
                                        case "Local de trabalho":
                                            colunasDB += ", local_trabalho";
                                            valoresDB += ", @local_trabalho";

                                            break;
                                        case "Data da designação da função":
                                            colunasDB += ", data_designacao_funcao";
                                            valoresDB += ", @data_designacao_funcao";

                                            break;
                                        case "Situação":
                                        case "Categoria funcional":
                                        case "área de atuação":
                                            break;
                                        default:
                                            throw new NotImplementedException(
                                                $"Vefificar beneficios do funcionario {nome}.");
                                    }

                                    Console.WriteLine($"{titulo}: {valor}");
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, ex.Message);
                                }
                            }

                            dbContext.Database.GetDbConnection()
                                .Execute(string.Format(sqlInsert, colunasDB, valoresDB));
                        }

                        if (linhas.Count() < 20)
                        {
                            logger.LogDebug("Parando na pagina {Pagina}!", pagina);
                            break;
                        }
                    }
                }

                dbContext.Database.GetDbConnection().Execute(@"
        INSERT ignore INTO cf_funcionario(chave, nome)
        SELECT chave, nome FROM camara.cf_funcionario_temp t");

                dbContext.Database.GetDbConnection().Execute(@"
        INSERT INTO cf_funcionario_contratacao 
        	(id_cf_deputado, id_cf_funcionario, id_cf_funcionario_grupo_funcional, id_cf_funcionario_cargo, id_cf_funcionario_nivel, id_cf_funcionario_funcao_comissionada, 
        		id_cf_funcionario_area_atuacao, id_cf_funcionario_local_trabalho, id_cf_funcionario_situacao, periodo_de, periodo_ate)
        SELECT NULL, f.id, gf.id, c.id, n.id, fc.id, aa.id, lt.id, s.id, t.data_designacao_funcao, null 
        FROM camara.cf_funcionario_temp t
        JOIN cf_funcionario f ON f.chave = t.chave
        LEFT JOIN cf_funcionario_grupo_funcional gf ON gf.nome = t.categoria_funcional
        LEFT JOIN cf_funcionario_cargo c ON c.nome = t.cargo
        LEFT JOIN cf_funcionario_nivel n ON n.nome = t.nivel
        LEFT JOIN cf_funcionario_funcao_comissionada fc ON fc.nome = t.funcao_comissionada
        LEFT JOIN cf_funcionario_area_atuacao aa ON aa.nome = t.area_atuacao
        LEFT JOIN cf_funcionario_local_trabalho lt ON lt.nome = t.local_trabalho
        LEFT JOIN cf_funcionario_situacao s ON s.nome = t.situacao
        LEFT JOIN cf_funcionario_contratacao ct ON ct.id_cf_funcionario = f.id
        WHERE ct.id IS NULL");
            }
        }
    }
}