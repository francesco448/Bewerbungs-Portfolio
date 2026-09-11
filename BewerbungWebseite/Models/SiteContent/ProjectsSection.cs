using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Bereich "Projekte" (section id="projects").</summary>
public class ProjectsSection
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleHighlight { get; set; } = string.Empty;

    public List<ProjectItem> Items { get; set; } = new();
}

/// <summary>Eine Projektkarte.</summary>
public class ProjectItem
{
    [StringLength(ContentLimits.Url, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Image { get; set; } = string.Empty;

    [StringLength(ContentLimits.AltText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string ImageAlt { get; set; } = string.Empty;

    [StringLength(ContentLimits.CardTitle, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optionaler fett gesetzter Auftakt vor der Beschreibung, z. B. "Ziel:".</summary>
    [StringLength(ContentLimits.DetailHeading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string DescriptionLead { get; set; } = string.Empty;

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Zwischentitel mit Text ("TECH:", "Meine Arbeit:", ...).</summary>
    public List<DetailBlock> Details { get; set; } = new();

    public List<LinkItem> Links { get; set; } = new();

    /// <summary>true = breite Karte ueber die volle Breite mit Bild links.</summary>
    public bool Wide { get; set; }
}
