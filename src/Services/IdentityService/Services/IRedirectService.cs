namespace IdentityService.Services
{
    public interface IRedirectService
    {
        string ExtractedRedirectUriFromReturnUrl(string url);
    }
}