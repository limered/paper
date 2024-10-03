using System.Linq;
using Godot;

namespace valleyfold.Utils;

public static class NodeUtils
{
    public static T[] GetChildNodes<T>(this Node parent)
    {
        return parent.GetChildren().Where(child => child is T).Cast<T>().ToArray();
    }
}