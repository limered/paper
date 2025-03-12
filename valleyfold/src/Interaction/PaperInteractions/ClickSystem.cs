using System.Linq;
using Godot;
using valleyfold.Fold;
using valleyfold.Folding;
using valleyfold.FrameModifications;
using valleyfold.Render.Edges;
using valleyfold.Utils;

namespace valleyfold.Interaction.PaperInteractions;

public class WorldNewPointData
{
    public Vector2 Coord;
    public Edge Edge;
    public Id ExistingVertex;
    public bool IsOnEdge;

    public WorldNewPointData Copy()
    {
        return new WorldNewPointData
        {
            Coord = Coord,
            ExistingVertex = ExistingVertex,
            IsOnEdge = IsOnEdge,
            Edge = Edge
        };
    }
}

public enum PickingMode
{
    Buttons,
    StartPoint,
    EndPoint
}

public partial class ClickSystem : Node3D
{
    private Vector3 _mouseWorldPosition;
    private PickingMode _pickingMode = PickingMode.StartPoint;
    private PickingMode _lastPickingMode = PickingMode.StartPoint;
    private WorldNewPointData _startPoint;
    private WorldNewPointData _tempPointData = new();
    private Vector2 _newEdgeStart;
    private Vector2 _newEdgeEnd;
    private VertexAction _currentAction;
    [Export] public Node3D FoldStartPoint;
    [Export] public Node3D GhostClickPosition;
    [Export] public EdgeLine GhostEdgeLine;
    [Export] public Area3D MouseCollision;
    [Export] public float VertexPickThreshold = 0.1f;

    public override void _Ready()
    {
        MouseCollision.InputEvent += MouseCollisionOnInputEvent;
        MouseCollision.MouseExited += () =>
        {
            _lastPickingMode = _pickingMode; _pickingMode = PickingMode.Buttons;  
        };
        MouseCollision.MouseEntered += () =>
        {
            _pickingMode = _lastPickingMode;
        };
    }

    private void MouseCollisionOnInputEvent(
        Node camera,
        InputEvent @event,
        Vector3 eventPosition,
        Vector3 normal,
        long shapeIdx)
    {
        _mouseWorldPosition = eventPosition;
    }

    public override void _Input(InputEvent @event)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        
        if (@event is not InputEventMouseButton mouseEvent) return;
        if (mouseEvent.ButtonIndex != MouseButton.Left || !mouseEvent.Pressed) return;
        if (_pickingMode == PickingMode.Buttons) return;
        if (_pickingMode == PickingMode.StartPoint)
        {
            _startPoint = _tempPointData.Copy();
            _pickingMode = PickingMode.EndPoint;
            return;
        }

        if (_pickingMode == PickingMode.EndPoint && GhostEdgeLine != null)
        {
            _currentAction?.ApplyFoldedEdges(frame);
            
            // var edgeStart = NearestPointToPoint(frame, _newEdgeStart);
            // var edgeEnd = NearestPointToPoint(frame, _newEdgeEnd);
            //
            // if (edgeStart.IsOnEdge && edgeEnd.IsOnEdge)
            //     new EdgeToEdgeFold(
            //             edgeStart.Edge,
            //             edgeEnd.Edge,
            //             edgeStart.Coord,
            //             edgeEnd.Coord)
            //         .Apply(Statics.Frame);
            // else if (edgeStart.IsOnEdge)
            //     new VertexToEdgeFold(
            //             edgeStart.Edge,
            //             edgeStart.Coord,
            //             edgeEnd.ExistingVertex)
            //         .Apply(Statics.Frame);
            // else if (edgeEnd.IsOnEdge)
            //     new VertexToEdgeFold(
            //             edgeEnd.Edge,
            //             edgeEnd.Coord,
            //             edgeStart.ExistingVertex)
            //         .Apply(Statics.Frame);
            // else
            //     new VertexToVertexFold(
            //             edgeStart.ExistingVertex,
            //             edgeEnd.ExistingVertex)
            //         .Apply(Statics.Frame);
            
            
            _newEdgeStart = new Vector2(100, 100);
            _newEdgeEnd = new Vector2(100, 100);
            _startPoint = null;
            _pickingMode = PickingMode.StartPoint;
            RemoveChild(GhostEdgeLine);
            GhostEdgeLine = null;
        }
    }

    private WorldNewPointData NearestPointToPoint(Frame frame, Vector2 point)
    {
        var result = new WorldNewPointData();
        var nearestVertexId = VertexQueries.NearestVertexIdTo(frame, point, VertexPickThreshold);
        if (nearestVertexId > -1)
        {
            result.ExistingVertex = nearestVertexId;
            result.Coord = frame.Vertices[nearestVertexId].Coord;
            return result;
        }

        var nearestEdges = EdgeQueries.NearestEdgesToPoint(frame, point);
        if (nearestEdges.Any())
        {
            var pointAndEdge = EdgeQueries.NearestPointOnEdgeToPoint(frame, point, nearestEdges);
            result.Coord = pointAndEdge.Item1;
            result.Edge = pointAndEdge.Item2;
            result.Edge.IsSelected = true;
            result.IsOnEdge = true;
            return result;
        }

        result.ExistingVertex = VertexQueries.NearestVertexIdTo(frame, point);
        result.Coord = frame.Vertices[result.ExistingVertex].Coord;
        return result;
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        frame.UnmarkEdges();
        frame.UnmarkVertices();

        if (_pickingMode == PickingMode.Buttons) return;
        if (_pickingMode == PickingMode.StartPoint)
        {
            _tempPointData = NearestPointToPoint(frame, _mouseWorldPosition.Vector2XZ());
        }
        else
        {
            _tempPointData.Coord = _mouseWorldPosition.Vector2XZ();

            if (GhostEdgeLine == null)
            {
                GhostEdgeLine = new EdgeLine();
                AddChild(GhostEdgeLine);
            }

            if (_startPoint.IsOnEdge)
            {
                
            }
            else
            {
                _currentAction = new VertexAction(
                        _startPoint.ExistingVertex, 
                        _tempPointData.Coord);
                
                var strip = _currentAction.Draft(frame);
                GhostEdgeLine.ClearPositions();
                for (var i = 0; i < strip.Points.Count - 1; i++)
                {
                    var start = strip.Points[i];
                    var end = strip.Points[i+1];
                    GhostEdgeLine.AddPositions(start.Vector3XZ(), end.Vector3XZ());
                    GhostEdgeLine.Draw();
                }
            }
        }

        GhostClickPosition.Position = _tempPointData.Coord.Vector3XZ();

        FoldStartPoint.Position = _startPoint?.Coord.Vector3XZ() ?? new Vector3(100, 100, 100);
    }
}