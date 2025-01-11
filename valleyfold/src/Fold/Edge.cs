using System.Collections.Generic;

namespace valleyfold.Fold;

public class Edge
{
    public Id[] Vertices = new Id[2]; 
    public List<Id> Faces;
    public Assignment Assignment;
    public float FoldAngle;
    public float Length;
}