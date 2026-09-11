using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>Startbereich der Seite (section id="hero").</summary>
public class HeroSection : IValidatableObject
{
    [StringLength(ContentLimits.Kicker, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Kicker { get; set; } = string.Empty;

    /// <summary>Grosse Titelzeilen. Die letzte Zeile wird farblich hervorgehoben.</summary>
    public List<string> TitleLines { get; set; } = new();

    [StringLength(ContentLimits.ParagraphText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string Intro { get; set; } = string.Empty;

    public LinkItem PrimaryButton { get; set; } = new();
    public LinkItem SecondaryButton { get; set; } = new();

    /// <summary>Kleine Links unter den Buttons (LinkedIn, E-Mail, ...).</summary>
    public List<LinkItem> SocialLinks { get; set; } = new();

    [StringLength(ContentLimits.Url, ErrorMessage = ContentLimits.TooLongMessage)]
    public string PortraitImage { get; set; } = string.Empty;

    [StringLength(ContentLimits.AltText, ErrorMessage = ContentLimits.TooLongMessage)]
    public string PortraitAlt { get; set; } = string.Empty;

    /// <summary>
    /// Prueft die Titelzeilen einzeln: [StringLength] laesst sich nicht direkt
    /// auf eine List&lt;string&gt; anwenden, deshalb hier von Hand.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        for (var i = 0; i < TitleLines.Count; i++)
        {
            if (TitleLines[i]?.Length > ContentLimits.Heading)
            {
                yield return new ValidationResult(
                    $"Titelzeile {i + 1} darf höchstens {ContentLimits.Heading} Zeichen lang sein.",
                    [nameof(TitleLines)]);
            }
        }
    }
}
