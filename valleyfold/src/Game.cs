using Godot;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.Templates;
using valleyfold.TwoDeeModels;
using valleyfold.Ui;
using valleyfold.Ui.Events;
using valleyfold.Utils;

namespace valleyfold;

public partial class Game : Node
{
    public override void _Ready()
    {
        Statics.Frame = new Frame();
        Statics.Frame.InitializePaper();
        Statics.Frame3d.ImportFromFrame(Statics.Frame);

        EventBus.Register<ResetPaperEvent>(_ => ResetPaper());

        // F10 debug auto-play harness for the boat fold sequence (issue 04).
        AddChild(new BoatAutoPlay());

        // F9 ghost-click flow for the boat template (issue 05). Reuses the
        // existing paper Area3D so we don't need to edit the scene.
        var controller = new TemplateSessionController
        {
            MouseCollisionArea = GetNode<Area3D>("Interaction/Area3D"),
        };
        AddChild(controller);
    }

    private void ResetPaper()
    {
        Statics.Frame = new Frame();
        Statics.Frame.InitializePaper();
        Statics.Frame3d.ImportFromFrame(Statics.Frame);

        Statics.ChangeMemory.Clear();
        Statics.TemplateSession = null;

        EventBus.Emit(new PaperFoldedEvent());
    }
}
