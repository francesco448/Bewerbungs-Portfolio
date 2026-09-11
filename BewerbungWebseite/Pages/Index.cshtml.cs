using Microsoft.AspNetCore.Mvc.RazorPages;
using BewerbungsSeite.Models.SiteContent;
using BewerbungsSeite.Services;

namespace BewerbungsSeite.Pages
{
    public class IndexModel(IPageContentService pageContentService) : PageModel
    {
        /// <summary>Der aus der Datenbank geladene Seiteninhalt.</summary>
        public PageContent PageContent { get; private set; } = new();

        /// <summary>Auf der oeffentlichen Seite nie aktiv; nur die Admin-Seite rendert im Editiermodus.</summary>
        public bool EditMode => false;

        public async Task OnGetAsync()
        {
            PageContent = await pageContentService.GetAsync(HttpContext.RequestAborted);
        }
    }
}
