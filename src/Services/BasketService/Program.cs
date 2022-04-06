using Azure.Core;
using Azure.Identity;
using BasketService;
using BasketService.Controllers;
using BasketService.Infrastructure;
using BasketService.Middleware;
using BasketService.Repository;
using BasketService.Services;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

var configuration = GetConfiguration();
Log.Logger = CreateSerilogLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext}) ", Program.AppName);
    var host = BuildWebHost(configuration, args);
    Log.Information("Starting web host ({ApplicationContext}) ", Program.AppName);
    host.Run();
    return 0;
}
catch(Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicaitonContext}) ", Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
WebHost.CreateDefaultBuilder(args)
    .CaptureStartupErrors(false)
    .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
    .UseFailing(options =>
    {
        options.ConfigPath = "/Failing";
        options.NotFilteredPaths.AddRange(new[] {"/hc", "/liveness"});
    })
    .UseStartup<Startup>()
    .UseContentRoot(Directory.GetCurrentDirectory())
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
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }
    return builder.Build();
}

public partial class Program
{
    public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.') + 1);
}

// var builder = WebApplication.CreateBuilder(args);
// var Configuration = GetConfiguration();

// // Add services to the container.

// builder.Services.AddControllers()
//     .AddApplicationPart(typeof(BasketController).Assembly)
//     .AddJsonOptions(Options => Options.JsonSerializerOptions.WriteIndented = true);

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "MicroWebShop - Basket Service HTTP",
//         Version = "v1",
//         Description = "Basket Service HTTP Api"
//     });
//     options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//     {
//         Type = SecuritySchemeType.OAuth2,
//         Flows = new OpenApiOAuthFlows()
//         {
//             Implicit = new OpenApiOAuthFlow()
//             {
//                 AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
//                 TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
//                 Scopes = new Dictionary<string, string>()
//                 {
//                     {"basketservice", "Basket Service"}
//                 }
//             }
//         }
//     });
//     options.OperationFilter<AuthCheck>();
// });

// builder.Services.Configure<BasketSettings>(Configuration);
// builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// builder.Services.AddTransient<IBasketRepo, RedisRepo>();
// builder.Services.AddTransient<IUserIdentity, UserIdentity>();

// // var redis = ConnectionMultiplexer.Connect("localhost");
// // builder.Services.AddSingleton(s => redis.GetDatabase());

// builder.Services.AddSingleton<ConnectionMultiplexer>(sp =>
// {
//     var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;
//     var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);
//     return ConnectionMultiplexer.Connect(configuration);  //("localhost");
// });

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("CorsPolicy",
//     builder => builder
//     .SetIsOriginAllowed((host) => true)
//     .AllowAnyMethod()
//     .AllowAnyHeader()
//     .AllowCredentials());
// });

// builder.Services.AddOptions();

// var app = builder.Build();

// var pathBase = Configuration["PATH_BASE"];
// if(!string.IsNullOrEmpty(pathBase))
// {
//     app.UsePathBase(pathBase);
// }

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseRouting();
// app.UseCors("CorsPolicy");
// app.UseStaticFiles();

// IConfiguration GetConfiguration()
// {
//     var builder = new ConfigurationBuilder()
//         .SetBasePath(Directory.GetCurrentDirectory())
//         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//         .AddEnvironmentVariables();

//     return builder.Build();
// }

// //app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// app.Run();