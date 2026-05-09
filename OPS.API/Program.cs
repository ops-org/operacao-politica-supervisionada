using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using OPS.API;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Note: AddServiceDefaults is from Aspire - may not be available in test context
// builder.AddServiceDefaults();

var startup = new Startup();
startup.ConfigureServices(builder, builder.Configuration, builder.Services);

var app = builder.Build();

// Note: MapDefaultEndpoints is from Aspire - may not be available in test context
// app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Only run if this is the entry point (not being tested)
if (args.Length == 0 || args[0] != "--test")
{
    app.Run();
}

// Make this file usable by WebApplicationFactory for testing
public partial class Program { }
