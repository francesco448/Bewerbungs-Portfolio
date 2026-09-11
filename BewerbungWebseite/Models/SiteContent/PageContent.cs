using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>
/// Wurzelobjekt des gesamten Seiteninhalts. Wird als JSON serialisiert und
/// in einer einzigen Zeile der Tabelle PageContent abgelegt.
/// Neue Bereiche kommen als zusaetzliche Property dazu; alte Datensaetze
/// kennen die Property noch nicht und erhalten beim Laden den Initialwert.
/// </summary>
public class PageContent
{
    public HeroSection Hero { get; set; } = new();
    public AboutSection About { get; set; } = new();
    public SkillsSection Skills { get; set; } = new();
    public ProjectsSection Projects { get; set; } = new();
    public GoalsSection Goals { get; set; } = new();
    public ExperienceSection Experience { get; set; } = new();
    public DocumentsSection Documents { get; set; } = new();
    public ContactSection Contact { get; set; } = new();
}

/// <summary>Ein Link mit Beschriftung, z. B. in der Hero-Section oder auf einer Projektkarte.</summary>
public class LinkItem
{
    [StringLength(ContentLimits.ButtonLabel, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;

    [StringLength(ContentLimits.Url, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Url { get; set; } = string.Empty;
}

/// <summary>Ein Abschnitt aus Zwischentitel und Text, z. B. "TECH:" auf einer Projektkarte.</summary>
public class DetailBlock
{
    [StringLength(ContentLimits.DetailHeading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Heading { get; set; } = string.Empty;

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;
}
