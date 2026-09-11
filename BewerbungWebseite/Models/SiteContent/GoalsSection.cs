using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Bereich "Meine Ziele" (section id="goals").</summary>
public class GoalsSection
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleHighlight { get; set; } = string.Empty;

    public List<GoalItem> Items { get; set; } = new();
}

/// <summary>Eine Ziel-Kachel.</summary>
public class GoalItem
{
    /// <summary>CSS-Klasse des Icons, z. B. "flaticon-growth".</summary>
    [StringLength(ContentLimits.IconClass, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(ContentLimits.TileTitle, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.TileText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;

    /// <summary>true = hervorgehobene Kachel (CSS-Klasse "active").</summary>
    public bool Highlighted { get; set; }
}
