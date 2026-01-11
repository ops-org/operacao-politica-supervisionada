using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.SenadoFederal;

namespace OPS.Importador.SenadoFederal
{
    public class ImportadorRemuneracaoSenado : IImportadorRemuneracao
    {
        protected readonly ILogger<ImportadorDespesasSenado> logger;

        protected readonly AppDbContext dbContext;

        public string rootPath { get; init; }
        public string tempPath { get; init; }

        private int linhasProcessadasAno { get; set; }

        public bool importacaoIncremental { get; init; }

        public HttpClient httpClient { get; }

        public ImportadorRemuneracaoSenado(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorDespesasSenado>>();
            dbContext = serviceProvider.GetService<AppDbContext>();

            var configuration = serviceProvider.GetService<IConfiguration>();
            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];
            importacaoIncremental = Convert.ToBoolean(configuration["AppSettings:ImportacaoDespesas:Incremental"] ?? "false");

            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");
        }

        /// <summary>
        ///  https://www.senado.leg.br/transparencia/rh/servidores/consulta_remuneracao.asp
        /// </summary>
        /// <param name="ano"></param>
        /// <param name="mes"></param>
        public void ImportarRemuneracao(int ano, int mes)
        {
            var anomes = Convert.ToInt32($"{ano:0000}{mes:00}");
            var urlOrigem = string.Format("https://www.senado.leg.br/transparencia/LAI/secrh/SF_ConsultaRemuneracaoServidoresParlamentares_{0}.csv", anomes);
            var caminhoArquivo = System.IO.Path.Combine(tempPath, "SF-RM-" + anomes + ".csv");

            try
            {
                var novoArquivoBaixado = BaixarArquivo(urlOrigem, caminhoArquivo);
                if (importacaoIncremental && !novoArquivoBaixado)
                {
                    logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                    return;
                }

                CarregaRemuneracaoCsv(caminhoArquivo, anomes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                if (File.Exists(caminhoArquivo))
                    File.Delete(caminhoArquivo);
            }
        }

        private Vinculo GetOrCreateVinculo(AppDbContext dbContext, string descricao, List<Vinculo> vinculos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Vínculo description cannot be empty");

            var vinculo = vinculos.FirstOrDefault(v => v.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (vinculo == null)
            {
                vinculo = new Vinculo { Descricao = descricao };
                dbContext.Vinculos.Add(vinculo);
                dbContext.SaveChanges();
                vinculos.Add(vinculo);
            }
            return vinculo;
        }

        private Categoria GetOrCreateCategoria(AppDbContext dbContext, string descricao, List<Categoria> categorias)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Categoria description cannot be empty");

            var categoria = categorias.FirstOrDefault(c => c.Descricao == descricao);
            if (categoria == null)
            {
                categoria = new Categoria { Descricao = descricao };
                dbContext.Categorias.Add(categoria);
                dbContext.SaveChanges();
                categorias.Add(categoria);
            }
            return categoria;
        }

        private Cargo? GetOrCreateCargo(AppDbContext dbContext, string descricao, List<Cargo> cargos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var cargo = cargos.FirstOrDefault(c => c.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (cargo == null)
            {
                cargo = new Cargo { Descricao = descricao };
                dbContext.Cargos.Add(cargo);
                dbContext.SaveChanges();
                cargos.Add(cargo);
            }
            return cargo;
        }

        private ReferenciaCargo? GetOrCreateReferenciaCargo(AppDbContext dbContext, string descricao, List<ReferenciaCargo> referenciaCargos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var referenciaCargo = referenciaCargos.FirstOrDefault(l => l.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (referenciaCargo == null)
            {
                referenciaCargo = new ReferenciaCargo { Descricao = descricao };
                dbContext.ReferenciaCargos.Add(referenciaCargo);
                dbContext.SaveChanges();
                referenciaCargos.Add(referenciaCargo);
            }
            return referenciaCargo;
        }

        private Funcao? GetOrCreateFuncao(AppDbContext dbContext, string descricao, List<Funcao> funcoes)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var funcao = funcoes.FirstOrDefault(f => f.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (funcao == null)
            {
                funcao = new Funcao { Descricao = descricao };
                dbContext.Funcoes.Add(funcao);
                dbContext.SaveChanges();
                funcoes.Add(funcao);
            }
            return funcao;
        }

        private Lotacao GetOrCreateLotacao(AppDbContext dbContext, string descricao, List<Lotacao> lotacoes)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Lotação description cannot be empty");

            var lotacao = lotacoes.FirstOrDefault(l => l.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (lotacao == null)
            {
                lotacao = new Lotacao { Descricao = descricao };
                dbContext.Lotacoes.Add(lotacao);
                dbContext.SaveChanges();
                lotacoes.Add(lotacao);
            }
            return lotacao;
        }

        private TipoFolha GetOrCreateTipoFolha(AppDbContext dbContext, string descricao, List<TipoFolha> tipoFolhas)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Tipo Folha description cannot be empty");

            var tipoFolha = tipoFolhas.FirstOrDefault(t => t.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (tipoFolha == null)
            {
                tipoFolha = new TipoFolha { Descricao = descricao };
                dbContext.TipoFolhas.Add(tipoFolha);
                dbContext.SaveChanges();
                tipoFolhas.Add(tipoFolha);
            }
            return tipoFolha;
        }

        private decimal? ParseDecimal(string value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, NumberStyles.Any, cultureInfo, out decimal result))
                return result;

            return 0;
        }

        private void CarregaRemuneracaoCsv(string file, int anomes)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            // var lotacoes = dbContext.Lotacoes.Where(x => x.Descricao.Contains("Senador", StringComparison.CurrentCultureIgnoreCase)).ToList();
            //using (var dbContext = dbContextFactory.CreateDbContext())
            {
                // Load all data upfront for caching
                var funcoes = dbContext.Funcoes.ToList();
                var cargos = dbContext.Cargos.ToList();
                var categorias = dbContext.Categorias.ToList();
                var vinculos = dbContext.Vinculos.ToList();
                var referenciaCargos = dbContext.ReferenciaCargos.ToList();
                var lotacoes = dbContext.Lotacoes.ToList();
                var tipoFolhas = dbContext.TipoFolhas.ToList();

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short linha = 0;

                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                    {
                        while (csv.Read())
                        {
                            linha++;
                            if (linha == 1) //Pula primeira linha, data da atualização
                                continue;

                            if (linha == 2)
                            {
                                if (
                                        (csv[(int)SenadoRemuneracaoCsvColumns.VÍNCULO] != "VÍNCULO") ||
                                        (csv[(int)SenadoRemuneracaoCsvColumns.VANT_INDENIZATORIAS].Trim() != "VANT_INDENIZATORIAS")
                                    )
                                {
                                    throw new Exception("Mudança de integração detectada para o Senado Federal");
                                }

                                // Pular linha de titulo
                                continue;
                            }

                            // Get or create related entities
                            var vinculo = GetOrCreateVinculo(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.VÍNCULO], vinculos);
                            var categoria = GetOrCreateCategoria(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.CATEGORIA], categorias);
                            var cargo = GetOrCreateCargo(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.CARGO], cargos);
                            var referenciaCargo = GetOrCreateReferenciaCargo(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.REFERÊNCIA_CARGO], referenciaCargos);
                            var funcao = GetOrCreateFuncao(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.SÍMBOLO_FUNÇÃO], funcoes);
                            var lotacao = GetOrCreateLotacao(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.LOTAÇÃO_EXERCÍCIO], lotacoes);
                            var tipoFolha = GetOrCreateTipoFolha(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.TIPO_FOLHA], tipoFolhas);

                            // Parse numeric values
                            var remunBasica = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.REMUN_BASICA], cultureInfo);
                            var vantPessoais = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.VANT_PESSOAIS], cultureInfo);
                            var funcComissionada = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.FUNC_COMISSIONADA], cultureInfo);
                            var gratNatalina = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.GRAT_NATALINA], cultureInfo);
                            var horasExtras = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.HORAS_EXTRAS], cultureInfo);
                            var outrasEventuais = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.OUTRAS_EVENTUAIS], cultureInfo);
                            var abonoPermanencia = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.ABONO_PERMANENCIA], cultureInfo);
                            var reversaoTetoConst = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.REVERSAO_TETO_CONST], cultureInfo);
                            var impostoRenda = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.IMPOSTO_RENDA], cultureInfo);
                            var previdencia = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.PREVIDENCIA], cultureInfo);
                            var faltas = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.FALTAS], cultureInfo);
                            var remLiquida = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.REM_LIQUIDA], cultureInfo);
                            var diarias = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.DIARIAS], cultureInfo);
                            var auxilios = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.AUXILIOS], cultureInfo);
                            var vantIndenizatorias = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.VANT_INDENIZATORIAS], cultureInfo);

                            var custoTotal = (remLiquida ?? 0) - (impostoRenda ?? 0) - (previdencia ?? 0) - (faltas ?? 0) + (diarias ?? 0) + (auxilios ?? 0) + (vantIndenizatorias ?? 0);

                            var remuneracao = new Remuneracao
                            {
                                IdVinculo = vinculo.Id,
                                IdCategoria = categoria.Id,
                                IdCargo = cargo?.Id,
                                IdReferenciaCargo = referenciaCargo?.Id,
                                IdSimboloFuncao = funcao?.Id,
                                IdLotacao = lotacao.Id,
                                IdTipoFolha = tipoFolha.Id,
                                AnoMes = (int)anomes,
                                Admissao = Convert.ToInt16(csv[(int)SenadoRemuneracaoCsvColumns.ANO_EXERCÍCIO]),
                                RemunBasica = remunBasica,
                                VantPessoais = vantPessoais,
                                FuncComissionada = funcComissionada,
                                GratNatalina = gratNatalina,
                                HorasExtras = horasExtras,
                                OutrasEventuais = outrasEventuais,
                                AbonoPermanencia = abonoPermanencia,
                                ReversaoTetoConst = reversaoTetoConst,
                                ImpostoRenda = impostoRenda,
                                Previdencia = previdencia,
                                Faltas = faltas,
                                RemLiquida = remLiquida,
                                Diarias = diarias,
                                Auxilios = auxilios,
                                VantIndenizatorias = vantIndenizatorias,
                                CustoTotal = custoTotal
                            };

                            dbContext.Remuneracoes.Add(remuneracao);

                            if (linha % 1000 == 0)
                            {
                                dbContext.SaveChanges();
                                dbContext.ChangeTracker.Clear();
                            }
                        }
                    }

                    logger.LogInformation("{Itens} processados!", linha);
                    dbContext.SaveChanges();
                    dbContext.ChangeTracker.Clear();
                }

                // Update senator total remuneration
                var anoMesParam = new NpgsqlParameter("@ano_mes", anomes);
                dbContext.Database.ExecuteSqlRaw(@"
                    UPDATE sf_senador s
                    JOIN sf_lotacao l ON l.id_senador = s.id
                    JOIN sf_remuneracao r ON l.id = r.id_lotacao
                    SET valor_total_remuneracao = (
	                        SELECT SUM(custo_total) AS total
	                        FROM senado.sf_remuneracao r
	                        JOIN sf_lotacao l ON l.id = r.id_lotacao
	                        where l.id_senador = s.id
	                    )
                    WHERE r.ano_mes = @ano_mes
			    ", anoMesParam);
            }
        }

        // Duplicated in ImportadorDespesasSenado.cs - consider refactoring to a shared utility class
        protected bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            var caminhoArquivoDb = caminhoArquivo.Replace(tempPath, "");

            if (importacaoIncremental && File.Exists(caminhoArquivo))
            {
                var arquivoDB = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);
                if (arquivoDB != null && arquivoDB.Verificacao > DateTime.UtcNow.AddDays(-7))
                {
                    logger.LogWarning("Ignorando arquivo verificado recentemente '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);
                    return false;
                }
            }

            logger.LogDebug("Baixando arquivo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);

            string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            var fileExt = Path.GetExtension(caminhoArquivo);
            var caminhoArquivoTmp = caminhoArquivo.Replace(fileExt, $"_tmp{fileExt}");
            if (File.Exists(caminhoArquivoTmp))
                File.Delete(caminhoArquivoTmp);

            //if (config.Estado != Estado.DistritoFederal)
            //    httpClientResilient.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();
            //else
            httpClient.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();

            string checksum = ChecksumCalculator.ComputeFileChecksum(caminhoArquivoTmp);
            var arquivoChecksum = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);
            if (arquivoChecksum != null && arquivoChecksum.Checksum == checksum && File.Exists(caminhoArquivo))
            {
                arquivoChecksum.Verificacao = DateTime.UtcNow;

                logger.LogDebug("Arquivo '{CaminhoArquivo}' é identico ao já existente.", caminhoArquivo);

                if (File.Exists(caminhoArquivoTmp))
                    File.Delete(caminhoArquivoTmp);

                return false;
            }

            if (arquivoChecksum == null)
            {
                logger.LogDebug("Arquivo '{CaminhoArquivo}' é novo.", caminhoArquivo);

                dbContext.ArquivoChecksums.Add(new ArquivoChecksum()
                {
                    Nome = caminhoArquivoDb,
                    Checksum = checksum,
                    TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length,
                    Criacao = DateTime.UtcNow,
                    Atualizacao = DateTime.UtcNow,
                    Verificacao = DateTime.UtcNow
                });
            }
            else
            {
                if (arquivoChecksum.Checksum != checksum)
                {
                    logger.LogDebug("Arquivo '{CaminhoArquivo}' foi atualizado.", caminhoArquivo);

                    arquivoChecksum.Checksum = checksum;
                    arquivoChecksum.TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length;

                }

                arquivoChecksum.Atualizacao = DateTime.UtcNow;
                arquivoChecksum.Verificacao = DateTime.UtcNow;
            }

            dbContext.SaveChanges();
            File.Move(caminhoArquivoTmp, caminhoArquivo, true);
            return true;
        }
    }
}
