using System;
using valleyfold.ChangeTracking;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.Utils;

namespace valleyfold.Folding;

/// <summary>
/// Owns the in-flight fold animation. While an animation is active the
/// renderer reads <see cref="EasedProgress"/> and applies the fold at
/// that fraction of its <see cref="ChangeRecord.TargetAngle"/>.
///
/// Only one fold animates at a time. New folds must be blocked while
/// <see cref="IsAnimating"/> is true.
/// </summary>
public class FoldAnimator
{
    public const float DefaultDurationSeconds = 0.4f;

    private ChangeRecord _change;
    private float _elapsed;
    private float _duration;

    public bool IsAnimating => _change != null;

    public ChangeRecord InFlightChange => _change;

    /// <summary>
    /// Linear progress from 0 to 1, eased out so the paper decelerates
    /// into its final position. <c>0</c> when not animating.
    /// </summary>
    public float EasedProgress
    {
        get
        {
            if (_change == null) return 0f;
            var t = Math.Min(1f, _elapsed / _duration);
            return EaseOut(t);
        }
    }

    public void Start(ChangeRecord change, float durationSeconds = DefaultDurationSeconds)
    {
        _change = change;
        _elapsed = 0f;
        _duration = durationSeconds > 0f ? durationSeconds : DefaultDurationSeconds;
    }

    public void Tick(double delta)
    {
        if (_change == null) return;

        _elapsed += (float)delta;
        if (_elapsed < _duration) return;

        // Completion: clear in-flight slot *before* emitting so any listener
        // that re-queries the animator sees the cleared state.
        _change = null;
        _elapsed = 0f;
        _duration = 0f;
        EventBus.Emit(new PaperFoldedEvent());
    }

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}
