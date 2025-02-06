using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class UnspecifiedFold : IFold
{
    public readonly Edge[] VertexEdges = new Edge[2];
    public readonly bool[] VertexExists = new bool[2];
    public readonly Id[] VertexIds = { -1, -1 };
    public readonly Vertex[] Vertices = new Vertex[2];

    public void Apply(Frame frame)
    {
        if (FoldAlreadyExists(frame)) return;

        AddNewVerticesIfNeeded(frame);

        var edge = new Edge
        {
            Assignment = Assignment.U,
            Vertices = VertexIds.ToArray(),
            FoldAngle = 0,
            Length = (Vertices[1].Coord - Vertices[0].Coord).Length()
        };

        var crossedEdges = EdgeQueries.EdgesCrossingEdge(frame, edge);
        if (crossedEdges.Any())
        {
            var crossedEdgesWithVerticesIds = new List<(Edge, Id)>();
            for (var i = 0; i < crossedEdges.Count; i++)
            {
                var crossedEdge = crossedEdges[i];
                var intersectionPoint = EdgeQueries.EdgeToEdgeIntersectionPoint(frame, edge, crossedEdge);
                var intersectionVertex = new Vertex
                    { Coord = new Vector3(intersectionPoint.X, 0, intersectionPoint.Y) };
                var intersectionVertexId = EdgeCommands.AddVertexToEdge(frame, intersectionVertex, crossedEdge);
                crossedEdgesWithVerticesIds.Add((crossedEdge, intersectionVertexId));
            }
            // ToDo sort and iterate through edges to split polys 
            
        }
        var facesToSplit = frame
            .Faces
            .Where(f => f.Vertices.Contains(VertexIds[0]) && f.Vertices.Contains(VertexIds[1]))
            .ToList();

        foreach (var face in facesToSplit)
        {
            var (faceL, faceR) = SplitFace(face, frame);

            frame.SplitFace(face, faceL, faceR);
        }

        frame.AddEdge(edge);
    }


    private void AddNewVerticesIfNeeded(Frame frame)
    {
        if (!VertexExists[0]) VertexIds[0] = EdgeCommands.AddVertexToEdge(frame, Vertices[0], VertexEdges[0]);
        if (!VertexExists[1]) VertexIds[1] = EdgeCommands.AddVertexToEdge(frame, Vertices[1], VertexEdges[1]);
    }

    private bool FoldAlreadyExists(Frame frame)
    {
        if (VertexEdges[0] != null && VertexEdges[1] != null && VertexEdges[0] == VertexEdges[1]) return true;
        if (VertexEdges[0] != null && VertexEdges[1] == null)
        {
            var existingEdge = frame.Edges.Find(e => e.Vertices[0] == VertexIds[1] || e.Vertices[1] == VertexIds[1]);
            if (existingEdge != null && existingEdge == VertexEdges[0]) return true;
        }

        if (VertexEdges[1] != null && VertexEdges[0] == null)
        {
            var existingEdge = frame.Edges.Find(e => e.Vertices[0] == VertexIds[0] || e.Vertices[1] == VertexIds[0]);
            if (existingEdge != null && existingEdge == VertexEdges[1]) return true;
        }

        if (frame.Edges.Any(e => VertexIds.Contains(e.Vertices[0]) && VertexIds.Contains(e.Vertices[1]))) return true;

        return false;
    }

    private (Face faceL, Face faceR) SplitFace(Face face, Frame frame)
    {
        var endVertexIndex = face.Vertices.IndexOf(VertexIds[1]);
        var otherIndex = endVertexIndex;
        var leftVertices = new List<Id> { VertexIds[1] };
        while (frame.Vertices[face.Vertices[otherIndex]] != frame.Vertices[VertexIds[0]])
        {
            otherIndex = (otherIndex + 1) % face.Vertices.Count;

            leftVertices.Add(face.Vertices[otherIndex]);
        }

        var rightVertices = new List<Id> { VertexIds[0] };
        while (frame.Vertices[face.Vertices[otherIndex]] != frame.Vertices[VertexIds[1]])
        {
            otherIndex = (otherIndex + 1) % face.Vertices.Count;

            rightVertices.Add(face.Vertices[otherIndex]);
        }

        return (
            new Face { Vertices = leftVertices },
            new Face { Vertices = rightVertices });
    }
}