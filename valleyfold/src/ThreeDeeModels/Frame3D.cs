using System.Collections.Generic;
using valleyfold.TwoDeeModels;

namespace valleyfold.ThreeDeeModels;

public class Frame3D
{
    public List<Vertex3D> Vertices { get; } = new();
    public List<Face> Faces => Statics.Frame.Faces;
    public List<Edge> Edges => Statics.Frame.Edges;
    public List<Vertex> Vertices2D => Statics.Frame.Vertices;

    /// <summary>
    /// Per-face stacking order. Higher = higher in the stack.
    /// Rebuilt fresh on every <see cref="ResetLayers"/> (called once per
    /// <c>PaperRenderer.RebuildFrame3D</c>), then bumped per replayed fold
    /// per the rule in ADR-0002 §"Layer-update rule on fold". Keyed by
    /// <see cref="Face.Id"/> so face references can come and go (splits)
    /// without invalidating in-flight lookups within a single replay pass.
    /// </summary>
    private readonly Dictionary<Id, int> _layers = new();

    public int LayerOf(Face face) => _layers.TryGetValue(face.Id, out var l) ? l : 0;

    /// <summary>
    /// Clears the layer map and seeds every face in <paramref name="frame"/>
    /// at layer 0. Called once at the start of every <c>RebuildFrame3D</c>.
    /// </summary>
    public void ResetLayers(Frame frame)
    {
        _layers.Clear();
        for (var i = 0; i < frame.Faces.Count; i++)
            _layers[frame.Faces[i].Id] = 0;
    }

    /// <summary>
    /// Adds <paramref name="bump"/> to the layer of every face in
    /// <paramref name="moved"/>. Preserves relative ordering within the set.
    /// </summary>
    public void BumpLayers(IEnumerable<Face> moved, int bump)
    {
        foreach (var face in moved)
            _layers[face.Id] = LayerOf(face) + bump;
    }

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
