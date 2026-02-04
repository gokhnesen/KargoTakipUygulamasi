var builder = DistributedApplication.CreateBuilder(args);

// Docker kullanmadan mevcut RabbitMQ'ya bağlan (connection string appsettings'ten okunur)
var messaging = builder.AddConnectionString("messaging");

builder.AddProject<Projects.KargoTakip_API>("kargotakip-api")
    .WithReference(messaging);

builder.Build().Run();
