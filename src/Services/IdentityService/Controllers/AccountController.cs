using System.Security.Claims;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityService.Models;
using IdentityService.Models.AccountViewModels;
using IdentityService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService<AppUser> _loginService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(ILoginService<AppUser> loginService, IIdentityServerInteractionService interaction, IClientStore clientStore,
            ILogger<AccountController> logger, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _loginService = loginService;
            _interaction = interaction;
            _clientStore = clientStore;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if(context?.IdP != null)
            {
                throw new NotImplementedException("External login is not implemented!");
            }

            var viewModel = await BuildLoginViewModelAsync(returnUrl, context);
            ViewData["ReturnUrl"] = returnUrl;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _loginService.FindByUsername(model.Email);
                if(await _loginService.ValidateCredentials(user, model.Password))
                {
                    var tokenLifetime = _configuration.GetValue("TokenLifetimeMinutes", 120);
                    var props = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(tokenLifetime),
                        AllowRefresh = true,
                        RedirectUri = model.ReturnUrl
                    };

                    if(model.RememberMe)
                    {
                        var permanentTokenLifetime = _configuration.GetValue("PermanentTokenLifetime", 365);
                        props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(permanentTokenLifetime);
                        props.IsPersistent = true;
                    };

                    await _loginService.SignInAsync(user, props);

                    if(_interaction.IsValidReturnUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return Redirect("~/");
                }
                ModelState.AddModelError("", "Invalid username or password");
            }

            var viewModel = await BuildLoginViewModelAsync(model);
            ViewData["ReturnUrl"] = model.ReturnUrl;
            return View(viewModel);
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl, AuthorizationRequest context)
        {
            var allowLocal = true;
            if(context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if(client != null)
                {
                    allowLocal = client.EnableLocalLogin;
                }
            }

            return new LoginViewModel
            {
                ReturnUrl = returnUrl,
                Email = context?.LoginHint,
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginViewModel model)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var viewModel = await BuildLoginViewModelAsync(model.ReturnUrl, context);
            viewModel.Email = model.Email;
            viewModel.RememberMe = model.RememberMe;
            return viewModel;
        }

        // Displays logout
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            if(User.Identity.IsAuthenticated == false)
            {
                return await Logout(new LogoutViewModel {LogoutId = logoutId});
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if(context?.ShowSignoutPrompt == false)
            {
                return await Logout(new LogoutViewModel {LogoutId = logoutId});
            }

            var viewModel = new LogoutViewModel
            {
                LogoutId = logoutId
            };
            return View(viewModel);
        }

        // Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutViewModel model)
        {
            var idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                if(model.LogoutId == null)
                {
                    model.LogoutId = await _interaction.CreateLogoutContextAsync();
                }

                string url = "/Account/Logout?logoutId=" + model.LogoutId;
                try
                {
                    await HttpContext.SignOutAsync(idp, new AuthenticationProperties
                    {
                        RedirectUri = url
                    });
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Logout Error: {ExceptionMessage}", ex.Message);
                }
            }
            // Delete Auth token
            await HttpContext.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            // Sets UI rendering to see anonymous user 
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            // Get context info (client name, post logout redirect uri and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(model.LogoutId);
            return Redirect(logout?.PostLogoutRedirectUri);
        }

        public async Task<IActionResult> DeviceLogOut(String redirectUrl)
        {
            // Delete auth cookie
            await HttpContext.SignOutAsync();
            // Sets UI rendering to see anonymous user 
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return Redirect(redirectUrl);
        }

        // Get /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if(ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.User.FirstName,
                    LastName = model.User.LastName,
                    Street = model.User.Street,
                    City = model.User.City,
                    Country = model.User.Country,
                    PostCode = model.User.PostCode
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if(result.Errors.Count() > 0)
                {
                    AddErrors(result);
                    return View(model);
                }
            }

            if(returnUrl != null)
            {
                if(HttpContext.User.Identity.IsAuthenticated)
                    return Redirect(returnUrl);
                else
                    if(ModelState.IsValid)
                    return RedirectToAction("login", "account", new {returnUrl = returnUrl});
                else
                    return View(model);
            }
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        public IActionResult Redirecting()
        {
            return View();
        }

        public void AddErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }
    }
}