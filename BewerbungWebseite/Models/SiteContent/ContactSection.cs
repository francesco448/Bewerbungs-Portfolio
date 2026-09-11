using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>
/// Bereich "Kontakt" (section id="contact"). Enthaelt nur die Texte;
/// das Formular selbst bleibt Bestandteil der Seite und ist nicht editierbar.
/// </summary>
public class ContactSection
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleHighlight { get; set; } = string.Empty;

    public List<ContactInfoItem> InfoBoxes { get; set; } = new();

    /// <summary>Einleitungstext oberhalb der Formularfelder.</summary>
    [StringLength(ContentLimits.FormIntro, ErrorMessage = ContentLimits.TooLongMessage)]
    public string FormHeading { get; set; } = string.Empty;
}

/// <summary>Eine Kontaktangabe (Standort, Telefon, E-Mail, ...).</summary>
public class ContactInfoItem
{
    /// <summary>CSS-Klasse des Icons, z. B. "flaticon-call".</summary>
    [StringLength(ContentLimits.IconClass, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(ContentLimits.FactLabel, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.FactValue, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional: macht den Text zum Link, z. B. "tel:0797367355".</summary>
    [StringLength(ContentLimits.Url, ErrorMessage = ContentLimits.TooLongMessage)]
    public string LinkUrl { get; set; } = string.Empty;
}
