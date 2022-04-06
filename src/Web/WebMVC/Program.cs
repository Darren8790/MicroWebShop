using Microsoft.AspNetCore;
using Serilog;
using WebMVC;
using WebMVC.Infrastructure;
using WebMVC.Services;
using WebMVC.ViewModels;

//var builder = WebApplication.CreateBuilder(args);
var configuration = GetConfiguration();
Log.Logger = SerilogLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    var host = BuildWebHost(configuration, args);
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    host.Run();
    return 0;
}
catch(Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})", Program.AppName);
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
    .UseSerilog()
    .Build();

Serilog.ILogger SerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashUrl"];
    var cfg = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    if(!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        cfg.WriteTo.Http(logstashUrl);
    }
    return cfg.CreateLogger();
}

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
    return builder.Build();
}

public partial class Program
{
    private static readonly string _namespace = typeof(Startup).Namespace;
    public static readonly string AppName = _namespace.Substring(_namespace.LastIndexOf('.') + 1);
}
// Add services to the container.
// builder.Services.AddControllersWithViews();
// builder.Services.AddControllers();

// builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// builder.Services.AddTransient<ClientAuthorizationDelegator>();
// builder.Services.AddTransient<ClientRequestIdDelegator>();

// builder.Services.AddHttpClient<ICatalogueService, CatalogueService>();
// builder.Services.AddHttpClient<IBasketService, BasketService>().AddHttpMessageHandler<ClientAuthorizationDelegator>();
// builder.Services.AddHttpClient<IOrderService, OrderService>();
// builder.Services.AddTransient<IIdentityParser<AppUser>, IdentityParser>();

// // builder.Services.Configure<AppSettings>(configuration);

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// //app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

// app.UseAuthorization();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Catalogue}/{action=Index}/{id?}");

// app.Run();
