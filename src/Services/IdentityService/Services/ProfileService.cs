using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        async public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));
            var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault().Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            if(user == null)
            throw new ArgumentException("Incorrect subject Id");

            var claims = GetClaimsFromUser(user);
            context.IssuedClaims = claims.ToList();
        }

        private IEnumerable<Claim> GetClaimsFromUser(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.Id),
                new Claim(JwtClaimTypes.PreferredUserName, user.UserName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            if(!string.IsNullOrWhiteSpace(user.FirstName))
            claims.Add(new Claim("first_name", user.FirstName));

            if(!string.IsNullOrWhiteSpace(user.LastName))
            claims.Add(new Claim("last_name", user.LastName));

            if(!string.IsNullOrWhiteSpace(user.Street))
            claims.Add(new Claim("address_street", user.Street));

            if(!string.IsNullOrWhiteSpace(user.City))
            claims.Add(new Claim("address_city", user.City));

            if(!string.IsNullOrWhiteSpace(user.Country))
            claims.Add(new Claim("address_country", user.Country));

            if(!string.IsNullOrWhiteSpace(user.PostCode))
            claims.Add(new Claim("address_postcode", user.PostCode));

            if(_userManager.SupportsUserEmail)
            {
                claims.AddRange(new[]
                {
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                });
            }
            return claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));
            var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = false;
            if(user != null)
            {
                if(_userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                    if(security_stamp != null)
                    {
                        var db_security_stamp = await _userManager.GetSecurityStampAsync(user);
                        if(db_security_stamp != security_stamp)
                        return;
                    }
                }
            }
            context.IsActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTime.Now;
        }
    }
}