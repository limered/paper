using System.Collections.Generic;
using Godot;

namespace valleyfold.Fold;

public class Vertex
{
    public uint Index;
    public Vector2 Coord;
    public List<Edge> Edges = new();
    public List<Face> Faces = new();
}