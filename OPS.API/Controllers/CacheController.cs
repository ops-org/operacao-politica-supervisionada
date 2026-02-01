using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OPS.API.Configuration;
using OPS.API.Services;
using System.Threading.Tasks;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController : Controller
    {
        private readonly IHybridCacheService _hybridCacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public CacheController(IHybridCacheService hybridCacheService, IOptions<CacheSettings> cacheSettings, IConfiguration configuration, IHostEnvironment environment)
        {
            _hybridCacheService = hybridCacheService;
            _cacheSettings = cacheSettings.Value;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetCacheStatus()
        {
            return Ok(new
            {
                Environment = _environment.EnvironmentName,
                Enabled = _cacheSettings.Enabled,
                DefaultExpirationHours = _cacheSettings.DefaultExpirationHours,
                LocalCacheExpirationHours = _cacheSettings.LocalCacheExpirationHours,
                ServiceEnabled = _hybridCacheService.IsEnabled,
                ConfigurationSource = $"appsettings{_environment.EnvironmentName}.json"
            });
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestCache()
        {
            const string testKey = "cache-test-key";
            const string testValue = "test-value";

            var result = await _hybridCacheService.GetOrCreateAsync(testKey, async () =>
            {
                return Task.FromResult(testValue);
            });

            return Ok(new
            {
                CacheEnabled = _cacheSettings.Enabled,
                TestResult = result,
                Timestamp = System.DateTime.UtcNow
            });
        }
    }
}
