using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Bereich "Kompetenzen" (section id="skills").</summary>
public class SkillsSection
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Hervorgehobener Teil des Titels.</summary>
    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleHighlight { get; set; } = string.Empty;

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Intro { get; set; } = string.Empty;

    public List<SkillItem> Items { get; set; } = new();
}

/// <summary>Eine Kompetenz-Kachel.</summary>
public class SkillItem
{
    /// <summary>CSS-Klasse des Icons, z. B. "flaticon-pen".</summary>
    [StringLength(ContentLimits.IconClass, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(ContentLimits.TileTitle, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.TileText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;

    /// <summary>true = hervorgehobene Kachel (CSS-Klasse "active").</summary>
    public bool Highlighted { get; set; }
}
