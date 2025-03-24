using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Folding;

public class FoldInteractionApplier
{
    public static void ApplyVertexValleyFold(Id startVertex, Vector3 endPoint)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        
        var centerPoint = frame3d.Vertices[startVertex].Coord.Lerp(endPoint, 0.5f);
        var direction = (endPoint - frame3d.Vertices[startVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);

        var lineA = centerPoint + perpendicular * 100;
        var lineB = centerPoint - perpendicular * 100;

        var crossings = new List<(float t, Edge edge)>();
        for (var e = 0; e < frame.Edges.Count; e++)
        {
            var edge = frame.Edges[e];
            var edgeStart = frame3d.Vertices[edge.Vertices[0]].Coord;
            var edgeEnd = frame3d.Vertices[edge.Vertices[1]].Coord;
            // Maybe use 3d line segment crossing (or projection)
            var crossing = FoldMath.LineSegmentCrossing(
                lineA.Vector2XZ(), 
                lineB.Vector2XZ(), 
                edgeStart.Vector2XZ(), 
                edgeEnd.Vector2XZ());
            if (!crossing.HasValue) continue;
            var startToPoint = crossing.Value.Vector3XZ() - frame3d.Vertices[edge.Vertices[0]].Coord;
            var edgeVector = edgeEnd - edgeStart;
            var t = startToPoint.Dot(edgeVector);
            crossings.Add((t, edge));
        }

        var addedVertices = new List<Id>();
        foreach (var (t, edge) in crossings)
        {
            var pointOn2dEdge = frame.Vertices[edge.Vertices[0]].Coord.Lerp(frame.Vertices[edge.Vertices[1]].Coord, t);
            var addedVertexId = EdgeCommands.AddVertexToEdge(frame, new Vertex { Coord = pointOn2dEdge }, edge);
            addedVertices.Add(addedVertexId);
        }

        for (var v = 0; v < addedVertices.Count; v++)
        {
            var vertexId = addedVertices[v];
            for (var cv = 0; cv < addedVertices.Count; cv++)
            {
                if(v == cv) continue;
                var otherVertexId = addedVertices[cv];
                var faces = frame.Faces.Where(f => f.Vertices.Contains(vertexId) && f.Vertices.Contains(otherVertexId));
                if (!faces.Any()) continue;
                if(frame.Edges.Any(e => e.Vertices.Contains(vertexId) && e.Vertices.Contains(otherVertexId))) continue; // already existing edge
                
                // add new edges between vertices
                new VertexToVertexFold(vertexId, otherVertexId, Assignment.V).Apply(frame);
            }
        }

        var changeRecord = new ChangeRecord()
        {
            PickedByVertex = true,
            PickedVertex = startVertex,
            FoldLineA = lineA,
            FoldLineB = lineB,
            AddedVertices = addedVertices,
        };
        Statics.ChangeMemory.AddChange(changeRecord);

        // -> renderer render change record
    }
}
