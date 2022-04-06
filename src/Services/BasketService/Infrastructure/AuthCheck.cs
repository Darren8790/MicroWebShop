using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BasketService.Infrastructure;

public class AuthCheck : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuth = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if(!hasAuth) return;

        operation.Responses.TryAdd("401", new OpenApiResponse {Description = "Unauthorized"});
        operation.Responses.TryAdd("403", new OpenApiResponse {Description = "Forbidden"});

        var oAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "oauth2"}
        };

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                [oAuthScheme] = new [] {"basketservice"}
            }
        };
    }
}