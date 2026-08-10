using Godot;

namespace valleyfold.Ui;

/// <summary>
/// Global scene-transition overlay. Registered as an autoload so any scene
/// can call <see cref="To"/> without owning a transition node.
/// </summary>
public partial class SceneTransition : CanvasLayer
{
    [Export] public Color WipeColor = new(0.14f, 0.13f, 0.12f, 1f);
    [Export] public float DurationSeconds = 0.35f;

    private Panel _panel;
    private Tween _tween;
    private bool _isTransitioning;

    public override void _Ready()
    {
        Layer = 100;

        _panel = new Panel();
        AddChild(_panel);
        _panel.MouseFilter = Control.MouseFilterEnum.Stop;
        _panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var style = new StyleBoxFlat
        {
            BgColor = WipeColor,
        };
        _panel.AddThemeStyleboxOverride("panel", style);

        HideInstantly();
    }

    public static void To(string scenePath, TransitionDirection direction)
    {
        var instance = Engine.GetSingleton("SceneTransition") as SceneTransition;
        instance?.TransitionTo(scenePath, direction);
    }

    private void TransitionTo(string scenePath, TransitionDirection direction)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        var viewportSize = GetViewport().GetVisibleRect().Size;
        var (enterFrom, exitTo) = PositionsFor(direction, viewportSize);

        _panel.Position = enterFrom;
        _panel.Visible = true;

        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(_panel, "position", Vector2.Zero, DurationSeconds);
        _tween.Finished += () =>
        {
            GetTree().ChangeSceneToFile(scenePath);
            CallDeferred(nameof(AnimateOut), (int)direction);
        };
    }

    private void AnimateOut(int directionValue)
    {
        var direction = (TransitionDirection)directionValue;
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var (_, exitTo) = PositionsFor(direction, viewportSize);

        _panel.Position = Vector2.Zero;

        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        _tween.TweenProperty(_panel, "position", exitTo, DurationSeconds);
        _tween.Finished += () =>
        {
            HideInstantly();
            _isTransitioning = false;
        };
    }

    private void HideInstantly()
    {
        _panel.Visible = false;
        _panel.Position = new Vector2(0, GetViewport().GetVisibleRect().Size.Y * 2f);
    }

    private static (Vector2 EnterFrom, Vector2 ExitTo) PositionsFor(TransitionDirection direction, Vector2 viewportSize)
    {
        return direction switch
        {
            TransitionDirection.Up => (new Vector2(0, viewportSize.Y), new Vector2(0, -viewportSize.Y)),
            TransitionDirection.Down => (new Vector2(0, -viewportSize.Y), new Vector2(0, viewportSize.Y)),
            TransitionDirection.Left => (new Vector2(viewportSize.X, 0), new Vector2(-viewportSize.X, 0)),
            TransitionDirection.Right => (new Vector2(-viewportSize.X, 0), new Vector2(viewportSize.X, 0)),
            _ => (new Vector2(0, viewportSize.Y), new Vector2(0, -viewportSize.Y)),
        };
    }

    public enum TransitionDirection
    {
        Up,
        Down,
        Left,
        Right,
    }
}
