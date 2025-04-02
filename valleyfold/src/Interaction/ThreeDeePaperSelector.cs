using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Folding;
using valleyfold.FrameModifications;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;
using valleyfold.Ui;
using valleyfold.Utils;

namespace valleyfold.Interaction;

public partial class ThreeDeePaperSelector : Node3D
{
    private readonly InteractionState _interactionState = new();
    private readonly MousePosition _mousePosition = new();
    [Export] public Area3D MouseCollisionArea;
    [Export] public Node3D MouseMarker;
    [Export] public float PickingThreshold = 0.25f;

    private Id _pickedVertex;
    private PreviewLine _previewLine;

    private List<Id> _hoveredEdgeIds;

    public override void _Ready()
    {
        _mousePosition.Init(MouseCollisionArea);
        
        EventBus.Register<FoldModeChange>(OnFoldModeChange);
    }
    
    private void OnFoldModeChange(FoldModeChange msg)
    {
        _interactionState.GoToState(
            msg.NextFoldMode == Assignment.F 
            ? PickingMode.EdgeSelect 
            : PickingMode.StartPoint);

        Statics.Frame?.UnmarkEdges();
        Statics.Frame?.UnmarkVertices();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return;

        switch (_interactionState.Current)
        {
            case PickingMode.Buttons:
                return;
            case PickingMode.StartPoint when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                StartFoldInteraction();
                _interactionState.GoToState(PickingMode.EndPoint);
                return;
            case PickingMode.EndPoint when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                ConfirmFoldInteraction();
                _interactionState.GoToState(PickingMode.StartPoint);
                break;
            case PickingMode.EndPoint when mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed:
                ResetFoldInteraction();
                _interactionState.GoToState(PickingMode.StartPoint);
                break;
            case PickingMode.EdgeSelect when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                ConfirmUnfoldInteraction();
                break;
            default:
                return;
        }
    }

    private void ConfirmUnfoldInteraction()
    {
        if(_hoveredEdgeIds == null || !_hoveredEdgeIds.Any()) return;
        var edge = Statics.Frame.Edges[_hoveredEdgeIds.First()];
        var changeContainingEdge = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if (changeContainingEdge is null) return;
        
        changeContainingEdge.Unfolded = true;
        for (var i = 0; i < changeContainingEdge.AddedEdges.Count; i++)
        {
            var edgeToChange = Statics.Frame.Edges[changeContainingEdge.AddedEdges[i]];
            edgeToChange.Assignment = Assignment.F;
            edgeToChange.FoldAngle = 0f;
        }

        var newChange = new ChangeRecord
        {
            ChangeType = ChangeType.Unfold,
            Unfolded = true,
        };
        Statics.ChangeMemory.AddChange(newChange);
        EventBus.Emit(new PaperFoldedEvent());
        _hoveredEdgeIds = null;
    }

    private void ResetFoldInteraction()
    {
        _previewLine?.ChangeVisibility(false);
        _previewLine?.Draw();
    }

    private void ConfirmFoldInteraction()
    {
        if (_pickedVertex == -1) return;
        FoldInteractionApplier.ApplyVertexValleyFold(_pickedVertex, _mousePosition.Current);
        _previewLine?.ChangeVisibility(false);
        _previewLine?.Draw();
    }

    private void StartFoldInteraction()
    {
        if (_pickedVertex == -1) return;
        if (_previewLine is null)
        {
            _previewLine = new PreviewLine();
            AddChild(_previewLine);
        }

        _previewLine.ChangeVisibility(true);
        _previewLine.LineColor(Colors.Aqua);
        _previewLine.Draw();
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;

        frame.UnmarkVertices();

        switch (_interactionState.Current)
        {
            case PickingMode.Buttons:
                return;
            case PickingMode.StartPoint:
                _pickedVertex = MarkVertexInFrame(frame3d, _mousePosition.Current);
                frame3d.ImportMetadataFromFrame(frame);
                break;
            case PickingMode.EndPoint:
                UpdateFoldPreview(frame3d, _pickedVertex, _mousePosition.Current, _previewLine);
                break;
            case PickingMode.EdgeSelect:
                ShowLastFoldedEdges();
                HoverEdgeToUnfold();
                break;
        }

        MouseMarker.GlobalPosition = _mousePosition.Current;
    }

    private void ShowLastFoldedEdges()
    {
        var frame = Statics.Frame;
        frame.UnmarkEdges();
        
        var lastFolded = Statics.ChangeMemory.LastStillFoldedChange();
        if (lastFolded is null) return;
        for (var i = 0; i < lastFolded.AddedEdges.Count; i++)
        {
            var addedEdgeId = lastFolded.AddedEdges[i];
            frame.Edges[addedEdgeId].IsSelected = true;
        }
    }

    private void HoverEdgeToUnfold()
    {
        var frame3d = Statics.Frame3d;
        
        var nearestEdges = EdgeQueries.NearestEdgesToPoint3d(frame3d, _mousePosition.Current);
        if (!nearestEdges.Any()) return;
        var (_, edge) = EdgeQueries.NearestPointOnEdgeToPoint(frame3d, _mousePosition.Current, nearestEdges);
        if (!edge.IsUnfoldable() || !IsLastFolded(edge)) return;
        
        var change = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if(change is null) return;

        _hoveredEdgeIds = change.AddedEdges;
    }

    private static bool IsLastFolded(Edge edge)
    {
        var lastUnfolded = Statics.ChangeMemory.LastStillFoldedChange();
        return lastUnfolded?.AddedEdges.Contains(edge.Id) ?? false;
    }

    private Id MarkVertexInFrame(Frame3D frame3d, Vector3 mouseWorldPosition)
    {
        if (frame3d.Vertices is null || !frame3d.Vertices.Any()) return -1;
        var closestId = 0;
        var closestDistance = float.MaxValue;
        for (var i = 0; i < frame3d.Vertices.Count; i++)
        {
            var vertex = frame3d.Vertices[i].Coord;
            var distance = vertex.DistanceTo(mouseWorldPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestId = i;
            }
        }
        
        if(closestDistance > PickingThreshold) return -1;
        
        Statics.Frame.Vertices[closestId].IsSelected = true;
        return closestId;
    }

    private static void UpdateFoldPreview(
        Frame3D frame3d,
        Id pickedVertex,
        Vector3 mouseWorldPosition,
        PreviewLine previewLine)
    {
        if (pickedVertex == -1) return;
        var centerPoint = frame3d.Vertices[pickedVertex].Coord.Lerp(mouseWorldPosition, 0.5f);
        var direction = (mouseWorldPosition - frame3d.Vertices[pickedVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);
        previewLine.UpdatePosition(centerPoint, perpendicular);

        previewLine.Draw();
    }
}