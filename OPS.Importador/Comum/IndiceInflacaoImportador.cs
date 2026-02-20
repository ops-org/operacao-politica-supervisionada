using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Importador.Comum
{
    public class IndiceInflacaoImportador
    {
        private readonly ILogger<IndiceInflacaoImportador> _logger;
        private readonly AppDbContext _dbContext;
        private readonly HttpClient _httpClient;

        public IndiceInflacaoImportador(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<IndiceInflacaoImportador>>();
            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
        }

        public async Task ImportarIpca()
        {
            _logger.LogInformation("Iniciando importação de IPCA do BCB");

            // IPCA - Variação mensal (%) - Série 433
            string url = "https://api.bcb.gov.br/dados/serie/bcdata.sgs.433/dados?formato=json";
            
            List<BcbSgsResponse> response;
            try 
            {
                response = await _httpClient.GetFromJsonAsync<List<BcbSgsResponse>>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados do BCB");
                return;
            }

            if (response == null) return;

            // Ensure data is sorted chronologically
            response = response.OrderBy(r => {
                var parts = r.data.Split('/');
                return new DateTime(int.Parse(parts[2]), int.Parse(parts[1]), 1);
            }).ToList();

            // Get existing indices to avoid duplicates and continue calculation
            var indicesExistentes = _dbContext.IndicesInflacao
                .OrderBy(i => i.Ano)
                .ThenBy(i => i.Mes)
                .ToList();

            _logger.LogInformation("Encontrados {Count} registros existentes no banco", indicesExistentes.Count);
            _logger.LogInformation("Recebidos {Count} registros do BCB", response.Count);

            decimal lastIndiceValue = 100;
            if (indicesExistentes.Any())
            {
                lastIndiceValue = indicesExistentes.Last().Indice;
            }

            int count = 0;
            foreach (var item in response)
            {
                var dateParts = item.data.Split('/');
                short mes = short.Parse(dateParts[1]);
                short ano = short.Parse(dateParts[2]);

                if (indicesExistentes.Any(i => i.Ano == ano && i.Mes == mes))
                {
                    continue;
                }

                decimal valor = decimal.Parse(item.valor, System.Globalization.CultureInfo.InvariantCulture);
                
                // If it's the first record ever, we just use 100 as base or calculate from valor
                // But for simplicity, we start at 100 and compound.
                lastIndiceValue *= (1 + valor / 100);

                var novoIndice = new IndiceInflacao
                {
                    Ano = ano,
                    Mes = mes,
                    Valor = valor,
                    Indice = lastIndiceValue
                };

                _dbContext.IndicesInflacao.Add(novoIndice);
                count++;
            }

            if (count > 0)
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Importados {Count} novos registros de IPCA", count);
            }
            else 
            {
                _logger.LogInformation("IPCA já está atualizado");
            }
        }
    }

    public class BcbSgsResponse
    {
        public string data { get; set; }
        public string valor { get; set; }
    }
}
