using Godot;
using valleyfold.Render.ThreeDee;

namespace valleyfold.Templates;

/// <summary>
/// Debug-only auto-play harness for issue 04. Press <c>F10</c> in the
/// editor: applies <see cref="BoatTemplate.Steps"/> one at a time,
/// waiting for the in-flight fold animation to finish between steps.
/// No UI, no ghost rendering, no click input — pure replay.
///
/// Temporary scaffolding. The user-facing flow comes with issue 05
/// (ghost-crease click) and the data-driven template format (post-MVP).
/// </summary>
public partial class BoatAutoPlay : Node
{
    private const Key TriggerKey = Key.F10;
    private int _nextStep = -1; // -1 = idle

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_nextStep != -1) return;
        if (@event is not InputEventKey k || !k.Pressed || k.Echo) return;
        if (k.Keycode != TriggerKey) return;
        if (!BoatTemplate.IsValidStartingFrame(Statics.Frame))
        {
            GD.PrintErr("[BoatAutoPlay] frame is not the unfolded square; reset before pressing F10.");
            return;
        }
        _nextStep = 0;
    }

    public override void _Process(double delta)
    {
        if (_nextStep < 0) return;
        if (_nextStep >= BoatTemplate.Steps.Length)
        {
            _nextStep = -1;
            return;
        }

        // Force a frame3d sync so the fold step reads up-to-date 3D coords
        // for the previous fold (see ThreeDeePaperSelector for the same
        // node-order race).
        PaperRenderer.RebuildFrame3D();

        if (Statics.FoldAnimator.IsAnimating) return;

        var step = BoatTemplate.Steps[_nextStep];
        if (!BoatTemplate.ApplyStep(step))
        {
            GD.PrintErr($"[BoatAutoPlay] step {_nextStep} failed to resolve; aborting.");
            _nextStep = -1;
            return;
        }
        _nextStep++;
    }
}
