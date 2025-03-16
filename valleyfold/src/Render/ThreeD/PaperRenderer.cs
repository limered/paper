using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;
using valleyfold.Render.Faces;
using valleyfold.Utils;

namespace valleyfold.Render.ThreeD;

public partial class PaperRenderer : Node3D
{
    [Export] public FaceRendering FaceRendering;
    
    private Vector3[] _vertices;

    public override void _Process(double delta)
    {
        // copy vertices from frame
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        _vertices = new Vector3[frame.Vertices.Count];
        
        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            _vertices[i] = frame.Vertices[i].Coord.Vector3XZ();
        }
        
        // fold vertices on edges
        var reflectedAroundFaces = new HashSet<Face>();
        var facesToReflectAround = new Queue<Face>();
        facesToReflectAround.Enqueue(frame.Faces[0]);
        while(facesToReflectAround.Any())
        {
            var initialFace = facesToReflectAround.Dequeue();
            var faceEdges = initialFace.Edges();
            for (var e = 0; e < faceEdges.Length; e++)
            {
                var edge = faceEdges[e];
                if (edge.Assignment == Assignment.B) continue;
                if (edge.Assignment == Assignment.V)
                {
                    // fold every connected vertex around the edge
                    var v1 = _vertices[edge.Vertices[0].Value];
                    var v2 = _vertices[edge.Vertices[1].Value];
                    var foldAngle = edge.FoldAngle;
                    if (foldAngle == 0) continue;
                    
                    // navigate connected faces and collect vertices to rotate
                    var connectedFace = edge.Faces().First(f => f != initialFace);
                    if (reflectedAroundFaces.Contains(connectedFace)) continue;
                    facesToReflectAround.Enqueue(connectedFace);
                    
                    var foldAxis = (v2 - v1).Normalized();
                    var foldAngleInRad = Math.PI * foldAngle;
                    for(var v = 0; v < connectedFace.Vertices.Count; v++)
                    {
                        var vertex = connectedFace.Vertices[v];
                        var vertexPosition = _vertices[vertex.Value];
                        var vertexPositionRelativeToEdge = vertexPosition - v1;
                        var vertexPositionRelativeToEdgeRotated = vertexPositionRelativeToEdge
                            .Rotated(foldAxis, (float)foldAngleInRad);
                        _vertices[vertex.Value] = v1 + vertexPositionRelativeToEdgeRotated;
                    }
                }
            }
            reflectedAroundFaces.Add(initialFace);
        }
        
        FaceRendering.RenderFaces(frame.Faces, _vertices.ToList());
    }
}