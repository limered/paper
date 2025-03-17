using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.Folding;

public class EdgeAction
{
    private readonly Vector2 _endPoint;
    private readonly Edge _selectedEdge;
    private readonly Vector2 _startPointOnEdge;

    public EdgeAction(Vector2 startPointOnEdge, Edge selectedEdge, Vector2 endPoint)
    {
        _startPointOnEdge = startPointOnEdge;
        _selectedEdge = selectedEdge;
        _endPoint = endPoint;
    }
}