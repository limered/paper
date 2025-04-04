using System.Collections.Generic;
using valleyfold.TwoDeeModels;

namespace valleyfold.ThreeDeeModels;

public class Frame3D
{
    public List<Vertex3D> Vertices { get; } = new();
    public List<Face> Faces => Statics.Frame.Faces;
    public List<Edge> Edges => Statics.Frame.Edges;
    public List<Vertex> Vertices2D => Statics.Frame.Vertices;

    public void ImportFromFrame(Frame frame)
    {
        Vertices.Clear();
        foreach (var vertex in frame.Vertices) Vertices.Add(vertex.To3D());
    }

    public void ImportMetadataFromFrame(Frame frame)
    {
        for (var i = 0; i < frame.Vertices.Count; i++)
            Vertices[i].IsSelected = frame.Vertices[i].IsSelected;
    }

    public void UnmarkVertices()
    {
        foreach (var vertex in Vertices)
            vertex.IsSelected = false;
    }
}