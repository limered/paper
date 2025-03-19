using System.Collections.Generic;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.ThreeDeeModels;

public class Frame3D
{
    public List<Vertex3D> Vertices { get; } = new();

    public void ImportFromFrame(Frame frame)
    {
        Vertices.Clear();
        foreach (var vertex in frame.Vertices)
            Vertices.Add(new Vertex3D
            {
                Coord = vertex.Coord.Vector3XZ(),
                IsSelected = vertex.IsSelected
            });
    }

    public void UnmarkVertices()
    {
        foreach (var vertex in Vertices)
            vertex.IsSelected = false;
    }
}