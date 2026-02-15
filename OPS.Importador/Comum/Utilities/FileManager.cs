using ICSharpCode.SharpZipLib.Zip;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Infraestrutura;

namespace OPS.Importador.Comum.Utilities;

public class FileManager
{
    private readonly ILogger<FileManager> logger;
    private readonly AppSettings appSettings;
    private static object monitorObj = new object();

    private IHttpClientFactory httpClientFactory { get; }

    private HttpClient _httpClientResilient;
    /// <summary>
    /// Client with Retry and NO AllowAutoRedirect
    /// </summary>
    private HttpClient httpClientResilient { get { return _httpClientResilient ??= httpClientFactory.CreateClient("ResilientClient"); } }

    private HttpClient _httpClientDefault;

    /// <summary>
    /// Client with AllowAutoRedirect
    /// </summary>
    private HttpClient httpClientDefault { get { return _httpClientDefault ??= httpClientFactory.CreateClient("DefaultClient"); } }

    public FileManager(IOptions<AppSettings> appSettings, ILogger<FileManager> logger, IHttpClientFactory httpClientFactory)
    {
        this.appSettings = appSettings.Value;
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<bool> BaixarArquivo(AppDbContext dbContext, string url, string filename, Estados? estado)
    {
        var caminhoArquivoDb = GetCaminhoArquivoDb(filename);

        Monitor.Enter(monitorObj);

        try
        {
            var _arquivoChecksum = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);

            if (appSettings.ReuseDownloadFile && File.Exists(filename))
            {
                var arquivoDB = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);
                if (arquivoDB != null)
                {
                    if (arquivoDB.Verificacao > DateTime.UtcNow.AddDays(-7))
                    {
                        logger.LogWarning("Ignorando arquivo verificado recentemente {CaminhoArquivo} a partir de {UrlOrigem}", filename, url);
                        return false;
                    }
                    else if (arquivoDB.Criacao < DateTime.UtcNow.AddMonths(-5))
                    {
                        logger.LogWarning("Ignorando arquivo antigo {CaminhoArquivo} a partir de {UrlOrigem}", filename, url);
                        return false;
                    }

                    if(url.Contains("cdn.tse.jus.br"))
                    {
                        logger.LogWarning("Ignorando arquivo do TSE existente {CaminhoArquivo} a partir de {UrlOrigem}", filename, url);
                        return false;
                    }
                }
            }
        }
        finally
        {
            Monitor.Exit(monitorObj);
        }

        logger.LogInformation("Baixando arquivo {CaminhoArquivo} a partir de {UrlOrigem}", filename, url);

        string diretorio = new FileInfo(filename).Directory.ToString();
        if (!Directory.Exists(diretorio))
            Directory.CreateDirectory(diretorio);

        var fileExt = Path.GetExtension(filename);
        var caminhoArquivoTmp = filename.Replace(fileExt, $"_tmp{fileExt}");
        if (File.Exists(caminhoArquivoTmp))
            File.Delete(caminhoArquivoTmp);

        if (estado != Estados.DistritoFederal)
            await httpClientResilient.DownloadFile(url, caminhoArquivoTmp);
        else
            await httpClientDefault.DownloadFile(url, caminhoArquivoTmp);

        return ProcessarArquivoTemp(dbContext, filename, estado);
    }

    public void MoverArquivoComErro(string caminhoArquivo)
    {
        if (File.Exists(caminhoArquivo))
        {
            var fileExt = Path.GetExtension(caminhoArquivo);
            var caminhoArquivoCorrompido = caminhoArquivo.Replace(fileExt, $"_broken{fileExt}");

            File.Move(caminhoArquivo, caminhoArquivoCorrompido, true);
        }
    }

    internal void AtualizaValorTotal(AppDbContext dbContext, string filename, decimal valorTotalArquivo, decimal valorTotalCalculado)
    {
        var caminhoArquivoDb = GetCaminhoArquivoDb(filename);

        dbContext.ArquivoChecksums
            .Where(x => x.Nome == caminhoArquivoDb)
            .ExecuteUpdate(x => x
                .SetProperty(y => y.ValorTotal, valorTotalArquivo)
                .SetProperty(y => y.Divergencia, valorTotalArquivo - valorTotalCalculado));
    }

    private string GetCaminhoArquivoDb(string filename)
    {
        return filename
            .Replace(appSettings.TempFolder, "")
            .Replace("Estados" + Path.DirectorySeparatorChar, "")
            .Replace(Path.DirectorySeparatorChar.ToString(), "/");
    }

    internal bool ProcessarArquivoTemp(AppDbContext dbContext, string filename, Estados? estado)
    {
        var caminhoArquivoDb = GetCaminhoArquivoDb(filename);
        var fileExt = Path.GetExtension(filename);
        var caminhoArquivoTmp = filename.Replace(fileExt, $"_tmp{fileExt}");

        // Arquivos do Piauí e Paraná são baixados em processo anterior. Quando não existe o arquivo temporário, podemos usar o arquivo existente.
        if ((estado == Estados.Piaui || estado == Estados.Parana) && !File.Exists(caminhoArquivoTmp)) 
            return false;

        string checksum = ChecksumCalculator.ComputeFileChecksum(caminhoArquivoTmp);

        Monitor.Enter(monitorObj);
        try
        {
            var _arquivoChecksum = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);

            if (_arquivoChecksum != null && _arquivoChecksum.Checksum == checksum && File.Exists(filename))
            {
                _arquivoChecksum.Verificacao = DateTime.UtcNow;
                dbContext.SaveChanges();

                logger.LogInformation("Arquivo {CaminhoArquivo} não teve alterações.", filename);

                if (File.Exists(caminhoArquivoTmp))
                    File.Delete(caminhoArquivoTmp);

                return false;
            }

            if (_arquivoChecksum == null)
            {
                logger.LogInformation("Arquivo {CaminhoArquivo} é novo.", filename);

                _arquivoChecksum = new ArquivoChecksum()
                {
                    Nome = caminhoArquivoDb,
                    Checksum = checksum,
                    TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length,
                    Criacao = DateTime.UtcNow,
                    Atualizacao = DateTime.UtcNow,
                    Verificacao = DateTime.UtcNow
                };
                dbContext.ArquivoChecksums.Add(_arquivoChecksum);
            }
            else
            {
                if (_arquivoChecksum.Checksum != checksum)
                {
                    logger.LogInformation("Arquivo {CaminhoArquivo} foi atualizado.", filename);

                    _arquivoChecksum.Checksum = checksum;
                    _arquivoChecksum.TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length;
                    _arquivoChecksum.Revisado = false;
                }

                _arquivoChecksum.Atualizacao = DateTime.UtcNow;
                _arquivoChecksum.Verificacao = DateTime.UtcNow;
            }

            dbContext.SaveChanges();
        }
        finally
        {

            Monitor.Exit(monitorObj);
        }

        if (appSettings.StoreBackupFile && File.Exists(filename))
        {
            var caminhoArquivoBkp = filename.Replace(fileExt, $"_bkp{fileExt}");
            File.Move(filename, caminhoArquivoBkp, true);
        }

        File.Move(caminhoArquivoTmp, filename, true);
        return true;
    }

    public void DescompactarArquivo(string caminhoArquivo, string fileFilter = null)
    {
        logger.LogDebug("Descompactando Arquivo '{CaminhoArquivo}'", caminhoArquivo);

        var zip = new FastZip();
        zip.ExtractZip(caminhoArquivo, Path.GetDirectoryName(caminhoArquivo), fileFilter);
    }
}
