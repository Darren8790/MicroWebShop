//using IdentityServer4.EntityFramework.Entities;
using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityService.Configuration
{
    public class Config
    {
        // ApiResources are defined here 
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("orderservice", "Order Service"),
                new ApiResource("basketservice", "Basket Service"),
            };
        }

        //Identity resources are definded here (user details such as Id, name, email)
        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        // The clients that want access are defined here (scopes)
        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientsUrl)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{clientsUrl["Mvc"]}",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orderservice",
                        "basketservice"

                    },
                    AccessTokenLifetime = 60*60*2,
                    IdentityTokenLifetime = 60*60*2
                },
                new Client
                {
                    ClientId = "orderserviceswagger",
                    ClientName = "Order Service Swagger",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = {$"{clientsUrl["OrderService"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = {$"{clientsUrl["OrderService"]}/swagger/" },
                    AllowedScopes =
                    {
                        "orderservice"
                    }
                },
                new Client
                {
                    ClientId = "basketserviceswagger",
                    ClientName = "Basket Service Swagger",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = {$"{clientsUrl["BasketService"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = {$"{clientsUrl["BasketService"]}/swagger/" },
                    AllowedScopes =
                    {
                        "basketservice"
                    }
                }                
            };
        }
    }
}