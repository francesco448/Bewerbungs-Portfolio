using Microsoft.EntityFrameworkCore;
using BewerbungsSeite.Data;
using BewerbungsSeite.Models;
using BewerbungsSeite.Models.SiteContent;

namespace BewerbungsSeite.Services;

public interface IPageContentService
{
    /// <summary>Laedt den Seiteninhalt. Existiert noch keine Zeile, kommt ein leerer Inhalt zurueck.</summary>
    Task<PageContent> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Schreibt den kompletten Seiteninhalt in die eine Zeile zurueck.</summary>
    Task SaveAsync(PageContent content, CancellationToken cancellationToken = default);
}

public class PageContentService(RazorPagesMovieContext context) : IPageContentService
{
    public async Task<PageContent> GetAsync(CancellationToken cancellationToken = default)
    {
        var row = await context.PageContent
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == PageContentEntity.SingletonId, cancellationToken);

        return PageContentSerializer.Deserialize(row?.Content);
    }

    public async Task SaveAsync(PageContent content, CancellationToken cancellationToken = default)
    {
        var row = await context.PageContent
            .SingleOrDefaultAsync(p => p.Id == PageContentEntity.SingletonId, cancellationToken);

        if (row is null)
        {
            row = new PageContentEntity { Id = PageContentEntity.SingletonId };
            context.PageContent.Add(row);
        }

        row.Content = PageContentSerializer.Serialize(content);
        row.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
