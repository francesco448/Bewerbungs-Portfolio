using Microsoft.EntityFrameworkCore;
using BewerbungsSeite.Models;

namespace BewerbungsSeite.Data;

public class RazorPagesMovieContext(DbContextOptions<RazorPagesMovieContext> options) : DbContext(options)
{
    public DbSet<AdminUser> AdminUser { get; set; } = default!;
    public DbSet<ContactMessage> ContactMessage { get; set; } = default!;
    public DbSet<PageContentEntity> PageContent { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Es gibt genau eine Zeile mit fester Id, deshalb keine Identity-Spalte.
        modelBuilder.Entity<PageContentEntity>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
    }
}
