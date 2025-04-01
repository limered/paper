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
    private PickingMode _lastPickingMode = PickingMode.StartPoint;
    private Vector3 _mouseWorldPosition;
    private PickingMode _pickingMode = PickingMode.StartPoint;
    [Export] public Area3D MouseCollisionArea;
    [Export] public Node3D MouseMarker;

    private Id _pickedVertex;
    private PreviewLine _previewLine;

    private List<Id> _hoveredEdgeIds;

    public override void _Ready()
    {
        MouseCollisionArea.InputEvent += MouseCollisionAreaOnInputEvent;
        MouseCollisionArea.MouseExited += ChangeToButtonsPicking;
        MouseCollisionArea.MouseEntered += ChangeToLastPaperPicking;
        
        EventBus.Register<FoldModeChange>(OnFoldModeChange);
    }

    private void OnFoldModeChange(FoldModeChange msg)
    {
        if (msg.NextFoldMode == Assignment.F)
        {
            _pickingMode = PickingMode.EdgeSelect;
            _lastPickingMode = PickingMode.EdgeSelect;
        }
        else
        {
            _pickingMode = PickingMode.StartPoint;
            _lastPickingMode = PickingMode.StartPoint;
        }
        
        Statics.Frame?.UnmarkEdges();
        Statics.Frame?.UnmarkVertices();
    }

    private void ChangeToLastPaperPicking()
    {
        _pickingMode = _lastPickingMode;
    }

    private void ChangeToButtonsPicking()
    {
        _lastPickingMode = _pickingMode;
        _pickingMode = PickingMode.Buttons;
    }

    private void MouseCollisionAreaOnInputEvent(
        Node camera,
        InputEvent @event,
        Vector3 eventPosition,
        Vector3 normal,
        long shapeIdx)
    {
        _mouseWorldPosition = eventPosition;
        _mouseWorldPosition.Y = 0;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return;

        switch (_pickingMode)
        {
            case PickingMode.Buttons:
                return;
            case PickingMode.StartPoint when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                StartFoldInteraction();
                _pickingMode = PickingMode.EndPoint;
                return;
            case PickingMode.EndPoint when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                ConfirmFoldInteraction();
                _pickingMode = PickingMode.StartPoint;
                break;
            case PickingMode.EndPoint when mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed:
                ResetFoldInteraction();
                _pickingMode = PickingMode.StartPoint;
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
        FoldInteractionApplier.ApplyVertexValleyFold(_pickedVertex, _mouseWorldPosition);
        _previewLine?.ChangeVisibility(false);
        _previewLine?.Draw();
    }

    private void StartFoldInteraction()
    {
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

        switch (_pickingMode)
        {
            case PickingMode.Buttons:
                return;
            case PickingMode.StartPoint:
                _pickedVertex = MarkVertexInFrame(frame3d, _mouseWorldPosition);
                frame3d.ImportMetadataFromFrame(frame);
                break;
            case PickingMode.EndPoint:
                UpdateFoldPreview(frame3d, _pickedVertex, _mouseWorldPosition, _previewLine);
                break;
            case PickingMode.EdgeSelect:
                HoverEdgeToUnfold();
                break;
        }

        MouseMarker.GlobalPosition = _mouseWorldPosition;
    }
    
    private void HoverEdgeToUnfold()
    {
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        
        frame.UnmarkEdges();
        
        var nearestEdges = EdgeQueries.NearestEdgesToPoint3d(frame3d, _mouseWorldPosition);
        if (!nearestEdges.Any()) return;
        var (_, edge) = EdgeQueries.NearestPointOnEdgeToPoint(frame3d, _mouseWorldPosition, nearestEdges);
        if (!edge.IsUnfoldable() || !IsLastFolded(edge)) return;
        
        var change = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if(change is null) return;
        
        for (var i = 0; i < change.AddedEdges.Count; i++)
        {
            var addedEdgeId = change.AddedEdges[i];
            frame.Edges[addedEdgeId].IsSelected = true;
        }
        _hoveredEdgeIds = change.AddedEdges;
    }

    private static bool IsLastFolded(Edge edge)
    {
        var lastUnfolded = Statics.ChangeMemory.LastStillFoldedChange();
        return lastUnfolded?.AddedEdges.Contains(edge.Id) ?? false;
    }

    private static Id MarkVertexInFrame(Frame3D frame3d, Vector3 mouseWorldPosition)
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

        Statics.Frame.Vertices[closestId].IsSelected = true;
        return closestId;
    }

    private static void UpdateFoldPreview(
        Frame3D frame3d,
        Id pickedVertex,
        Vector3 mouseWorldPosition,
        PreviewLine previewLine)
    {
        var centerPoint = frame3d.Vertices[pickedVertex].Coord.Lerp(mouseWorldPosition, 0.5f);
        var direction = (mouseWorldPosition - frame3d.Vertices[pickedVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);
        previewLine.UpdatePosition(centerPoint, perpendicular);

        previewLine.Draw();
    }
}