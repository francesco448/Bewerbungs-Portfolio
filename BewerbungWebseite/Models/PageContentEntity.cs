namespace BewerbungsSeite.Models;

/// <summary>
/// Speicherzeile fuer den gesamten Seiteninhalt. Es existiert genau ein
/// Datensatz mit <see cref="SingletonId"/>; der Inhalt liegt serialisiert
/// als UTF-8-JSON im Feld <see cref="Content"/>.
/// </summary>
public class PageContentEntity
{
    /// <summary>Feste Id des einzigen Datensatzes.</summary>
    public const int SingletonId = 1;

    public int Id { get; set; }

    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>Zeitpunkt der letzten Speicherung (UTC).</summary>
    public DateTime UpdatedAt { get; set; }
}
