using System.Collections.Generic;

namespace valleyfold.Fold;

public class Face
{
    public Id[] Vertices;
    public List<Id> Edges = new();
    public List<Id> Faces = new();
}