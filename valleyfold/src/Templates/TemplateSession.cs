using System.Collections.Generic;
using valleyfold.Templates.Events;
using valleyfold.Utils;

namespace valleyfold.Templates;

/// <summary>
/// Tracks progress through a template's ordered <see cref="BoatTemplate.FoldStep"/>
/// list. The session is the source of truth for "which step is next" so the
/// ghost renderer and click handler stay stateless w.r.t. progress.
///
/// Pure C# — no Godot, no Statics — so the state machine is unit-testable
/// in isolation. Applying a step (mutating <c>Frame</c>, starting the
/// animator) is the caller's job; this object only advances the cursor and
/// fires <see cref="TemplateCompletedEvent"/> exactly once when the last
/// step has been advanced past.
/// </summary>
public class TemplateSession
{
    private readonly IReadOnlyList<BoatTemplate.FoldStep> _steps;
    private int _index;
    private bool _completionEmitted;

    public string TemplateId { get; }
    public string PaperId { get; }

    public TemplateSession(string templateId, string paperId, IReadOnlyList<BoatTemplate.FoldStep> steps)
    {
        TemplateId = templateId;
        PaperId = paperId;
        _steps = steps;
        _index = 0;
    }

    public bool IsComplete => _index >= _steps.Count;

    /// <summary>Next step to apply, or <c>default</c> if the session is complete.</summary>
    public BoatTemplate.FoldStep CurrentStep
        => IsComplete ? default : _steps[_index];

    /// <summary>
    /// Move past the current step. On the transition that crosses the last
    /// step, emits <see cref="TemplateCompletedEvent"/>. Subsequent calls
    /// are no-ops — the event never re-fires for the same session.
    /// </summary>
    public void Advance()
    {
        if (IsComplete) return;
        _index++;
        if (IsComplete && !_completionEmitted)
        {
            _completionEmitted = true;
            EventBus.Emit(new TemplateCompletedEvent
            {
                TemplateId = TemplateId,
                PaperId = PaperId,
            });
        }
    }
}
