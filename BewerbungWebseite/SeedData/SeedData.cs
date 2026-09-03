using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BewerbungsSeite.Data;

namespace BewerbungsSeite.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new RazorPagesMovieContext(
            serviceProvider.GetRequiredService<DbContextOptions<RazorPagesMovieContext>>()))
        {
            if (context == null)
            {
                throw new ArgumentNullException("Null RazorPagesMovieContext");
            }

            if (!context.AdminUser.Any())
            {
                var accounts = serviceProvider.GetRequiredService<IConfiguration>()
                    .GetSection("AdminAccounts")
                    .Get<List<AdminAccountSeed>>() ?? new List<AdminAccountSeed>();

                var hasher = new PasswordHasher<AdminUser>();

                foreach (var account in accounts)
                {
                    if (string.IsNullOrWhiteSpace(account.Username) || string.IsNullOrWhiteSpace(account.Password))
                    {
                        continue;
                    }

                    var user = new AdminUser { Username = account.Username };
                    user.PasswordHash = hasher.HashPassword(user, account.Password);
                    context.AdminUser.Add(user);
                }

                context.SaveChanges();
            }
        }
    }

    private class AdminAccountSeed
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}