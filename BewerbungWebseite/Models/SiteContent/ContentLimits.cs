namespace BewerbungsSeite.Models.SiteContent;

/// <summary>
/// Einzige Quelle der Wahrheit fuer die Laenge der CMS-Textfelder. Jede Zahl
/// steht hier genau einmal und wird von drei Stellen verwendet:
///   - den [StringLength]-Attributen auf den SiteContent-Models (harte
///     Grenze, die ein manipulierter Request nicht umgehen kann),
///   - den data-max-Attributen, die EditMarkup im Admin-Editor rendert,
///   - dem Live-Zaehler in wwwroot/js/page-editor.js, der data-max ausliest.
/// Ändert sich eine Grenze, reicht die Aenderung an genau dieser Zahl.
///
/// Die Werte sind nicht pro einzelnem Feld gewaehlt, sondern pro Feld-Typ:
/// viele Felder teilen denselben Zweck (z. B. alle "kicker"-Felder) und
/// damit denselben verfuegbaren Platz im Layout. Herleitung siehe die
/// Analyse-Tabelle in der Aufgabenbeschreibung; kurz zusammengefasst:
///   - Kicker/Titel/Buttons/Icons sitzen einzeilig neben anderen Elementen
///     -> enge, harte Grenzen.
///   - Fliesstexte (Intro, Beschreibungen) haben viel Breite und brechen
///     dank CSS sauber um -> grosszuegiger Deckel, kein hartes Sperren beim
///     Tippen im Editor.
/// </summary>
public static class ContentLimits
{
    /// <summary>Kicker-Label ueber einem Section-Titel (alle 8 Sections).</summary>
    public const int Kicker = 40;

    /// <summary>Grosser Section-/Hero-Titel bzw. dessen hervorgehobener Teil.</summary>
    public const int Heading = 50;

    /// <summary>Titel einer Skill-/Ziel-Kachel.</summary>
    public const int TileTitle = 50;

    /// <summary>
    /// Text einer Skill-/Ziel-Kachel. Kacheln stehen im selben Flex-Row-Verbund
    /// wie ihre Nachbarn und werden dadurch alle gleich hoch -- deshalb der
    /// engste Deckel unter den Fliesstext-Feldern.
    /// </summary>
    public const int TileText = 180;

    /// <summary>Label einer Kurzfakten-Zeile (about.facts.label).</summary>
    public const int FactLabel = 30;

    /// <summary>Wert einer Kurzfakten-Zeile (about.facts.value).</summary>
    public const int FactValue = 80;

    /// <summary>CSS-Klasse eines Icons (kein Freitext).</summary>
    public const int IconClass = 60;

    /// <summary>Titel einer Projekt-/Erfahrungs-/Dokumentkarte.</summary>
    public const int CardTitle = 70;

    /// <summary>Kleiner Zwischentitel innerhalb einer Karte (z. B. "TECH:").</summary>
    public const int DetailHeading = 50;

    /// <summary>Kurze Meta-Zeile unter einem Kartentitel (Experience.meta).</summary>
    public const int Meta = 80;

    /// <summary>Beschriftung eines Buttons oder Links.</summary>
    public const int ButtonLabel = 40;

    /// <summary>
    /// Fliesstext in einer breiten Spalte (Hero-Intro, About-Text,
    /// Projekt-/Detail-/Erfahrungstext, Dokumentbeschreibung). Grosszuegiger
    /// Deckel als Rueckfallgrenze -- im Editor nur Warnfarbe, kein Sperren.
    /// </summary>
    public const int ParagraphText = 500;

    /// <summary>Zweizeilige Einleitung ueber dem Kontaktformular.</summary>
    public const int FormIntro = 200;

    /// <summary>Url, Datei- oder Bildpfad. Kein Layoutrisiko, nur Schutz vor Missbrauch.</summary>
    public const int Url = 300;

    /// <summary>Alt-Text eines Bildes. Unsichtbar, nur Hygiene-Deckel.</summary>
    public const int AltText = 200;

    /// <summary>
    /// Deutsche Fehlermeldung fuer alle [StringLength]-Attribute unten (der
    /// ASP.NET-Standardtext ist Englisch). {1} fuellt ASP.NET Core automatisch
    /// mit der maximalen Laenge des jeweiligen Attributs.
    /// </summary>
    public const string TooLongMessage = "Text ist zu lang (maximal {1} Zeichen erlaubt).";
}
