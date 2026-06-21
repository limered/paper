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
///
/// Direction is controlled by passing explicit start/end progress to
/// <see cref="Start(ChangeRecord, float, float, System.Action, float)"/>:
/// folds and refolds animate 0 → 1, unfolds animate 1 → 0.
/// </summary>
public class FoldAnimator
{
    public const float DefaultDurationSeconds = 0.6f;

    private ChangeRecord _change;
    private float _elapsed;
    private float _duration;
    private float _startProgress;
    private float _endProgress = 1f;
    private Action _onComplete;

    public bool IsAnimating => _change != null;

    public ChangeRecord InFlightChange => _change;

    /// <summary>
    /// Eased progress between the configured start and end progress,
    /// interpolated by an ease-out curve. <c>0</c> when not animating.
    /// </summary>
    public float EasedProgress
    {
        get
        {
            if (_change == null) return 0f;
            var t = Math.Min(1f, _elapsed / _duration);
            var eased = EaseOut(t);
            return _startProgress + (_endProgress - _startProgress) * eased;
        }
    }

    public void Start(ChangeRecord change, float durationSeconds = DefaultDurationSeconds)
        => Start(change, 0f, 1f, null, durationSeconds);

    public void Start(ChangeRecord change, float startProgress, float endProgress,
                      Action onComplete, float durationSeconds = DefaultDurationSeconds)
    {
        _change = change;
        _elapsed = 0f;
        _duration = durationSeconds > 0f ? durationSeconds : DefaultDurationSeconds;
        _startProgress = startProgress;
        _endProgress = endProgress;
        _onComplete = onComplete;
    }

    public void Tick(double delta)
    {
        if (_change == null) return;

        _elapsed += (float)delta;
        if (_elapsed < _duration) return;

        // Completion: clear in-flight slot *before* invoking callback / emitting
        // so any listener that re-queries the animator sees the cleared state.
        var callback = _onComplete;
        _change = null;
        _elapsed = 0f;
        _duration = 0f;
        _startProgress = 0f;
        _endProgress = 1f;
        _onComplete = null;
        callback?.Invoke();
        EventBus.Emit(new PaperFoldedEvent());
    }

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}
