using System.Collections.Generic;

namespace valleyfold.Fold;

public class Edge
{
    /**
     * Vertices[0] is first
     */
    public Id[] Vertices = new Id[2];
    
    /**
     * Faces[0] is the LEFT face
     */
    public Id[] Faces = new Id[2];
    public Assignment Assignment;
    public float FoldAngle;
    public float Length;
}