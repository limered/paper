using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;

namespace valleyfold.Folding;

public class VertexToVertexFold : IFold
{
    private readonly Id _vertexIdA;
    private readonly Id _vertexIdB;
    private readonly Assignment _assignment;

    public VertexToVertexFold(Id vertexIdA, Id vertexIdB, Assignment assignment = Assignment.U)
    {
        _vertexIdA = vertexIdA;
        _vertexIdB = vertexIdB;
        _assignment = assignment;
    }


    public Id Apply(Frame frame)
    {
        if (VerticesAreTheSame() || VerticesLieOnTheSameEdge(frame)) return -1;
        
        return SplitFacesAndGenerateEdges(frame, new List<Id>{_vertexIdA, _vertexIdB});
    }

    private Id SplitFacesAndGenerateEdges(Frame frame, List<Id> crossedEdgeVertexIds)
    {
        var last = crossedEdgeVertexIds[0];
        var current = crossedEdgeVertexIds[1];
        var faceToSplit = FaceQueries.FacesContainingVertexIds(frame, new List<Id> { last, current });

        FaceCommands.SplitFace(frame, faceToSplit, last, current);

        var edge = new Edge
        {
            Assignment = _assignment,
            FoldAngle = 1f,
            Vertices = new[] { last, current },
            Id = frame.Edges.Count
        };
        
        frame.AddEdge(edge);
        
        return edge.Id;        
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
            
            var intersectionVertexId = EdgeCommands.AddVertexToEdge(frame, intersectionPoint, crossedEdge);
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