using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Bereich "Berufliche Erfahrung" (section id="experience").</summary>
public class ExperienceSection
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleHighlight { get; set; } = string.Empty;

    public List<ExperienceItem> Items { get; set; } = new();

    [StringLength(ContentLimits.ButtonLabel, ErrorMessage = ContentLimits.TooLongMessage)]
    public string ToggleMoreText { get; set; } = string.Empty;

    [StringLength(ContentLimits.ButtonLabel, ErrorMessage = ContentLimits.TooLongMessage)]
    public string ToggleLessText { get; set; } = string.Empty;
}

/// <summary>Ein Eintrag der Erfahrungs-Timeline.</summary>
public class ExperienceItem
{
    [StringLength(ContentLimits.CardTitle, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.Meta, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Meta { get; set; } = string.Empty;

    [StringLength(ContentLimits.DetailHeading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string DetailHeading { get; set; } = string.Empty;

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;

    /// <summary>true = erst nach Klick auf "Mehr erfahren" sichtbar.</summary>
    public bool Collapsed { get; set; }
}
