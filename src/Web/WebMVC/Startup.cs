using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using WebMVC.Infrastructure;
using WebMVC.Services;
using WebMVC.ViewModels;

namespace WebMVC;

public class Startup
{
    public IConfiguration Configuration;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews()
            .Services
            .AddCustomMvc(Configuration)
            .AddHttpClientServices(Configuration);
        services.AddControllers();
        services.AddCustomAuthentication(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
        if(env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var pathBase = Configuration["PATH_BASE"];
        if(!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseStaticFiles();
        app.UseSession();
        app.UseCookiePolicy(new CookiePolicyOptions {MinimumSameSitePolicy = SameSiteMode.Lax});
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default", "{controller=Catalogue}/{action=Index}/{id?}");
            endpoints.MapControllers();
        });
        
    }
}
static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<AppSettings>(configuration);
        services.AddSession();
        services.AddDistributedMemoryCache();

        if(configuration.GetValue<string>("IsClusterEnv") == bool.TrueString)
        {
            services.AddDataProtection(opts =>
            {
                opts.ApplicationDiscriminator = "microwebshop.webmvc";
            })
            .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(configuration["DPConnectionString"]), "DataProtection-Keys");
        }
        return services;
    }

    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<ClientAuthorizationDelegator>();
        services.AddTransient<ClientRequestIdDelegator>();
        
        // Http client services
        services.AddHttpClient("extendedhandlerlifetime").SetHandlerLifetime(TimeSpan.FromMinutes(5));
        services.AddHttpClient<IBasketService, BasketService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddHttpMessageHandler<ClientAuthorizationDelegator>();
        services.AddHttpClient<ICatalogueService, CatalogueService>();
        services.AddHttpClient<IOrderService, OrderService>()
            .AddHttpMessageHandler<ClientAuthorizationDelegator>()
            .AddHttpMessageHandler<ClientRequestIdDelegator>();

        services.AddTransient<IIdentityParser<AppUser>, IdentityParser>();

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var identityUrl = configuration.GetValue<string>("IdentityUrl");
        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
        .AddOpenIdConnect(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = identityUrl.ToString();
            options.ClientId = "mvc";
            options.ClientSecret = "secret";
            options.ResponseType = "code id_token";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("orderservice");
            options.Scope.Add("basketservice");
        });
        return services;
    }
}