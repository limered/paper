using System.Collections.Generic;

namespace valleyfold.Fold;

public enum Assignment
{
    B, // Border
    M, // Mountain Fold
    V, // Valley Fold
    F, // Unfolded ( M/V than open)
    U, // Unspecified Fold
    C, // Cut Fold
    J  // Join (Flat triangulated polygon edge)
}

public class Frame
{
    public List<Face> Faces = new();
    public List<Vertex> Vertices = new();
    public List<Edge> Edges = new();
    public List<Assignment> Assignments = new();
    public List<Edge> TriangulatedEdges = new();
}