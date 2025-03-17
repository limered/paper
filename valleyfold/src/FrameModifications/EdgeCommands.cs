using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.FrameModifications;

public static class EdgeCommands
{
    public static Id AddVertexToEdge(Frame frame, Vertex newVertex, Edge edge)
    {
        var id = frame.Vertices.Count;
        frame.Vertices.Add(newVertex);

        var edgeAdjacentFaces = FaceQueries.FacesAdjacentToEdge(frame, edge);
        foreach (var edgeAdjacentFace in edgeAdjacentFaces)
        {
            var startVertexId = edgeAdjacentFace.Vertices.IndexOf(edge.Vertices[0]);
            var endVertexId = edgeAdjacentFace.Vertices.IndexOf(edge.Vertices[1]);
            // case for edge between start- and endpoint
            if ((endVertexId == 0 && startVertexId == edgeAdjacentFace.Vertices.Count - 1) ||
                (startVertexId == 0 && endVertexId == edgeAdjacentFace.Vertices.Count - 1))
                edgeAdjacentFace.Vertices.Add(id);
            else
                edgeAdjacentFace.Vertices
                    .Insert(startVertexId < endVertexId ? endVertexId : startVertexId, id);
        }

        var secondEdge = new Edge
        {
            Vertices = new Id[] { id, edge.Vertices[1] },
            Assignment = edge.Assignment,
            FoldAngle = edge.FoldAngle
        };
        edge.Vertices[1] = id;
        frame.AddEdge(secondEdge);

        return id;
    }
}