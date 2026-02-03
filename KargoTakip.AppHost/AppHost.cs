var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("messaging");

builder.AddProject<Projects.KargoTakip_API>("kargotakip-api")
    .WithReference(rabbitmq);

builder.Build().Run();
