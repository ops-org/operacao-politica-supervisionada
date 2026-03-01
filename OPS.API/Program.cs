using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using OPS.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var startup = new Startup();
startup.ConfigureServices(builder, builder.Configuration, builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

app.Run();
