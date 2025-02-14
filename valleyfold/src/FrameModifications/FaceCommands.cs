using System.Collections.Generic;
using valleyfold.Fold;

namespace valleyfold.FrameModifications;

public static class FaceCommands
{
    public static (Face faceL, Face faceR) SplitFace(Frame frame, Face face, Id start, Id end)
    {
        var endVertexIndex = face.Vertices.IndexOf(end);
        var otherIndex = endVertexIndex;
        var leftVertices = new List<Id> { end };
        while (frame.Vertices[face.Vertices[otherIndex]] != frame.Vertices[start])
        {
            otherIndex = (otherIndex + 1) % face.Vertices.Count;

            leftVertices.Add(face.Vertices[otherIndex]);
        }

        var rightVertices = new List<Id> { start };
        while (frame.Vertices[face.Vertices[otherIndex]] != frame.Vertices[end])
        {
            otherIndex = (otherIndex + 1) % face.Vertices.Count;

            rightVertices.Add(face.Vertices[otherIndex]);
        }

        var faceL = new Face { Vertices = leftVertices };
        var faceR = new Face { Vertices = rightVertices };
        
        frame.SplitFace(face, faceL, faceR);

        return (faceL, faceR);
    }
}