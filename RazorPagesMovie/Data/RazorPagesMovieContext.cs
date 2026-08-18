using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Data;

public class RazorPagesMovieContext(DbContextOptions<RazorPagesMovieContext> options) : DbContext(options)
{
    public DbSet<RazorPagesMovie.Models.Movie> Movie { get; set; } = default!;
    public DbSet<RazorPagesMovie.Models.AdminUser> AdminUser { get; set; } = default!;
    public DbSet<RazorPagesMovie.Models.ContactMessage> ContactMessage { get; set; } = default!;
}