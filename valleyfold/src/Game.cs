using System.Linq;
using Godot;
using valleyfold.Folding;
using valleyfold.TwoDeeModels;
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
        Statics.Frame3d.ImportFromFrame(Statics.Frame);
        // FoldFrame();

        EventBus.Register<FoldModeChange>(OnFoldModeChange);
        EventBus.Register<AnimationModeChange>(OnAnimationModeChanged);
    }

    private static void FoldFrame()
    {
        var vertexA = new Vertex { Coord = new Vector2(0, 0.2f) };
        var vertexB = new Vertex { Coord = new Vector2(1f, 0.5f) };
        var edgeA = Statics.Frame.Edges.ElementAt(3);
        var edgeB = Statics.Frame.Edges.ElementAt(1);

        new EdgeToEdgeFold(edgeA, edgeB, vertexA.Coord, vertexB.Coord, Assignment.V)
            .Apply(Statics.Frame);
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