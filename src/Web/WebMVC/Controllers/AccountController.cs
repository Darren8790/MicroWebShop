using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers;

[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SignIn(string returnUrl)
    {
        var user = User as ClaimsPrincipal;
        var token = await HttpContext.GetTokenAsync("access_token");

        if(token != null)
        {
            ViewData["access_token"] = token;
        }
        return RedirectToAction(nameof(CatalogueController.Index), "Catalogue");
    }

    public async Task<IActionResult> Signout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        var homeUrl = Url.Action(nameof(CatalogueController.Index), "Catalogue");
        return new SignOutResult(OpenIdConnectDefaults.AuthenticationScheme,
            new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = homeUrl });
    }
}