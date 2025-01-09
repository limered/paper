using System.Collections.Generic;

namespace valleyfold.Models;

public enum Assignment
{
    Cm,
    Cv
}

public class Model
{
    public List<ModelFace> Faces = new();
    public List<ModelVertex> Vertices = new();
    public List<ModelEdge> Edges = new();
    public List<Assignment> Assignments = new();
    public List<ModelEdge> TriangulatedEdges = new();
}