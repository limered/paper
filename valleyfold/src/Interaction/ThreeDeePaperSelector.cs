using Godot;
using valleyfold.Interaction.SelectionStates;
using valleyfold.Render.ThreeDee;
using valleyfold.TwoDeeModels;
using valleyfold.Ui;
using valleyfold.Utils;

namespace valleyfold.Interaction;

public partial class ThreeDeePaperSelector : Node3D
{
    private ISelectionState _currentSelectionState;
    private readonly SelectionContext _selectionContext = new();
    
    [Export] public Area3D MouseCollisionArea;
    [Export] public Node3D MouseMarker;
    [Export] public float PickingThreshold = 0.25f;
    [Export] public float SnapThreshold = 0.1f;

    public override void _Ready()
    {
        _selectionContext.Parent = this;
        _selectionContext.MousePosition = new MousePosition();
        _selectionContext.MousePosition.Init(MouseCollisionArea);
        _currentSelectionState = new StartPointSelectionState();
        
        EventBus.Register<FoldModeChange>(OnFoldModeChange);
    }
    
    private void OnFoldModeChange(FoldModeChange msg)
    {
        _selectionContext.FoldMode = msg.NextFoldMode;
        _currentSelectionState = msg.NextFoldMode switch
        {
            Assignment.F => new EdgeSelectionState(),
            Assignment.R => new RefoldSelectionState(),
            _ => new StartPointSelectionState(),
        };

        Statics.Frame?.UnmarkEdges();
        Statics.Frame?.UnmarkVertices();
    }

    public override void _Input(InputEvent @event)
    {
        // While a guided template session owns input, freeform picking is
        // suppressed (issue 05).
        if (Statics.TemplateSession != null) return;
        _currentSelectionState = _currentSelectionState
            .OnInput(_selectionContext, @event);
    }

    public override void _Process(double delta)
    {
        if (Statics.TemplateSession != null) return;
        if (Statics.Frame == null) return;

        // Force a Frame3D sync before reading it from the selection state.
        // Otherwise, on the tick after a fold completes, EdgeCommands.AddVertexToEdge
        // has grown Frame.Vertices in _Input but Frame3D.Vertices is still
        // one rebuild behind. Without this call, ordering depends on node
        // _Process order (see refactor-frame3d-sync/01).
        PaperRenderer.RebuildFrame3D();

        _currentSelectionState = _currentSelectionState
            .OnProcess(_selectionContext);

        MouseMarker.GlobalPosition = _selectionContext.TargetPosition;
    }
}