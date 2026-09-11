using System.Text.Json;
using BewerbungsSeite.Models.SiteContent;

namespace BewerbungsSeite.Services;

/// <summary>
/// Wandelt den Seiteninhalt zwischen Objektmodell und UTF-8-JSON um, so wie er
/// im Feld PageContent.Content abgelegt wird.
/// </summary>
public static class PageContentSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static byte[] Serialize(PageContent content) =>
        JsonSerializer.SerializeToUtf8Bytes(content, Options);

    /// <summary>
    /// Liest den Inhalt zurueck. Fehlende oder unbekannte Felder fuehren nicht zu
    /// einem Fehler: unbekannte werden ignoriert, fehlende erhalten ihren Initialwert.
    /// </summary>
    public static PageContent Deserialize(byte[]? content)
    {
        if (content is null || content.Length == 0)
        {
            return new PageContent();
        }

        var result = JsonSerializer.Deserialize<PageContent>(content, Options) ?? new PageContent();
        return Normalize(result);
    }

    /// <summary>
    /// Stellt sicher, dass kein Bereich null ist, auch wenn ein aelterer
    /// Datensatz ihn noch nicht kennt oder ihn ausdruecklich als null enthaelt.
    /// </summary>
    private static PageContent Normalize(PageContent content)
    {
        content.Hero ??= new HeroSection();
        content.Hero.TitleLines ??= new List<string>();
        content.Hero.PrimaryButton ??= new LinkItem();
        content.Hero.SecondaryButton ??= new LinkItem();
        content.Hero.SocialLinks ??= new List<LinkItem>();

        content.About ??= new AboutSection();
        content.About.Facts ??= new List<FactItem>();

        content.Skills ??= new SkillsSection();
        content.Skills.Items ??= new List<SkillItem>();

        content.Projects ??= new ProjectsSection();
        content.Projects.Items ??= new List<ProjectItem>();
        foreach (var project in content.Projects.Items)
        {
            project.Details ??= new List<DetailBlock>();
            project.Links ??= new List<LinkItem>();
        }

        content.Goals ??= new GoalsSection();
        content.Goals.Items ??= new List<GoalItem>();

        content.Experience ??= new ExperienceSection();
        content.Experience.Items ??= new List<ExperienceItem>();

        content.Documents ??= new DocumentsSection();
        content.Documents.Items ??= new List<DocumentItem>();

        content.Contact ??= new ContactSection();
        content.Contact.InfoBoxes ??= new List<ContactInfoItem>();

        return content;
    }
}
