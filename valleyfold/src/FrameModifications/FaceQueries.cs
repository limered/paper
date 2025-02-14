using System.Collections.Generic;
using System.Linq;
using valleyfold.Fold;

namespace valleyfold.FrameModifications;

public static class FaceQueries
{
    public static Face[] FacesAdjacentToEdge(Frame frame, Edge edge)
    {
        return frame.Faces
            .Where(face => face.Vertices.Contains(edge.Vertices[0]) && face.Vertices.Contains(edge.Vertices[1]))
            .ToArray();
    }

    public static Face FacesContainingVertexIds(Frame frame, List<Id> vertexIds)
    {
        return frame.Faces
            .FirstOrDefault(face => vertexIds.All(face.Vertices.Contains));
    }
}