namespace valleyfold.Models;

public class ModelHalfEdge
{
    public ModelVertex Vertex;
    public ModelHalfEdge Pair;
    public ModelHalfEdge Prev;
    public ModelHalfEdge Next;
    public bool IsCurve;
    public ModelEdge Edge;
    public ModelFace Face;
}