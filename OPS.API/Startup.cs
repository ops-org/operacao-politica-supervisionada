using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;
using OPS.Core.Repositories;
using OPS.Core.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Interceptors;

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
            Padrao.ConnectionString = Configuration.GetConnectionString("AuditoriaContext");
            //new ParametrosRepository().CarregarPadroes();

            services.AddDbContext<AppDbContext>(options => options
                    .UseNpgsql(Configuration.GetConnectionString("AuditoriaContext"))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            //services.AddTransient<IDbConnection>(_ => new NpgsqlConnection(Configuration.GetConnectionString("AuditoriaContext")));

            services.AddScoped<DeputadoRepository>();
            services.AddScoped<SenadorRepository>();
            services.AddScoped<DeputadoEstadualRepository>();
            services.AddScoped<FornecedorRepository>();
            //services.AddScoped<PresidenciaRepository>();
            //services.AddScoped<ParametrosRepository>();
            services.AddScoped<InicioRepository>();
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
                    builder
                        .AllowAnyOrigin()
                        .WithHeaders(HeaderNames.ContentType, HeaderNames.Origin, HeaderNames.AccessControlAllowOrigin, "x-custom-header", "accept")
                        .WithMethods("POST", "GET", "OPTIONS");
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
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Use PascalCase for property names (null = no transformation, keeps original casing)
                    //options.JsonSerializerOptions.PropertyNamingPolicy = null;

                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                });

            services.AddApplicationInsightsTelemetry(Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

            // Add Swagger services
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OPS API",
                    Version = "v1",
                    Description = "Operação Política Supervisionada API",
                    Contact = new OpenApiContact
                    {
                        Name = "OPS Team",
                        Email = "vanderlei@ops.org.br"
                    }
                });

                // Include XML comments if file exists
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                if (System.IO.File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
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

            // Enable Swagger in development
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OPS API v1");
                    c.RoutePrefix = string.Empty; // Sets Swagger UI at root
                });
            }


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
