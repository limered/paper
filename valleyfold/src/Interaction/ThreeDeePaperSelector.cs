using Godot;
using valleyfold.Interaction.SelectionStates;
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
        _currentSelectionState =
            msg.NextFoldMode == Assignment.F 
            ? new EdgeSelectionState() 
            : new StartPointSelectionState();

        Statics.Frame?.UnmarkEdges();
        Statics.Frame?.UnmarkVertices();
    }

    public override void _Input(InputEvent @event)
    {
        _currentSelectionState = _currentSelectionState
            .OnInput(_selectionContext, @event);
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        
        _currentSelectionState = _currentSelectionState
            .OnProcess(_selectionContext);

        MouseMarker.GlobalPosition = _selectionContext.MousePosition.Current;
    }
}