using Godot;

namespace valleyfold.Render.Edges;

public partial class EdgeRendering : MeshInstance3D
{
    public override void _Ready()
    {
        Mesh = new ImmediateMesh();
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var mesh = (ImmediateMesh)Mesh;
        
        mesh.ClearSurfaces();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        foreach (var edge in frame.Edges)
        {
            var start = frame.Vertices[edge.Vertices[0]];
            var end = frame.Vertices[edge.Vertices[1]];

            mesh.SurfaceAddVertex(start.Coord);
            mesh.SurfaceAddVertex(end.Coord);
        }

        mesh.SurfaceEnd();
    }
}