using AspNetCore.CacheOutput;
using AspNetCore.CacheOutput.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPS.Core.DAO;
using System.Globalization;

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
            var cultureInfo = new CultureInfo("pt-BR");
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Core.Padrao.ConnectionString = Configuration["ConnectionStrings:AuditoriaContext"];
            new ParametrosDao().CarregarPadroes();

            services.AddSingleton<CacheKeyGeneratorFactory, CacheKeyGeneratorFactory>();
            services.AddSingleton<ICacheKeyGenerator, DefaultCacheKeyGenerator>();
            services.AddSingleton<IApiCacheOutput, InMemoryCacheOutputProvider>();

            services.AddCors(o => o.AddPolicy("OPS_CORS", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMvc().AddNewtonsoftJson();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseCors("OPS_CORS");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //var cultureInfo = new CultureInfo("pt-BR");
            //app.UseRequestLocalization(new RequestLocalizationOptions
            //{
            //    DefaultRequestCulture = new RequestCulture(cultureInfo),
            //    SupportedCultures = new List<CultureInfo>
            //    {
            //        cultureInfo,
            //    },
            //    SupportedUICultures = new List<CultureInfo>
            //    {
            //        cultureInfo,
            //    }
            //});

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
            //sfOptions.OnPrepareResponse = ctx =>
            //{
            //    const int durationInSeconds = 60 * 60 * 24;
            //    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
            //};
            app.UseStaticFiles(sfOptions);

            //app.UseMvc();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
