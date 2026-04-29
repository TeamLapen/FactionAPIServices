using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("timescale/timescaledb", "latest-pg16")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var data = postgres.AddDatabase("DefaultConnection");

var resourceBuilder = builder.AddProject<FactionAPI_Services>("api")
    .WaitFor(data)
    .WithReference(data);

builder.Build().Run();