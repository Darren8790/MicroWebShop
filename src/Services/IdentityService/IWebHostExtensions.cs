using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using Microsoft.Data.SqlClient;
using Polly;

namespace IdentityService
{
    public static class IWebHostExtensions
    {
        public static bool IsInKubernetes(this IWebHost webHost)
        {
            var config = webHost.Services.GetService<IConfiguration>();
            var orchestratorType = config.GetValue<string>("OrchestratorType");
            return orchestratorType?.ToUpper() == "K8S";
        }
        public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            var underK8S = webHost.IsInKubernetes();
            using(var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                    if(underK8S)
                    {
                        InvokeSeeder(seeder, context, services);
                    }
                    else
                    {
                        var retries = 10;
                        var retry = Policy.Handle<SqlException>()
                            .WaitAndRetry(
                                retryCount: retries,
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                onRetry: (exception, timeSpan, retry, ctx) =>
                                {
                                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                                });
                        retry.Execute(() => InvokeSeeder(seeder, context, services));
                    }
                    logger.LogInformation("Migrated db of context {DbContextName}", typeof(TContext).Name);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the db used on context {DbContextName}", typeof(TContext).Name);
                    if(underK8S)
                    {
                        throw;
                    }
                }
            }
            return webHost;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services) where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}