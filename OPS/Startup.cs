using AspNetCore.CacheOutput;
using AspNetCore.CacheOutput.Extensions;
using AspNetCore.CacheOutput.InMemory.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OPS.Core.DAO;
using System;
using System.IO;
using System.Net;

namespace OPS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //if (String.IsNullOrWhiteSpace(environmentName))
            //    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

            //Console.WriteLine($"Running in {environmentName} mode");

            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile($"appsettings.{environmentName}.json", optional: false);

            Configuration = configuration; // builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            Core.Padrao.ConnectionString = Configuration["ConnectionStrings:AuditoriaContext"].ToString();
            new ParametrosDao().CarregarPadroes();

            services.AddInMemoryCacheOutput();

            services.AddSingleton<ICacheKeyGenerator, DefaultCacheKeyGenerator>();
            //services.AddSingleton<IApiOutputCache, InMemoryOutputCacheProvider>();
            //services.AddSingleton<IApiOutputCache, LiteDBOutputCacheProvider>(provider =>
            //{
            //    return new LiteDBOutputCacheProvider("OutputCacheLite.db");
            //});

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Middleware to handle all request
            //app.Use(async (Context, next) =>
            //{
            //    await next();
            //    if (Context.Response.StatusCode == 404 && !Path.HasExtension(Context.Request.Path.Value))
            //    {
            //        Context.Request.Path = "/index.html";
            //        Context.Response.StatusCode = 200;
            //        await next();
            //    }
            //});
            app.UseCacheOutput();

            DefaultFilesOptions dfOptions = new DefaultFilesOptions();
            dfOptions.DefaultFileNames.Clear();
            dfOptions.DefaultFileNames.Add("/index.html");
            app.UseDefaultFiles(dfOptions);

            StaticFileOptions sfOptions = new StaticFileOptions();
            FileExtensionContentTypeProvider typeProvider = new FileExtensionContentTypeProvider();
            if (!typeProvider.Mappings.ContainsKey(".woff2"))
            {
                typeProvider.Mappings.Add(".woff", "application/font-woff");
                typeProvider.Mappings.Add(".woff2", "application/font-woff2");
            }
            sfOptions.ContentTypeProvider = typeProvider;
            app.UseStaticFiles(sfOptions);
            app.UseFileServer(enableDirectoryBrowsing: false);

            app.UseMvc();
        }
    }
}
