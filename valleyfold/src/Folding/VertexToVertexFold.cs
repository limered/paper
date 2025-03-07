using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class VertexToVertexFold : IFold
{
    private readonly Id _vertexIdA;
    private readonly Id _vertexIdB;

    public VertexToVertexFold(Id vertexIdA, Id vertexIdB)
    {
        _vertexIdA = vertexIdA;
        _vertexIdB = vertexIdB;
    }


    public void Apply(Frame frame)
    {
        if (VerticesAreTheSame() || VerticesLieOnTheSameEdge(frame)) return;

        var crossedVertices = VertexQueries.VerticesCrossedByEdge(frame, _vertexIdA, _vertexIdB);
        var crossedEdges = EdgeQueries.EdgesCrossedByEdge(frame, _vertexIdA, _vertexIdB);
        if (crossedVertices.Any() && crossedEdges.Any())
            crossedEdges = FilterEdgesCrossedByStartOrEnd(crossedVertices, crossedEdges);

        var crossedEdgeVertexIds = crossedEdges.Any() ? 
                GenerateCrossVerticesOnEdges(frame, crossedEdges) : 
                new List<Id>();
        crossedEdgeVertexIds = OrderCrossedVerticesByDistanceToStart(
                frame, crossedEdgeVertexIds.Concat(crossedVertices));

        SplitFacesAndGenerateEdges(frame, crossedEdgeVertexIds);
    }

    private static void SplitFacesAndGenerateEdges(Frame frame, List<Id> crossedEdgeVertexIds)
    {
        var last = crossedEdgeVertexIds[0];
        for (var i = 1; i < crossedEdgeVertexIds.Count; i++)
        {
            var current = crossedEdgeVertexIds[i];
            var faceToSplit = FaceQueries.FacesContainingVertexIds(frame, new List<Id> { last, current });

            FaceCommands.SplitFace(frame, faceToSplit, last, current);

            frame.AddEdge(new Edge
            {
                Assignment = Assignment.U,
                FoldAngle = 0,
                Vertices = new[] { last, current }
            });

            last = current;
        }
    }

    private List<Id> OrderCrossedVerticesByDistanceToStart(Frame frame, IEnumerable<Id> crossedVertexIds)
    {
        return crossedVertexIds.OrderBy(v =>
                frame.Vertices[v].Coord.DistanceSquaredTo(frame.Vertices[_vertexIdA].Coord))
            .Prepend(_vertexIdA)
            .Append(_vertexIdB)
            .ToList();
    }

    private static List<Edge> FilterEdgesCrossedByStartOrEnd(List<Id> crossedVertices, List<Edge> crossedEdges)
    {
        return crossedVertices
            .SelectMany(_ => crossedEdges,
                (crossedVertex, crossedEdge) => new { crossedVertex, crossedEdge })
            .Where(t => !t.crossedEdge.Vertices.Contains(t.crossedVertex))
            .Select(t => t.crossedEdge)
            .ToList();
    }

    private List<Id> GenerateCrossVerticesOnEdges(Frame frame, List<Edge> crossedEdges)
    {
        var crossedEdgesWithVerticesIds = new List<(Edge, Id)>();
        for (var i = 0; i < crossedEdges.Count; i++)
        {
            var crossedEdge = crossedEdges[i];
            var intersectionPoint = EdgeQueries.EdgeToEdgeIntersectionPoint(
                frame.Vertices[_vertexIdA],
                frame.Vertices[_vertexIdB],
                frame.Vertices[crossedEdge.Vertices[0]],
                frame.Vertices[crossedEdge.Vertices[1]]);

            var intersectionVertex = new Vertex
                { Coord = new Vector2(intersectionPoint.X, intersectionPoint.Y) };
            var intersectionVertexId = EdgeCommands.AddVertexToEdge(frame, intersectionVertex, crossedEdge);
            crossedEdgesWithVerticesIds.Add((crossedEdge, intersectionVertexId));
        }

        return crossedEdgesWithVerticesIds.Select(tuple => tuple.Item2).ToList();
    }

    private bool VerticesLieOnTheSameEdge(Frame frame)
    {
        return EdgeQueries.EdgeContainingVertices(frame, _vertexIdA, _vertexIdB) != null;
    }

    private bool VerticesAreTheSame()
    {
        return _vertexIdA == _vertexIdB;
    }
}