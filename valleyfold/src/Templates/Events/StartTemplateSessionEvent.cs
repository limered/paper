namespace valleyfold.Templates.Events;

/// <summary>
/// UI request to begin a guided template session. The controller decides
/// which template — for M0 there's only the boat.
/// </summary>
public class StartTemplateSessionEvent
{
    public string TemplateId { get; set; } = string.Empty;
    public string PaperId { get; set; } = string.Empty;
}
