using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.Utils;

namespace valleyfold.TwoDeeModels;

public class Vertex
{
    public Vector2 Coord;
    public Id Id;
    public bool IsSelected;

    public Vertex3D To3D()
    {
        return new Vertex3D
        {
            Coord = Coord.Vector3XZ(),
            IsSelected = IsSelected
        };
    }
}