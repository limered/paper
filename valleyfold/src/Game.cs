using Godot;
using valleyfold.Fold;
using valleyfold.Ui;
using valleyfold.Utils;

namespace valleyfold;

public partial class Game : Node
{
    private bool _isAnimating;
    private Assignment _foldMode;

    public override void _Ready()
    {
        Statics.Game = this;
        Statics.Frame = new Frame();
        Statics.Frame.InitializePaper();

        EventBus.Register<FoldModeChange>(OnFoldModeChange);
        EventBus.Register<AnimationModeChange>(OnAnimationModeChanged);
    }

    private void OnAnimationModeChanged(AnimationModeChange _)
    {
        _isAnimating = !_isAnimating;
    }

    private void OnFoldModeChange(FoldModeChange msg)
    {
        if (_isAnimating) return;
        _foldMode = msg.NextFoldMode;
    }
}