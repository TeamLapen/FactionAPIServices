using Projects;

var builder = DistributedApplication.CreateBuilder(args);


var resourceBuilder = builder.AddProject<FactionAPI_Services>("api");

builder.Build().Run();