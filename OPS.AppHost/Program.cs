var builder = DistributedApplication.CreateBuilder(args);

//var postgres = builder.AddPostgres("postgres")
//    .WithDataVolume()
//    .WithPgAdmin();

//var db = postgres.AddDatabase("AuditoriaContext");

//var api = builder.AddProject<Projects.OPS_API>("api")
//    .WithReference(db)
//    .WaitFor(db);

// builder.AddProject<Projects.OPS_Importador>("importador")
//     .WithReference(db)
//
//

var api = builder.AddProject<Projects.OPS_API>("api");

builder.AddJavaScriptApp("frontend", "../OPS.Site", runScriptName: "dev")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(targetPort: 8080)
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"));

builder.Build().Run();
