using System.Text.RegularExpressions;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using IdentityService.Extensions;

namespace IdentityService.Data
{
    using Microsoft.Extensions.Logging;
    public class AppDbContextSeed
    {
        private readonly IPasswordHasher<AppUser> _passwordHasher = new PasswordHasher<AppUser>();

        public async Task SeedAsync(AppDbContext context, IWebHostEnvironment env,
            ILogger<AppDbContextSeed> logger, IOptions<AppSettings> settings, int? retry = 0)
        {
            int retryForAvailability = retry.Value;
            try
            {
                var useCustomData = settings.Value.UseCustomizationData;
                var contentRootPath = env.ContentRootPath;
                var webroot = env.WebRootPath;

                if(!context.Users.Any())
                {
                    context.Users.AddRange(useCustomData
                        ? GetUsersFromFile(contentRootPath, logger)
                        : GetDefaultUser());
                    await context.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                if(retryForAvailability < 10)
                {
                    retryForAvailability++;
                    logger.LogError(ex, "Exception Error while migrating {DbContextName}", nameof(AppDbContext));
                    await SeedAsync(context, env, logger, settings, retryForAvailability);
                }
            }
        }

        private IEnumerable<AppUser> GetUsersFromFile(string contentRootPath, ILogger logger)
        {
            string csvFileUsers = Path.Combine(contentRootPath, "Setup", "Users.csv");
            if(!File.Exists(csvFileUsers))
            {
                return GetDefaultUser();
            }

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = {
                    "firstname", "lastname", "street", "city", "country",
                    "postcode", "username", "email", "normalizedemail", "normalizedusername",
                    "password"
                };
                csvheaders = GetHeaders(requiredHeaders, csvFileUsers);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Exception Error: {Message}");
                return GetDefaultUser();
            }

            List<AppUser> users = File.ReadAllLines(csvFileUsers)
                .Skip(1)
                .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                .SelectTry(column => CreateApplicationUser(column, csvheaders))
                .OnCaughtException(ex => {logger.LogError(ex, "Exception Error: {Message}", ex.Message); return null;})
                .Where(x => x != null)
                .ToList();
            return users;
        }

        private AppUser CreateApplicationUser(string[] column, string[] headers)
        {
            if(column.Count() != headers.Count())
            {
                throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = column[Array.IndexOf(headers, "firstname")].Trim('"').Trim(),
                LastName = column[Array.IndexOf(headers, "lastname")].Trim('"').Trim(),
                Street = column[Array.IndexOf(headers, "street")].Trim('"').Trim(),
                City = column[Array.IndexOf(headers, "city")].Trim('"').Trim(),
                Country = column[Array.IndexOf(headers, "country")].Trim('"').Trim(),
                PostCode = column[Array.IndexOf(headers, "postcode")].Trim('"').Trim(),
                UserName = column[Array.IndexOf(headers, "username")].Trim('"').Trim(),
                Email = column[Array.IndexOf(headers, "email")].Trim('"').Trim(),
                NormalizedEmail = column[Array.IndexOf(headers, "normalizedemail")].Trim('"').Trim(),
                NormalizedUserName = column[Array.IndexOf(headers, "normailizedusername")].Trim('"').Trim(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = column[Array.IndexOf(headers, "password")].Trim('"').Trim(),
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
            return user;
        }

        private IEnumerable<AppUser> GetDefaultUser()
        {
            var user = new AppUser()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Demouser",
                LastName = "Demolast",
                Street = "123 Street",
                City = "Edinburgh",
                Country = "Scotland",
                PostCode = "EH14 1DJ",
                UserName = "demouser@test.com",
                Email = "demouser@test.com",
                NormalizedEmail = "DEMOUSER@TEST.COM",
                NormalizedUserName = "DEMOUSER@TEST.COM",
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word1");

            return new List<AppUser>()
            {
                user
            };
        }

        static string[] GetHeaders(string[] requiredHeaders, string csvfile)
        {
            string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');
            if(csvheaders.Count() != requiredHeaders.Count())
            {
                throw new Exception($"requiredHeader count '{ requiredHeaders.Count()}' is different than read header '{csvheaders.Count()}'");
            }

            foreach(var requiredHeader in requiredHeaders)
            {
                if(!csvheaders.Contains(requiredHeader))
                {
                    throw new Exception($"Does not contain required header '{requiredHeader}'");
                }
            }
            return csvheaders;
        }
    }
}