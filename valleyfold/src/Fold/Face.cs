using System.Collections.Generic;

namespace valleyfold.Fold;

public class Face
{
    public List<Id> Edges = new();
    public List<Id> Faces = new();
    public List<Id> Vertices = new();

    public bool IsBased = true;
    public bool IsUp = true;
}
