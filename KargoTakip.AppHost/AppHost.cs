var builder = DistributedApplication.CreateBuilder(args);

var rabbitmqUsername = builder.AddParameter("rabbitmq-username", "guest");
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", rabbitmqUsername, rabbitmqPassword)
    .WithManagementPlugin()  
    .WithDataVolume();

var api = builder.AddProject<Projects.KargoTakip_API>("kargotakip-api")
    .WithHttpsEndpoint(port: 7055, name: "https-7055")
    .WithReference(rabbitmq);

_ = builder.AddNpmApp("kargonuzu-gezdiriyoruz", "../KargoTakip.Client/KargoTakipClient", "start:aspire")
    .WithHttpsEndpoint(port: 4200, targetPort: 4200, isProxied: false);

builder.Build().Run();
