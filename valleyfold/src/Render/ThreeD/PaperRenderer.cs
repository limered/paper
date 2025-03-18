using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Render.Edges;
using valleyfold.Render.Faces;
using valleyfold.Render.ThreeD.Events;
using valleyfold.Render.Vertices;
using valleyfold.Utils;

namespace valleyfold.Render.ThreeD;

public partial class PaperRenderer : Node3D
{
    [Export] public FaceRendering FaceRendering;
    [Export] public EdgeRendering EdgeRendering;
    [Export] public VertexRendering VertexRendering;
    
    private Vector3[] _vertices;

    public override void _Ready()
    {
        EventBus.Register<PaperFoldedEvent>(OnPaperFolded);
    }

    private void OnPaperFolded(PaperFoldedEvent _)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        _vertices = new Vector3[frame.Vertices.Count];
        
        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            _vertices[i] = frame.Vertices[i].Coord.Vector3XZ();
        }

        var changes = Statics.ChangeMemory.Changes;
        foreach (var change in changes)
        {
            for (var v = 0; v < _vertices.Length; v++)
            {
                var vertex = _vertices[v];
                var start = _vertices[change.FoldLine.start];
                var end = _vertices[change.FoldLine.end];
                
                // TODO: use vertex id if can be used, else interpolate edge segment
                if (start.DirectionTo(vertex).Dot(change.StartPoint) < 0) continue;
                _vertices[v] = ReflectedAroundLine(start, end, vertex);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        if (_vertices is not null && _vertices.Any())
        {
            FaceRendering.RenderFaces(Statics.Frame.Faces, _vertices.ToList());
            EdgeRendering.Render(Statics.Frame.Edges, _vertices.ToList());
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