using Microsoft.EntityFrameworkCore;

namespace BewerbungsSeite.Data;

public class RazorPagesMovieContext(DbContextOptions<RazorPagesMovieContext> options) : DbContext(options)
{
    public DbSet<BewerbungsSeite.Models.AdminUser> AdminUser { get; set; } = default!;
    public DbSet<BewerbungsSeite.Models.ContactMessage> ContactMessage { get; set; } = default!;
}