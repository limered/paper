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

    private List<Id> GenerateCrossVertices(Frame frame, List<Edge> crossedEdges)
    {
        var crossedEdgesWithVerticesIds = new List<(Edge, Id)>();
        for (var i = 0; i < crossedEdges.Count; i++)
        {
            var crossedEdge = crossedEdges[i];
            var intersectionPoint = EdgeQueries.EdgeToEdgeIntersectionPoint(
                Vertices[0], 
                Vertices[1], 
                frame.Vertices[crossedEdge.Vertices[0]], 
                frame.Vertices[crossedEdge.Vertices[1]]);
            
            var intersectionVertex = new Vertex
                { Coord = new Vector3(intersectionPoint.X, 0, intersectionPoint.Y) };
            var intersectionVertexId = EdgeCommands.AddVertexToEdge(frame, intersectionVertex, crossedEdge);
            crossedEdgesWithVerticesIds.Add((crossedEdge, intersectionVertexId));
        }

        return crossedEdgesWithVerticesIds.Select(tuple => tuple.Item2).OrderBy(v =>
                frame.Vertices[v].Coord.DistanceSquaredTo(Vertices[0].Coord))
            .Prepend(VertexIds[0])
            .Append(VertexIds[1])
            .ToList();
    }
    
    public void Apply(Frame frame)
    {
        if (FoldAlreadyExists(frame)) return;

        AddNewVerticesIfNeeded(frame);

        List<Id> sortedVertexIds;
        
        var crossedEdges = EdgeQueries.EdgesCrossingEdge(frame, VertexIds[0], VertexIds[1]);
        if (crossedEdges.Any())
        {
            sortedVertexIds = GenerateCrossVertices(frame, crossedEdges);
        }
        else
        {
            sortedVertexIds = VertexIds.ToList();
        }

        var last = sortedVertexIds[0];
        for (var i = 1; i < sortedVertexIds.Count; i++)
        {
            var current = sortedVertexIds[i];

            var face = frame
                .Faces
                .First(f => f.Vertices.Contains(last) && f.Vertices.Contains(current));
            
            var (faceL, faceR) = SplitFace(face, frame, last, current);
            frame.SplitFace(face, faceL, faceR);
            
            frame.AddEdge(new Edge
            {
                Assignment = Assignment.U,
                FoldAngle = 0,
                Vertices = new []{last, current}
            });
            
            last = current;
        }
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

    private static (Face faceL, Face faceR) SplitFace(Face face, Frame frame, Id start, Id end)
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

        return (
            new Face { Vertices = leftVertices },
            new Face { Vertices = rightVertices });
    }
}