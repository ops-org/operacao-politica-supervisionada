using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.CacheOutput;
using AspNetCore.CacheOutput.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPS.Core.DAO;

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
            var cultureInfo = new CultureInfo("pt-BR");
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Core.Padrao.ConnectionString = Configuration["ConnectionStrings:AuditoriaContext"];
            new ParametrosDao().CarregarPadroes();

            services.AddSingleton<CacheKeyGeneratorFactory, CacheKeyGeneratorFactory>();
            services.AddSingleton<ICacheKeyGenerator, DefaultCacheKeyGenerator>();
            services.AddSingleton<IApiCacheOutput, InMemoryCacheOutputProvider>();

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
                    builder
                        .AllowAnyOrigin()
                        //.WithOrigins("http://*.ops.net.br", "https://*.ops.net.br")
                        // Support https://*.domain.com
                        //.SetIsOriginAllowedToAllowWildcardSubdomains()

                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Credentials
                        // https://stackoverflow.com/questions/24687313/what-exactly-does-the-access-control-allow-credentials-header-do
                        // JWT is not a cookie solution, disable it without allow credential
                        // .AllowCredentials()
                        //.DisallowCredentials()

                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
                        // Without it will popup error: Request header field content-type is not allowed by Access-Control-Allow-Headers in preflight response
                        .AllowAnyHeader()

                        // Web Verbs like GET, POST, default enabled
                        .AllowAnyMethod();
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

            //services.AddMvc().AddNewtonsoftJson();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseCacheOutput();
            app.UseRouting();

            // Enable CORS (Cross-Origin Requests)
            // The call to UseCors must be placed after UseRouting, but before UseAuthorization
            app.UseCors("CorsPolicy");

            //app.UseAuthorization();

            // Enable compression
            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
