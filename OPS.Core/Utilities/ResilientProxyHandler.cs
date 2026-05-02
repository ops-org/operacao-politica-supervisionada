using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OPS.Core.Utilities;

public class ResilientProxyHandler : HttpClientHandler
{
    private bool _configured;
    private readonly ProxySettings _settings;
    private readonly object _proxyLock = new();

    public ResilientProxyHandler(ProxySettings settings = null, bool allowRedirect = true)
    {
        _settings = settings;

        if (!allowRedirect)
        {
            // Configurações resilientes
            AllowAutoRedirect = false;
            MaxAutomaticRedirections = 1;
            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;
        }

        // Desabilitar proxy automático
        Proxy = null;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_configured)
        {
            var host = request?.RequestUri?.Host;

            if (host != null && _settings.ProxyUrl != null && _settings.Hosts.Exists(h => host.EndsWith(h, StringComparison.OrdinalIgnoreCase)))
            {
                var proxy = new WebProxy(_settings.ProxyUrl);
                lock (_proxyLock)
                {
                    Proxy = proxy;
                    UseProxy = true;
                }
            }
            else
            {
                lock (_proxyLock)
                {
                    UseProxy = false;
                    Proxy = null;
                }
            }

            _configured = true;
        }

        return base.SendAsync(request, cancellationToken);
    }
}


public class ProxySettings
{
    public string ProxyUrl { get; set; }

    public List<string> Hosts { get; set; } = new List<string>();
}