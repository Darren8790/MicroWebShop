namespace WebMVC.Infrastructure;

public class ClientRequestIdDelegator : DelegatingHandler
{
    public ClientRequestIdDelegator()
    {
        
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if(request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
        {
            if(!request.Headers.Contains("x-requestid"))
            {
                request.Headers.Add("x-requestid", Guid.NewGuid().ToString());
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}