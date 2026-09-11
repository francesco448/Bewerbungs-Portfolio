using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Bereich "Dokumente und Zertifikate" (section id="documents").</summary>
public class DocumentsSection
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.Heading, ErrorMessage = ContentLimits.TooLongMessage)]
    public string TitleHighlight { get; set; } = string.Empty;

    public List<DocumentItem> Items { get; set; } = new();
}

/// <summary>Ein Dokument bzw. Zertifikat mit Link auf die PDF-Datei.</summary>
public class DocumentItem
{
    [StringLength(ContentLimits.CardTitle, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Title { get; set; } = string.Empty;

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Description { get; set; } = string.Empty;

    [StringLength(ContentLimits.Url, ErrorMessage = ContentLimits.TooLongMessage)]
    public string FileUrl { get; set; } = string.Empty;

    [StringLength(ContentLimits.ButtonLabel, ErrorMessage = ContentLimits.TooLongMessage)]
    public string LinkText { get; set; } = string.Empty;
}
