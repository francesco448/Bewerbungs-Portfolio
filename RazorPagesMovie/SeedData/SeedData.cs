using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RazorPagesMovie.Data;

namespace RazorPagesMovie.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new RazorPagesMovieContext(
            serviceProvider.GetRequiredService<DbContextOptions<RazorPagesMovieContext>>()))
        {
            if (context == null || context.Movie == null)
            {
                throw new ArgumentNullException("Null RazorPagesMovieContext");
            }

            if (!context.Movie.Any())
            {
                context.Movie.AddRange(
new Movie { Title = "When Harry Met Sally", ReleaseDate = DateTime.Parse("1989-2-12"), Genre = "Romantic Comedy", Price = 7.99M, Rating = "R" },
new Movie { Title = "Ghostbusters", ReleaseDate = DateTime.Parse("1984-3-13"), Genre = "Comedy", Price = 8.99M, Rating = "G" },
new Movie { Title = "Ghostbusters 2", ReleaseDate = DateTime.Parse("1986-2-23"), Genre = "Comedy", Price = 9.99M, Rating = "G" },
new Movie { Title = "Rio Bravo", ReleaseDate = DateTime.Parse("1959-4-15"), Genre = "Western", Price = 3.99M, Rating = "NR" }
                );
                context.SaveChanges();
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