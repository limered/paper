using Godot;
using valleyfold.Fold;

namespace valleyfold.Folding;

public class VertexAction
{
    private readonly Vector2 _endPoint;
    private readonly Id _selectedVertexId;

    public VertexAction(Id selectedVertexId, Vector2 endPoint)
    {
        _selectedVertexId = selectedVertexId;
        _endPoint = endPoint;
    }

    public void Apply(Frame frame)
    {
        
    }
}