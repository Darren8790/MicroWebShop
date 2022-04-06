using System.IdentityModel.Tokens.Jwt;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BasketService.Controllers;
using BasketService.Infrastructure;
using BasketService.Repository;
using BasketService.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace BasketService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public virtual IServiceProvider ConfigureServices(IServiceCollection services)
    {
        RegisterAppInsights(services);
        services.AddControllers()
            .AddApplicationPart(typeof(BasketController).Assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MicroWebShop - Basket Service HTTP",
                Version = "v1",
                Description = "Basket service HTTP API"
            });
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                        TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                        {
                            {"basketservice", "Basket Service"}
                        }
                    }
                }
            });
            options.OperationFilter<AuthCheck>();
        });
        
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
            builder => builder
            .SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        });
        ConfigureAuthService(services);
        services.AddCustomHealthCheck(Configuration);
        services.Configure<BasketSettings>(Configuration);
        services.AddSingleton<ConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;
            var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);
            return ConnectionMultiplexer.Connect(configuration);
        });

        // var redis = ConnectionMultiplexer.Connect("localhost");
        // services.AddScoped(s => redis.GetDatabase());

        // services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
        // {
        //     var factory = new ConnectionFactory()
        //     {
        //         HostName = Configuration["EventBusConnection"],
        //         DispatchConsumersAsync = true
        //     };
        //     if(!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
        //     {
        //         factory.UserName = Configuration["EventBusUserName"];
        //     }
        //     if(!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
        //     {
        //         factory.Password = Configuration["EventBusPassword"];
        //     }
        //     return new DefaultRabbitMQPersistentConnection(factory);
        // });

        RegisterEventBus(services);
        // services.AddCors(options =>
        // {
        //     options.AddPolicy("CorsPolicy",
        //     builder => builder
        //     .SetIsOriginAllowed((host) => true)
        //     .AllowAnyMethod()
        //     .AllowAnyHeader()
        //     .AllowCredentials());
        // });
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<IBasketRepo, RedisRepo>();
        services.AddTransient<IUserIdentity, UserIdentity>();
        services.AddOptions();

        var container = new ContainerBuilder();
        container.Populate(services);
        return new AutofacServiceProvider(container.Build());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseCors("CorsPolicy");
        var pathBase = Configuration["PATH_BASE"];
        if(!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseSwagger()
        .UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "BasketService V1");
            setup.OAuthClientId("basketserviceswagger");
            setup.OAuthAppName("Basket Service");
        });

        app.UseRouting();
        //app.UseCors("CorsPolicy");
        ConfigureAuth(app);
        app.UseStaticFiles();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
        ConfigureEventBus(app);
    }

    private void RegisterAppInsights(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry(Configuration);
    }

    private void ConfigureAuthService(IServiceCollection services)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
        var identityUrl = Configuration.GetValue<string>("IdentityUrl");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "basketservice";
        });
    }

    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    private void RegisterEventBus(IServiceCollection services)
    {

    }

    private void ConfigureEventBus(IApplicationBuilder app)
    {

    }
}

public static class CustomExtensions
{
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var healthBuiler = services.AddHealthChecks();
        healthBuiler.AddCheck("self", () => HealthCheckResult.Healthy());
        healthBuiler.AddRedis(
            configuration["ConnectionString"],
            name: "redis-check",
            tags: new string[] {"redis"}
        );
        // healthBuiler.AddRabbitMQ(
        //     configuration["EventBusConnection"],
        //     name: "rabbit-event-bus",
        //     tags: new string[] {"rabbitmqbus"}
        // );
        return services;
    }
}