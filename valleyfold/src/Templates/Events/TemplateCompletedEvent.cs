namespace valleyfold.Templates.Events;

/// <summary>
/// Emitted by <see cref="TemplateSession"/> when the player has applied
/// the final fold step. Payload is intentionally just identifiers — the
/// celebration beat (camera, audio) and collection placement live
/// elsewhere (issue 07/08).
/// </summary>
public class TemplateCompletedEvent
{
    public string TemplateId { get; init; }
    public string PaperId { get; init; }
}
