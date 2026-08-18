using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages;

public class LoginModel : PageModel
{
    private readonly RazorPagesMovieContext _context;

    public LoginModel(RazorPagesMovieContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        var user = await _context.AdminUser.SingleOrDefaultAsync(u => u.Username == Username);
        var hasher = new PasswordHasher<AdminUser>();

        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, Password) == PasswordVerificationResult.Failed)
        {
            ErrorMessage = "Benutzername oder Passwort ist falsch.";
            return Page();
        }

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, user.Username) },
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return LocalRedirect(returnUrl ?? "/Admin");
    }
}
