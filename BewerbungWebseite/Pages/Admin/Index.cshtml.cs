using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BewerbungsSeite.Models.SiteContent;
using BewerbungsSeite.Services;

namespace BewerbungsSeite.Pages.Admin;

public class IndexModel(IPageContentService pageContentService) : PageModel
{
    public PageContent PageContent { get; private set; } = new();

    /// <summary>Wird ueber ?edit=1 eingeschaltet, wenn der Benutzer auf das Edit-Icon klickt.</summary>
    public bool EditMode { get; private set; }

    public async Task OnGetAsync(bool edit)
    {
        EditMode = edit;
        PageContent = await pageContentService.GetAsync(HttpContext.RequestAborted);
    }

    /// <summary>
    /// Liefert einen einzelnen Bereich neu gerendert zurueck. Wird nach dem
    /// Hinzufuegen, Verschieben oder Loeschen eines Eintrags aufgerufen, damit
    /// die Seite ohne vollstaendiges Neuladen wieder stimmt.
    /// </summary>
    public async Task<IActionResult> OnGetSectionAsync(string name)
    {
        var content = await pageContentService.GetAsync(HttpContext.RequestAborted);

        return name switch
        {
            "hero" => Partial("Sections/_Hero", SectionView.For(content.Hero, true)),
            "about" => Partial("Sections/_About", SectionView.For(content.About, true)),
            "skills" => Partial("Sections/_Skills", SectionView.For(content.Skills, true)),
            "projects" => Partial("Sections/_Projects", SectionView.For(content.Projects, true)),
            "goals" => Partial("Sections/_Goals", SectionView.For(content.Goals, true)),
            "experience" => Partial("Sections/_Experience", SectionView.For(content.Experience, true)),
            "documents" => Partial("Sections/_Documents", SectionView.For(content.Documents, true)),
            "contact" => Partial("Sections/_Contact", SectionView.For(content.Contact, true)),
            _ => NotFound()
        };
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}
