using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RazorPagesMovieContext") ?? throw new InvalidOperationException("Connection string 'RazorPagesMovieContext' not found.")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
});

builder.Services.AddHttpClient();
builder.Services.AddHostedService<ContactMessageNotificationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RazorPagesMovieContext>();
    context.Database.Migrate();
    SeedData.Initialize(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapPost("/api/contact", async (ContactMessageRequest request, RazorPagesMovieContext context) =>
{
    var contactMessage = new ContactMessage
    {
        Name = request.Name ?? string.Empty,
        Email = request.Email ?? string.Empty,
        Phone = request.Phone,
        Subject = request.Subject,
        Message = request.Message ?? string.Empty
    };

    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(contactMessage, new ValidationContext(contactMessage), validationResults, validateAllProperties: true))
    {
        return Results.BadRequest(new { error = "Bitte alle Pflichtfelder korrekt ausfüllen." });
    }

    context.ContactMessage.Add(contactMessage);
    await context.SaveChangesAsync();

    return Results.Ok(new { success = true });
});

app.Run();

record ContactMessageRequest(string? Name, string? Email, string? Phone, string? Subject, string? Message);
