using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityService.Configuration;
//using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class ConfigurationDbContextSeed
    {
        public async Task SeedAsync(ConfigurationDbContext context, IConfiguration configuration)
        {
            var clientUrls = new Dictionary<string, string>();

            clientUrls.Add("Mvc", configuration.GetValue<string>("MvcClient"));
            clientUrls.Add("BasketService", configuration.GetValue<string>("BasketServiceClient"));
            clientUrls.Add("OrderService", configuration.GetValue<string>("OrderServiceClient"));

            if(!context.Clients.Any())
            {
                foreach(var client in Config.GetClients(clientUrls))
                {
                    context.Clients.Add(client.ToEntity());
                }
                await context.SaveChangesAsync();
            }
            else
            {
                List<ClientRedirectUri> oldRedirects = (await context.Clients.Include(c => c.RedirectUris).ToListAsync())
                    .SelectMany(c => c.RedirectUris)
                    .Where(ru => ru.RedirectUri.EndsWith("/o2c.html"))
                    .ToList();
                
                if(oldRedirects.Any())
                {
                    foreach(var ru in oldRedirects)
                    {
                        ru.RedirectUri = ru.RedirectUri.Replace("/o2c.html", "/oauth2-redirect.html");
                        context.Update(ru.Client);
                    }
                    await context.SaveChangesAsync();
                }
            }
            if(!context.IdentityResources.Any())
            {
                foreach(var resource in Config.GetResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                await context.SaveChangesAsync();
            }
            if(!context.ApiResources.Any())
            {
                foreach(var api in Config.GetApis())
                {
                    context.ApiResources.Add(api.ToEntity());
                }
                await context.SaveChangesAsync();
            }
        }
    }
}