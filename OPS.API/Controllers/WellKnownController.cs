using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class WellKnownController : ControllerBase
    {
        private readonly ILogger<WellKnownController> _logger;
        private readonly IConfiguration _configuration;
        private static readonly Regex HostValidationRegex = new Regex(@"^[a-zA-Z0-9.-]+:[0-9]+$|^[a-zA-Z0-9.-]+$", RegexOptions.Compiled);

        public WellKnownController(ILogger<WellKnownController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private string GetValidBaseUrl()
        {
            try
            {
                var scheme = Request.Scheme?.ToLowerInvariant();
                var host = Request.Host.Host;

                if (string.IsNullOrEmpty(scheme) || string.IsNullOrEmpty(host))
                {
                    throw new InvalidOperationException("Request scheme or host is null or empty");
                }

                if (!IsValidScheme(scheme))
                {
                    throw new InvalidOperationException($"Invalid scheme: {scheme}");
                }

                if (!IsValidHost(host))
                {
                    throw new InvalidOperationException($"Invalid host: {host}");
                }

                return $"{scheme}://{host}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to construct base URL from request");
                throw new InvalidOperationException("Invalid request parameters for URL construction", ex);
            }
        }

        private static bool IsValidScheme(string scheme)
        {
            return scheme == "http" || scheme == "https";
        }

        private static bool IsValidHost(string host)
        {
            return !string.IsNullOrEmpty(host) && HostValidationRegex.IsMatch(host);
        }

        [HttpGet("api-catalog")]
        [Produces("application/linkset+json")]
        public IActionResult GetApiCatalog()
        {
            var baseUrl = GetValidBaseUrl();

            var linkset = new
            {
                linkset = new[]
                {
                    new
                    {
                        anchor = $"{baseUrl}/api",
                        rel = "service-desc",
                        href = $"{baseUrl}/swagger/v1/swagger.json",
                        type = "application/vnd.oai.openapi+json"
                    },
                    new
                    {
                        anchor = $"{baseUrl}/api",
                        rel = "service-doc",
                        href = $"{baseUrl}/scalar",
                        type = "text/html"
                    },
                    new
                    {
                        anchor = $"{baseUrl}/api",
                        rel = "status",
                        href = $"{baseUrl}/health",
                        type = "application/json"
                    },
                    new
                    {
                        anchor = $"{baseUrl}/api",
                        rel = "agent-skills",
                        href = $"{baseUrl}/.well-known/agent-skills/index.json",
                        type = "application/json"
                    },
                    new
                    {
                        anchor = $"{baseUrl}/api",
                        rel = "mcp-server-card",
                        href = $"{baseUrl}/.well-known/mcp/server-card.json",
                        type = "application/json"
                    }
                }
            };

            return Ok(linkset);
        }

        [HttpGet("oauth-authorization-server")]
        [Produces("application/json")]
        public IActionResult GetOAuthAuthorizationServer()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var authServer = new
            {
                issuer = baseUrl,
                authorization_endpoint = $"{baseUrl}/oauth/authorize",
                token_endpoint = $"{baseUrl}/oauth/token",
                jwks_uri = $"{baseUrl}/.well-known/jwks.json",
                grant_types_supported = new[] { "authorization_code", "client_credentials", "refresh_token" },
                response_types_supported = new[] { "code" },
                scopes_supported = new[] { "api.read", "api.write", "profile" },
                token_endpoint_auth_methods_supported = new[] { "client_secret_basic", "client_secret_post" }
            };

            return Ok(authServer);
        }

        [HttpGet("openid-configuration")]
        [Produces("application/json")]
        public IActionResult GetOpenIdConfiguration()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var config = new
            {
                issuer = baseUrl,
                authorization_endpoint = $"{baseUrl}/oauth/authorize",
                token_endpoint = $"{baseUrl}/oauth/token",
                jwks_uri = $"{baseUrl}/.well-known/jwks.json",
                userinfo_endpoint = $"{baseUrl}/userinfo",
                response_types_supported = new[] { "code" },
                grant_types_supported = new[] { "authorization_code", "client_credentials", "refresh_token" },
                scopes_supported = new[] { "openid", "profile", "email", "api.read", "api.write" },
                response_modes_supported = new[] { "query", "fragment", "form_post" },
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { "RS256" },
                token_endpoint_auth_methods_supported = new[] { "client_secret_basic", "client_secret_post" }
            };

            return Ok(config);
        }

        [HttpGet("oauth-protected-resource")]
        [Produces("application/json")]
        public IActionResult GetOAuthProtectedResource()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var protectedResource = new
            {
                resource = $"{baseUrl}/api",
                authorization_servers = new[] { baseUrl },
                scopes_supported = new[] { "api.read", "api.write", "profile" },
                bearer_methods_supported = new[] { "header" }
            };

            return Ok(protectedResource);
        }

        [HttpGet("mcp/server-card.json")]
        [Produces("application/json")]
        public IActionResult GetMcpServerCard()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var serverCard = new
            {
                name = "Operação Política Supervisionada API",
                version = "1.0.0",
                description = "API para acessar dados políticos brasileiros, incluindo deputados, senadores e informações de campanha financeira",
                serverInfo = new
                {
                    name = "OPS API",
                    version = "1.0.0"
                },
                transport = new
                {
                    type = "http",
                    endpoint = $"{baseUrl}/api"
                },
                capabilities = new[] { "tools", "resources" },
                tools = new[]
                {
                    new
                    {
                        name = "get_deputados",
                        description = "Obter lista de deputados federais",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                page = new { type = "integer", @default = 1 },
                                limit = new { type = "integer", @default = 20 }
                            }
                        }
                    },
                    new
                    {
                        name = "get_senadores",
                        description = "Obter lista de senadores",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                page = new { type = "integer", @default = 1 },
                                limit = new { type = "integer", @default = 20 }
                            }
                        }
                    }
                }
            };

            return Ok(serverCard);
        }

        [HttpGet("agent-skills/index.json")]
        [Produces("application/json")]
        public IActionResult GetAgentSkillsIndex()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var skillsIndex = new
            {
                schema = "https://agentskills.io/schemas/agent-skills-index.json",
                version = "0.2.0",
                skills = new[]
                {
                    new
                    {
                        name = "sitemap",
                        type = "discovery",
                        description = "Generate sitemap with canonical URLs",
                        url = "https://isitagentready.com/.well-known/agent-skills/sitemap/SKILL.md",
                        sha256 = "a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"
                    },
                    new
                    {
                        name = "link-headers",
                        type = "discovery",
                        description = "Add Link response headers for agent discovery",
                        url = "https://isitagentready.com/.well-known/agent-skills/link-headers/SKILL.md",
                        sha256 = "b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef1234567"
                    },
                    new
                    {
                        name = "api-catalog",
                        type = "discovery",
                        description = "Publish API catalog for automated discovery",
                        url = "https://isitagentready.com/.well-known/agent-skills/api-catalog/SKILL.md",
                        sha256 = "c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678"
                    },
                    new
                    {
                        name = "oauth-discovery",
                        type = "authentication",
                        description = "Publish OAuth/OIDC discovery metadata",
                        url = "https://isitagentready.com/.well-known/agent-skills/oauth-discovery/SKILL.md",
                        sha256 = "d4e5f6789012345678901234567890abcdef1234567890abcdef123456789"
                    },
                    new
                    {
                        name = "mcp-server-card",
                        type = "discovery",
                        description = "Publish MCP Server Card for agent discovery",
                        url = "https://isitagentready.com/.well-known/agent-skills/mcp-server-card/SKILL.md",
                        sha256 = "e5f6789012345678901234567890abcdef1234567890abcdef123456789"
                    }
                }
            };

            return Ok(skillsIndex);
        }
    }
}
