using System;
using System.Collections.Generic;
using System.Linq;
using valleyfold.Fold;

namespace valleyfold.Folding;

public class Valleyfold : IFold
{
    public List<Id> Vertices = new();

    public void Apply(Frame frame)
    {
        var startVertex = frame.Vertices[Vertices[0]];
        var endVertex = frame.Vertices[Vertices[1]];

        var edge = new Edge
        {
            Assignment = Assignment.V,
            Vertices = Vertices.ToArray(),
            FoldAngle = 0,
            Length = (endVertex.Coord - startVertex.Coord).Length()
        };

        var facesToSplit = frame
            .Faces
            .Where(f => f.Vertices.Contains(Vertices[0]) || f.Vertices.Contains(Vertices[1]))
            .ToList();

        foreach (var face in facesToSplit)
        {
            var (faceL, faceR) = SplitFace(face, frame);

            frame.SplitFace(face, faceL, faceR);
        }

        frame.AddEdge(edge);
    }

    private (Face faceL, Face faceR) SplitFace(Face face, Frame frame)
    {
        var endVertexIndex = Array.IndexOf(face.Vertices, Vertices[1]);
        var otherIndex = endVertexIndex;
        var leftVertices = new List<Id> { Vertices[1] };
        while (frame.Vertices[otherIndex] != frame.Vertices[Vertices[0]])
        {
            otherIndex = (otherIndex + 1) % face.Vertices.Length;

            leftVertices.Add(otherIndex);
        }

        var rightVertices = new List<Id> { Vertices[0] };
        while (frame.Vertices[otherIndex] != frame.Vertices[Vertices[1]])
        {
            otherIndex = (otherIndex + 1) % face.Vertices.Length;

            rightVertices.Add(otherIndex);
        }

        return (new Face { Vertices = leftVertices.ToArray() }, new Face { Vertices = rightVertices.ToArray() });
    }
}