using valleyfold.Templates.Events;
using valleyfold.Utils;

namespace valleyfold.Ui;

/// <summary>
/// Pure C# selection model for the Desk drawer's template + paper choices.
/// Keeps UI state testable without touching the Godot scene tree.
/// </summary>
public class DeskSelection
{
    public const string BoatTemplateId = "boat";
    public const string DefaultPaperId = "default";
    public const string LockedPaperId = "locked";

    public string SelectedTemplateId { get; private set; } = string.Empty;
    public string SelectedPaperId { get; private set; } = string.Empty;

    public bool CanBegin =>
        SelectedTemplateId == BoatTemplateId &&
        SelectedPaperId == DefaultPaperId;

    public bool IsPaperSelectable(string paperId) => paperId == DefaultPaperId;

    public void SelectTemplate(string templateId)
    {
        SelectedTemplateId = templateId;
    }

    public void SelectPaper(string paperId)
    {
        if (IsPaperSelectable(paperId)) SelectedPaperId = paperId;
    }

    public void Begin()
    {
        if (!CanBegin) return;

        EventBus.Emit(new StartTemplateSessionEvent
        {
            TemplateId = SelectedTemplateId,
            PaperId = SelectedPaperId,
        });
    }
}
