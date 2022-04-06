using System.Net;
using CatalogueService;
using CatalogueService.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using CatalogueService.Extensions;

//var builder = WebApplication.CreateBuilder(args);
var configuration = GetConfiguration();
Log.Logger = CreateSerilogLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext}) ", Program.AppName);
    var host = CreateHostBuilder(configuration, args);
    Log.Information("Applying migrations ({ApplicationContext}) ", Program.AppName);
    host.MigrateDbContext<CatalogueContext>((context, services) =>
    {
        var env = services.GetService<IWebHostEnvironment>();
        var settings = services.GetService<IOptions<CatalogueSettings>>();
        var logger = services.GetService<ILogger<CatalogueContextSeed>>();
        //PrepDb.PrepPopulation();
        new CatalogueContextSeed().SeedAsync(context, env, settings, logger).Wait();
    });
    Log.Information("Starting web host ({ApplicationContext}) ", Program.AppName);
    host.Run();
    return 0;
}
catch(Exception ex)
{
    Log.Fatal(ex, "Program Terminated Unexpectedly ({ApplicationContext}) ", Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IWebHost CreateHostBuilder(IConfiguration configuration, string[] args) =>
WebHost.CreateDefaultBuilder(args)
 .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
 .CaptureStartupErrors(false)
 .UseStartup<Startup>()
 .UseContentRoot(Directory.GetCurrentDirectory())
 .UseWebRoot("Pics")
 .UseSerilog()
 .Build();

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

// (int httpPort) GetDefinedPorts(IConfiguration config)
// {
//     var port = config.GetValue("PORT", 80);
//     return (port);
// }

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
    
    var config = builder.Build();

    if(config.GetValue<bool>("UseVault", false))
    {
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
    }
    return builder.Build();
}

public partial class Program
{
    public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.') + 1);
}

// // Add services to the container.
// builder.Services.AddDbContext<CatalogueContext>(opt => opt.UseInMemoryDatabase("InMem"));

// builder.Services.AddScoped<ICatalogueRepo, CatalogueRepo>();
// builder.Services.AddControllers();
// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// //app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();

// PrepDb.PrepPopulation(app);

// app.Run();
