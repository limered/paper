using System;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Render.Edges;
using valleyfold.Render.Faces;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.Render.Vertices;
using valleyfold.ThreeDeeModels;
using valleyfold.Utils;

namespace valleyfold.Render.ThreeDee;

public partial class PaperRenderer : Node3D
{
    [Export] public EdgeRendering EdgeRendering;
    [Export] public FaceRendering FaceRendering;
    [Export] public VertexRendering VertexRendering;

    public override void _Ready()
    {
        EventBus.Register<PaperFoldedEvent>(OnPaperFolded);
    }

    private void OnPaperFolded(PaperFoldedEvent _)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        frame3d.ImportFromFrame(frame);

        var changes = Statics.ChangeMemory.Changes;
        foreach (var change in changes)
        {
            if(change.Unfolded || change.ChangeType != ChangeType.ValleyFold) continue;
            
            var pickedVertex = frame3d.Vertices[change.PickedVertex].Coord;
            for (var v = 0; v < frame3d.Vertices.Count; v++)
            {
                var vertex = frame3d.Vertices[v].Coord;
                var start = change.FoldLineA;
                var end = change.FoldLineB;

                if (FoldMath.AreOnSameSideOfLine(
                        start.Vector2XZ(), 
                        end.Vector2XZ(), 
                        vertex.Vector2XZ(), 
                        pickedVertex.Vector2XZ()))
                {
                    frame3d.Vertices[v] = new Vertex3D { Coord = ReflectedAroundLine(start, end, vertex) };
                }
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;

        if (frame3d.Vertices is not null && 
            frame3d.Vertices.Any() && 
            frame3d.Vertices.Count == frame.Vertices.Count)
        {
            var vertexPoints = frame3d.Vertices.Select(v => v.Coord).ToList();
            FaceRendering.RenderFaces(frame.Faces, vertexPoints);
            EdgeRendering.Render(frame.Edges, vertexPoints);
            VertexRendering.Render(frame3d.Vertices);
        }
    }

    private Vector3 ReflectedAroundLine(Vector3 foldLineStart, Vector3 foldLineEnd, Vector3 vertex)
    {
        var foldAxis = (foldLineEnd - foldLineStart).Normalized();
        const double foldAngle = Math.PI;
        var vertexPositionRelativeToEdge = vertex - foldLineStart;
        var vertexPositionRelativeToEdgeRotated = vertexPositionRelativeToEdge
            .Rotated(foldAxis, (float)foldAngle);
        return foldLineStart + vertexPositionRelativeToEdgeRotated;
    }
}