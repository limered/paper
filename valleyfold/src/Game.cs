using Godot;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.TwoDeeModels;
using valleyfold.Ui;
using valleyfold.Ui.Events;
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
        Statics.Frame3d.ImportFromFrame(Statics.Frame);

        EventBus.Register<FoldModeChange>(OnFoldModeChange);
        EventBus.Register<AnimationModeChange>(OnAnimationModeChanged);
        
        EventBus.Register<ResetPaperEvent>(_ => ResetPaper());
    }

    private void ResetPaper()
    {
        Statics.Frame = new Frame();
        Statics.Frame.InitializePaper();
        Statics.Frame3d.ImportFromFrame(Statics.Frame);
        
        Statics.ChangeMemory.Clear();
        
        EventBus.Emit(new PaperFoldedEvent());
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