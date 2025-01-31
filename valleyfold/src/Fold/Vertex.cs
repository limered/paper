using System.Collections.Generic;
using Godot;

namespace valleyfold.Fold;

public class Vertex
{
    public Vector3 Coord;
    public List<Id> Edges = new();
    public List<Id> Faces = new();
    public bool IsSelected;
}