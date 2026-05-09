using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OPS.Infraestrutura;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(AppDbContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            var overallStatus = "healthy";
            var checks = new List<object>();

            // Database connectivity check
            try
            {
                var dbStopwatch = Stopwatch.StartNew();
                await _dbContext.Database.CanConnectAsync();
                dbStopwatch.Stop();

                checks.Add(new
                {
                    name = "database",
                    status = "healthy",
                    responseTime = $"{dbStopwatch.ElapsedMilliseconds}ms",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                overallStatus = "unhealthy";

                checks.Add(new
                {
                    name = "database",
                    status = "unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }

            stopwatch.Stop();

            var health = new
            {
                status = overallStatus,
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                uptime = Environment.TickCount64,
                responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                checks = checks
            };

            return overallStatus == "healthy" ? Ok(health) : StatusCode(503, health);
        }
    }
}
