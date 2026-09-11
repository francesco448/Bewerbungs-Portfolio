namespace BewerbungsSeite.Models.SiteContent;

/// <summary>
/// Modell eines Section-Partials: der Inhalt des Bereichs und die Angabe,
/// ob die Seite im Bearbeitungsmodus gerendert wird. Dasselbe Partial wird
/// so von der oeffentlichen Seite und von der Admin-Seite benutzt.
/// </summary>
public class SectionView<T>(T section, bool editMode)
{
    public T Section { get; } = section;
    public bool EditMode { get; } = editMode;
}

public static class SectionView
{
    public static SectionView<T> For<T>(T section, bool editMode) => new(section, editMode);
}

/// <summary>
/// Ein Textstueck, das im Edit-Modus in ein bearbeitbares Element eingepackt wird
/// und im normalen Modus als reiner Text erscheint. So bleibt das oeffentliche
/// Markup unveraendert, auch wenn der Text mit anderen Elementen eine Zeile teilt.
/// </summary>
public class EditableText(string text, string field, bool editMode, int? max = null)
{
    public string Text { get; } = text;
    public string Field { get; } = field;
    public bool EditMode { get; } = editMode;
    public int? Max { get; } = max;
}
