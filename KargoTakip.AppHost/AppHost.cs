using System.IO;

var builder = DistributedApplication.CreateBuilder(args);

// RabbitMQ'yu Docker container olarak ayağa kaldır; Aspire dashboard'da görünür ve yönetim eklentisi ile erişilebilir
var rabbitmqUsername = builder.AddParameter("rabbitmq-username", "guest");
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", rabbitmqUsername, rabbitmqPassword)
    .WithManagementPlugin()  
    .WithDataVolume();

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume()
    .WithEnvironment("xpack.security.enabled", "false");  // Dev ortami icin; production'da acin

var kibana = builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "8.15.3")
    .WithReference(elasticsearch)
    .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elasticsearch:9200")
    .WithHttpEndpoint(port: 5601, targetPort: 5601);

var logstashPipelinePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "logstash-pipeline"));
var logstash = builder.AddContainer("logstash", "docker.elastic.co/logstash/logstash", "8.15.3")
    .WithEnvironment("LS_JAVA_OPTS", "-Xms256m -Xmx256m")
    .WithBindMount(logstashPipelinePath, "/usr/share/logstash/pipeline", isReadOnly: true)
    .WithHttpEndpoint(port: 9600, targetPort: 9600, name: "monitoring")
    .WithEndpoint(port: 5000, targetPort: 5000, name: "tcp")
    .WithEndpoint(port: 5044, targetPort: 5044, name: "beats");

var api = builder.AddProject<Projects.KargoTakip_API>("kargotakip-api")
    .WithHttpsEndpoint(port: 7055, name: "https-7055")
    .WithReference(rabbitmq)
    .WithReference(elasticsearch);

_ = builder.AddNpmApp("frontend", "../KargoTakip.Client/KargoTakipClient", "start:aspire")
    .WithHttpsEndpoint(port: 4200, targetPort: 4200, isProxied: false);

builder.Build().Run();
