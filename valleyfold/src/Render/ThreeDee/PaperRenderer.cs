using System;
using System.Linq;
using Godot;
using valleyfold.Render.Edges;
using valleyfold.Render.Faces;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.Render.Vertices;
using valleyfold.ThreeDeeModels;
using valleyfold.Utils;

namespace valleyfold.Render.ThreeDee;

public partial class PaperRenderer : Node3D
{
    [Export] public FaceRendering FaceRendering;
    [Export] public EdgeRendering EdgeRendering;
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
            for (var v = 0; v < frame3d.Vertices.Count; v++)
            {
                var vertex = frame3d.Vertices[v].Coord;
                var start = frame3d.Vertices[change.FoldLine.start].Coord;
                var end = frame3d.Vertices[change.FoldLine.end].Coord;
                
                // TODO: use vertex id if can be used, else interpolate edge segment
                if (start.DirectionTo(vertex).Dot(change.StartPoint) < 0) continue;
                frame3d.Vertices[v] = new Vertex3D { Coord = ReflectedAroundLine(start, end, vertex) };
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        frame3d.ImportFromFrame(frame);
        
        if (frame3d.Vertices is not null && frame3d.Vertices.Any())
        {
            var vertices = frame3d.Vertices.Select(v => v.Coord).ToList();
            // FaceRendering.RenderFaces(Statics.Frame.Faces, vertices);
            // EdgeRendering.Render(Statics.Frame.Edges, vertices);
            VertexRendering.Render(frame3d.Vertices);
        }
        
        // fold vertices on edges
        // var reflectedAroundFaces = new HashSet<Face>();
        // var facesToReflectAround = new Queue<Face>();
        // facesToReflectAround.Enqueue(frame.Faces[0]);
        // while(facesToReflectAround.Any())
        // {
        //     var initialFace = facesToReflectAround.Dequeue();
        //     var faceEdges = initialFace.Edges();
        //     for (var e = 0; e < faceEdges.Length; e++)
        //     {
        //         var edge = faceEdges[e];
        //         if (edge.Assignment == Assignment.B) continue;
        //         if (edge.Assignment == Assignment.V)
        //         {
        //             // fold every connected vertex around the edge
        //             var v1 = _vertices[edge.Vertices[0].Value];
        //             var v2 = _vertices[edge.Vertices[1].Value];
        //             var foldAngle = edge.FoldAngle;
        //             if (foldAngle == 0) continue;
        //             
        //             // navigate connected faces and collect vertices to rotate
        //             var connectedFace = edge.Faces().First(f => f != initialFace);
        //             if (reflectedAroundFaces.Contains(connectedFace)) continue;
        //             facesToReflectAround.Enqueue(connectedFace);
        //             
        //             var foldAxis = (v2 - v1).Normalized();
        //             var foldAngleInRad = Math.PI * foldAngle;
        //             for(var v = 0; v < connectedFace.Vertices.Count; v++)
        //             {
        //                 var vertex = connectedFace.Vertices[v];
        //                 var vertexPosition = _vertices[vertex.Value];
        //                 var vertexPositionRelativeToEdge = vertexPosition - v1;
        //                 var vertexPositionRelativeToEdgeRotated = vertexPositionRelativeToEdge
        //                     .Rotated(foldAxis, (float)foldAngleInRad);
        //                 _vertices[vertex.Value] = v1 + vertexPositionRelativeToEdgeRotated;
        //             }
        //         }
        //     }
        //     reflectedAroundFaces.Add(initialFace);
        // }
    }

    private Vector3 ReflectedAroundLine(Vector3 foldLineStart, Vector3 foldLineEnd, Vector3 vertex)
    {
        var foldAxis = (foldLineEnd - foldLineStart).Normalized();
        const double foldAngle = Math.PI * 0.9f;
        var vertexPositionRelativeToEdge = vertex - foldLineStart;
        var vertexPositionRelativeToEdgeRotated = vertexPositionRelativeToEdge
            .Rotated(foldAxis, (float)foldAngle);
        return foldLineStart + vertexPositionRelativeToEdgeRotated;
    }
}