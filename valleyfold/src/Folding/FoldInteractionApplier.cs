using System.Collections.Generic;
using Godot;
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

        var crossings = new List<(Vector2 point, Edge edge)>();
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
            crossings.Add((crossing.Value, edge));
        }
        
        // collect faces to add edges to
        // sort then to reuse vertices
        // add edges to 2d faces
        
        // apply vertex interaction
        // save all data into change record
        
        // -> renderer render change record
    }
}