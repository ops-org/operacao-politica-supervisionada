using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using MySqlConnector;
using MySqlConnector.Logging;
using OPS.Core.DAO;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO.Compression;

namespace OPS.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Core.Padrao.ConnectionString = Configuration.GetConnectionString("AuditoriaContext");
            new ParametrosRepository().CarregarPadroes();

            services.AddTransient<IDbConnection>(_ => new MySqlConnection(Configuration.GetConnectionString("AuditoriaContext")));

            services.AddScoped<PartidoRepository>();
            services.AddScoped<EstadoRepository>();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder
            //            .AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .AllowCredentials());
            //});

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder =>
                {
#if DEBUG
                    builder
                        .AllowAnyOrigin()
                        .WithHeaders(HeaderNames.ContentType, HeaderNames.Origin, HeaderNames.AccessControlAllowOrigin, "x-custom-header", "accept")
                        .WithMethods("POST", "GET", "OPTIONS");
#else
                    builder
                        .WithOrigins("http://www.ops.net.br", "https://www.ops.net.br", "http://ops.net.br", "https://ops.net.br")
                        // Support https://*.domain.com
                        //.SetIsOriginAllowedToAllowWildcardSubdomains()

                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Credentials
                        // https://stackoverflow.com/questions/24687313/what-exactly-does-the-access-control-allow-credentials-header-do
                        // JWT is not a cookie solution, disable it without allow credential
                        // .AllowCredentials()
                        //.DisallowCredentials()

                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
                        // Without it will popup error: Request header field content-type is not allowed by Access-Control-Allow-Headers in preflight response
                        .WithHeaders(HeaderNames.ContentType, HeaderNames.Origin, HeaderNames.AccessControlAllowOrigin, "x-custom-header", "accept")
                        // Web Verbs like GET, POST, default enabled
                        .WithMethods("POST", "GET", "OPTIONS");
#endif

                });
            });

            // Configue compression
            // https://gunnarpeipman.com/aspnet-core-compress-gzip-brotli-content-encoding/
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

            // https://github.com/KevinDockx/HttpCacheHeaders
            services.AddHttpCacheHeaders();
            //services.AddMvc().AddNewtonsoftJson();
            services.AddControllers();
            services.AddApplicationInsightsTelemetry(Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //var dtf = new DateTimeFormatInfo
            //{
            //    ShortDatePattern = "dd/MM/yyyy",
            //    LongDatePattern = "dd/MM/yyyy HH:mm",
            //    ShortTimePattern = "HH:mm",
            //    LongTimePattern = "HH:mm"
            //};

            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("pt-BR") // { DateTimeFormat = dtf },
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pt-BR"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //// This will make the HTTP requests log as rich logs instead of plain text.
            //app.UseSerilogRequestLogging(options =>
            //{
            //    // Customize the message template
            //    //options.MessageTemplate = "Handled {RequestPath}";

            //    // Emit debug-level events instead of the defaults
            //    //options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

            //    // Attach additional properties to the request completion event
            //    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            //    {
            //        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            //        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            //        diagnosticContext.Set("RemoteIpAddress", httpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString());
            //        diagnosticContext.Set("LocalIpAddress", httpContext.Request.HttpContext.Connection.LocalIpAddress.ToString());
            //    };
            //});

            //app.UseHttpsRedirection();
            //app.UseCacheOutput();
            app.UseRouting();

            // Enable CORS (Cross-Origin Requests)
            // The call to UseCors must be placed after UseRouting, but before UseAuthorization
            app.UseCors("CorsPolicy");

            //app.UseAuthorization();

            // Enable compression
            app.UseResponseCompression();

            app.UseHttpCacheHeaders();
           

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    const int durationInSeconds = 60 * 60 * 24;
                    context.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
