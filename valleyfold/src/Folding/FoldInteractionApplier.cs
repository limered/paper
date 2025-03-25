using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.Render.ThreeDee.Events;
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
        
        var lineA2d = lineA.Vector2XZ();
        var lineB2d = lineB.Vector2XZ();

        var addedVertices = new List<Id>();
        for (var e = 0; e < frame.Edges.Count; e++)
        {
            var edge = frame.Edges[e];
            var edgeStart = frame3d.Vertices[edge.Vertices[0]].Coord;
            var edgeEnd = frame3d.Vertices[edge.Vertices[1]].Coord;
            
            var crossing = FoldMath.LineSegmentCrossing(
                lineA2d, 
                lineB2d, 
                edgeStart.Vector2XZ(), 
                edgeEnd.Vector2XZ());
            if (!crossing.HasValue) continue;
            
            // calculate t value for point on edge
            var pointDirection = crossing.Value.Vector3XZ() - edgeStart;
            var lineDirection = edgeEnd - edgeStart;
            var t = pointDirection.Dot(lineDirection) / lineDirection.LengthSquared();
            
            // add new vertices to edge
            var pointOn2dEdge = frame.Vertices[edge.Vertices[0]].Coord.Lerp(frame.Vertices[edge.Vertices[1]].Coord, t);
            var addedVertexId = EdgeCommands.AddVertexToEdge(frame, new Vertex { Coord = pointOn2dEdge }, edge);
            addedVertices.Add(addedVertexId);
        }
        
        // add new edges between added vertices
        for (var v = 0; v < addedVertices.Count; v++)
        {
            var vertexId = addedVertices[v];
            for (var cv = 0; cv < addedVertices.Count; cv++)
            {
                if(v == cv) continue;
                var otherVertexId = addedVertices[cv];
                var faces = frame.Faces.Where(f => f.Vertices.Contains(vertexId) && f.Vertices.Contains(otherVertexId));
                if (!faces.Any()) continue;
                if (frame.Edges.Any(e => e.Vertices.Contains(vertexId) && e.Vertices.Contains(otherVertexId))) continue; // already existing edge
                
                // ToDo: use different assignments depending on face up direction
                new VertexToVertexFold(vertexId, otherVertexId, Assignment.V).Apply(frame);
            }
        }

        var changeRecord = new ChangeRecord
        {
            PickedByVertex = true,
            PickedVertex = startVertex,
            FoldLineA = lineA,
            FoldLineB = lineB,
            AddedVertices = addedVertices,
        };
        Statics.ChangeMemory.AddChange(changeRecord);

        EventBus.Emit(new PaperFoldedEvent());
    }
}
