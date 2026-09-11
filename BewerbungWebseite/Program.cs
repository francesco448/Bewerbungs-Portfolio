using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using BewerbungsSeite.Data;
using BewerbungsSeite.Models;
using BewerbungsSeite.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BewerbungsSeite") ?? throw new InvalidOperationException("Connection string 'BewerbungsSeite' not found.")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";

        // Nach 30 Minuten ohne Aktivitaet automatisch abmelden. Die Frist steht im
        // verschluesselten Ticket und wird serverseitig geprueft -- sie gilt also auch
        // dann, wenn der Browser das Cookie ueber einen Neustart hinweg wiederherstellt.
        // SlidingExpiration erneuert sie bei jedem Aufruf, beim Arbeiten fliegt niemand raus.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = true;

        // Das Cookie wird bei Aufrufen von fremden Seiten nicht mitgeschickt.
        options.Cookie.SameSite = SameSiteMode.Strict;

        // Aufrufe der API sollen 401 bekommen statt einer Weiterleitung auf die
        // Login-Seite -- sonst haelt der Editor eine abgelaufene Sitzung faelschlich
        // fuer eine erfolgreiche Speicherung.
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
});
builder.Services.AddControllers();

builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

builder.Services.AddHttpClient();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

builder.Services.AddScoped<IPageContentService, PageContentService>();
builder.Services.AddSingleton<IContactMessageNotificationQueue, ContactMessageNotificationQueue>();
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
app.MapControllers();

app.Run();
