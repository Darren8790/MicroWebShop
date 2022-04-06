using System.Security.Claims;
using System.Security.Principal;
using WebMVC.ViewModels;

namespace WebMVC.Services;

public class IdentityParser : IIdentityParser<AppUser>
{
    public AppUser Parse(IPrincipal principal)
    {
        if(principal is ClaimsPrincipal claimsPrincipal)
        {
            return new AppUser
            {
                Id = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "",
                FirstName = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "first_name")?.Value ?? "",
                LastName = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "last_name")?.Value ?? "",
                Street = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "address_street")?.Value ?? "",
                City = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "address_city")?.Value ?? "", 
                Country = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "address_country")?.Value ?? "",
                PostCode = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "address_post_code")?.Value ?? "",
                Email = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
            };
        }
        throw new ArgumentException(message: "Principal must be ClaimsPrincipal", paramName: nameof(principal));
    }
}
