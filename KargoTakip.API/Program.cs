using KargoTakip.API.HostedServices;
using KargoTakip.API.Middleware;
using KargoTakip.API.Services;
using KargoTakip.Application;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Infrastructure;
using KargoTakip.Infrastructure.Services.Search;
using KargoTakip.Persistence;
using KargoTakip.Persistence.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
    
builder.Host.UseSerilog();
builder.AddServiceDefaults();

// Elasticsearch: Baglanti varsa client 8.x ile kaydedilir (ES sunucusu 8.x; client 9.x "compatible-with=9" gonderir, sunucu reddeder)
var elasticsearchConnection = builder.Configuration["ConnectionStrings:elasticsearch"]
    ?? builder.Configuration["ConnectionStrings__elasticsearch"];
if (!string.IsNullOrWhiteSpace(elasticsearchConnection))
{
    builder.Services.AddSingleton(_ =>
    {
        var settings = new Elastic.Clients.Elasticsearch.ElasticsearchClientSettings(new Uri(elasticsearchConnection));
        return new Elastic.Clients.Elasticsearch.ElasticsearchClient(settings);
    });
    builder.Services.AddScoped<ICargoSearchService, CargoElasticsearchService>();
}
else
{
    builder.Services.AddScoped<ICargoSearchService, NoOpCargoSearchService>();
}

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KargoTakip API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITrustedDeviceService, TrustedDeviceCookieService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddAuthorization();
builder.Services.AddHostedService<EmailConsumerHostedService>();
builder.Services.AddHostedService<ElasticsearchReindexHostedService>();

var app = builder.Build();



app.MapDefaultEndpoints();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("Uygulama başarıyla başlatıldı.");
app.Run();
