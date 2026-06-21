namespace valleyfold.TwoDeeModels;

public class Edge
{
    /**
     * Vertices[0] is first
     */
    public Id[] Vertices = new Id[2];
    public Id Id { get; init; }
    
    public Assignment Assignment;
    public float FoldAngle;
    public bool IsSelected;
    
    public override string ToString()
    {
        return $"[Edge: ({Vertices[0].Value},{Vertices[1].Value}) , ass: {Assignment} , angle: {FoldAngle}]";
    }

    public bool IsUnfoldable()
    {
        return Assignment is Assignment.V or Assignment.M;
    }

    public bool IsRefoldable()
    {
        return Assignment is Assignment.F;
    }
}