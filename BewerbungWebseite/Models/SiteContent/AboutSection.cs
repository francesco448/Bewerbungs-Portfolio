using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Bereich "Ueber mich" (section id="about").</summary>
public class AboutSection
{
    [StringLength(ContentLimits.Url, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Image { get; set; } = string.Empty;

    [StringLength(ContentLimits.AltText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string ImageAlt { get; set; } = string.Empty;

    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleLine1 { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleLine2 { get; set; } = string.Empty;

    /// <summary>Hervorgehobener Teil der zweiten Titelzeile.</summary>
    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleLine2Highlight { get; set; } = string.Empty;

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;

    public List<FactItem> Facts { get; set; } = new();
}

/// <summary>Eine Zeile der Kurzuebersicht rechts unter dem Text.</summary>
public class FactItem
{
    [StringLength(ContentLimits.FactLabel, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Label { get; set; } = string.Empty;

    [StringLength(ContentLimits.FactValue, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Value { get; set; } = string.Empty;
}
