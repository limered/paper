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
    /// Per-layer Z-nudge for coplanar disambiguation. Single source of truth
    /// for face / edge / vertex renderers so the whole frame moves together.
    /// Per ADR-0002 §"Consequences": small enough to be invisible, large enough
    /// to win against float-precision z-fighting between stacked elements.
    /// </summary>
    public const float LayerEpsilon = 1e-4f;

    /// <summary>
    /// Max layer over all faces incident to <paramref name="edge"/>. Edges on
    /// the crease between two layers (top and bottom face of a fold) snap to
    /// the higher layer so they render visually attached to the topmost face.
    /// O(faces); cache externally if it becomes a hot path.
    /// </summary>
    public int LayerOfEdge(Edge edge)
    {
        var max = 0;
        var faces = Faces;
        for (var i = 0; i < faces.Count; i++)
        {
            var f = faces[i];
            if (f.Vertices.Contains(edge.Vertices[0]) && f.Vertices.Contains(edge.Vertices[1]))
            {
                var l = LayerOf(f);
                if (l > max) max = l;
            }
        }
        return max;
    }

    /// <summary>
    /// Max layer over all faces containing <paramref name="vertexId"/>. Shared
    /// crease vertices snap to the highest incident face so the vertex marker
    /// renders attached to the topmost layer.
    /// </summary>
    public int LayerOfVertex(Id vertexId)
    {
        var max = 0;
        var faces = Faces;
        for (var i = 0; i < faces.Count; i++)
        {
            var f = faces[i];
            if (f.Vertices.Contains(vertexId))
            {
                var l = LayerOf(f);
                if (l > max) max = l;
            }
        }
        return max;
    }

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
