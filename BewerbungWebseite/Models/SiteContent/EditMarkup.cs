using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace BewerbungsSeite.Models.SiteContent;

/// <summary>
/// Erzeugt die data-Attribute, an denen der Editor die bearbeitbaren Stellen
/// erkennt. Ausserhalb des Edit-Modus kommt nichts zurueck, damit das Markup
/// der oeffentlichen Seite unveraendert bleibt.
/// (Razor laesst Attribute mit null-Wert nicht weg, deshalb dieser Weg.)
/// </summary>
public static class EditMarkup
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Default;

    private static IHtmlContent Attributes(bool editMode, string markup) =>
        editMode ? new HtmlString(markup) : HtmlString.Empty;

    /// <summary>Kennzeichnet einen Bereich.</summary>
    public static IHtmlContent Section(bool editMode, string name) =>
        Attributes(editMode, $"data-section=\"{Encoder.Encode(name)}\"");

    /// <summary>
    /// Kennzeichnet ein bearbeitbares Textfeld. <paramref name="max"/> kommt aus
    /// <see cref="ContentLimits"/> und landet als data-max im Markup, wo es der
    /// Editor (page-editor.js) fuer den Live-Zaehler und die Eingabegrenze liest.
    /// </summary>
    public static IHtmlContent Field(bool editMode, string name, bool multiline = false, int? max = null) =>
        Attributes(editMode,
            $"data-field=\"{Encoder.Encode(name)}\"" +
            (multiline ? " data-multiline=\"true\"" : string.Empty) +
            (max.HasValue ? $" data-max=\"{max}\"" : string.Empty) +
            " contenteditable=\"true\"");

    /// <summary>Macht ein Element bearbeitbar, ohne ihm ein eigenes Feld zuzuordnen.</summary>
    public static IHtmlContent Editable(bool editMode, int? max = null) =>
        Attributes(editMode,
            "contenteditable=\"true\"" + (max.HasValue ? $" data-max=\"{max}\"" : string.Empty));

    /// <summary>Kennzeichnet einen Listencontainer.</summary>
    public static IHtmlContent List(bool editMode, string name) =>
        Attributes(editMode, $"data-list=\"{Encoder.Encode(name)}\"");

    /// <summary>Kennzeichnet eine Liste aus reinen Texten.</summary>
    public static IHtmlContent StringList(bool editMode, string name) =>
        Attributes(editMode, $"data-list=\"{Encoder.Encode(name)}\" data-string-list=\"true\"");

    /// <summary>Kennzeichnet einen Listeneintrag.</summary>
    public static IHtmlContent Item(bool editMode) =>
        Attributes(editMode, "data-item=\"true\"");

    /// <summary>Kennzeichnet ein verschachteltes Objekt.</summary>
    public static IHtmlContent Object(bool editMode, string name) =>
        Attributes(editMode, $"data-object=\"{Encoder.Encode(name)}\"");

    /// <summary>Kennzeichnet ein austauschbares Bild und haelt dessen gespeicherten Pfad fest.</summary>
    public static IHtmlContent Image(bool editMode, string name, string path) =>
        Attributes(editMode,
            $"data-image=\"{Encoder.Encode(name)}\" data-value=\"{Encoder.Encode(path ?? string.Empty)}\"");
}
