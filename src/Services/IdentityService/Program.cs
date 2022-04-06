using Azure.Core;
using Azure.Identity;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityService;
using IdentityService.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Serilog;

string Namespace = typeof(Startup).Namespace;
string AppName = Namespace.Substring(Namespace.LastIndexOf('.')+ 1);

var configuration = GetConfiguration();
Log.Logger = CreateSerilogLogger(configuration);

// Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
// {
//     var seqServerUrl = configuration["Serilog:SeqServerUrl"];
//     var logstashUrl = configuration["Serilog:LogstashUrl"];
//     return new LoggerConfiguration()
//         .MinimumLevel.Verbose()
//         .Enrich.WithProperty("ApplicationContext", AppName)
//         .Enrich.FromLogContext()
//         .WriteTo.Console()
//         .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
//         .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://localhost:8080" : logstashUrl)
//         .ReadFrom.Configuration(configuration)
//         .CreateLogger();
// }

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", AppName);
    var host = BuildWebHost(configuration, args);
    Log.Information("Applying migrations ({ApplicationContext})...", AppName);
    host.MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
        .MigrateDbContext<AppDbContext>((context, services) =>
        {
            var env = services.GetService<IWebHostEnvironment>();
            var logger = services.GetService<ILogger<AppDbContextSeed>>();
            var settings = services.GetService<IOptions<AppSettings>>();

            new AppDbContextSeed()
                .SeedAsync(context, env, logger, settings)
                .Wait();
        })
        .MigrateDbContext<ConfigurationDbContext>((context, services) =>
        {
            new ConfigurationDbContextSeed()
                .SeedAsync(context, configuration)
                .Wait();
        });
    Log.Information("Starting web host ({ApplicationContext})...", AppName);
    host.Run();
    return 0;
}
catch(Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
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
        .Enrich.WithProperty("ApplicationContext", AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://localhost:8080" : logstashUrl)
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

// // Add services to the container.
// builder.Services.AddControllersWithViews();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

// app.UseAuthorization();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// app.Run();
