using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace WebMVC.Infrastructure;

public class ClientAuthorizationDelegator : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientAuthorizationDelegator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMsg, CancellationToken cancellationToken)
    {
        var authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
        if(!string.IsNullOrEmpty(authHeader))
        {
            requestMsg.Headers.Add("Authorization", new List<string>() {authHeader});
        }
        
        var token = await GetToken();
        if(token != null)
        {
            requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(requestMsg, cancellationToken);
    }

    async Task<string> GetToken()
    {
        const string ACCESS_TOKEN = "access_token";
        return await _httpContextAccessor.HttpContext.GetTokenAsync(ACCESS_TOKEN);
    }
}