using Godot;
// ReSharper disable InconsistentNaming

namespace valleyfold.Utils;

public static class VectorExtensions
{
    public static Vector2 Vector2XZ(this Vector3 v)
    {
        return new Vector2(v.X, v.Z);
    }
    
    public static Vector3 Vector3XZ(this Vector2 v)
    {
        return new Vector3(v.X, 0, v.Y);
    }
}