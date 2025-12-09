var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.KargoTakip_API>("kargotakip-api");

builder.Build().Run();
