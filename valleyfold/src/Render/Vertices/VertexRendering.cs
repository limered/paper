using Godot;

namespace valleyfold.Render.Vertices;

public partial class VertexRendering : MeshInstance3D
{
    private ArrayMesh _mesh;
    
    public override void _Ready()
    {
        _mesh = new ArrayMesh();
        Mesh = _mesh;
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        _mesh.ClearSurfaces();
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        
        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            AddQuad(st, frame.Vertices[i].Coord, i);
        }

        st.Commit(_mesh);
    }

    private static void AddQuad(SurfaceTool st, Vector3 offset, int index)
    {
        st.SetUV(new Vector2(0, 0));
        st.AddVertex(new Vector3(0, 0, 0) + offset);
        
        
        st.SetUV(new Vector2(1, 0));
        st.AddVertex(new Vector3(1, 0, 0) + offset);
        
        
        st.SetUV(new Vector2(1, 1));
        st.AddVertex(new Vector3(1, 1, 0) + offset);
        
        
        st.SetUV(new Vector2(0, 1));
        st.AddVertex(new Vector3(0, 1, 0) + offset);


        // Add custom attribute for circle properties
        // st.AddCustom(new Vector2((float)index / quadCount, GD.Randf()));

        st.AddIndex(index * 4 + 0);
        st.AddIndex(index * 4 + 1);
        st.AddIndex(index * 4 + 2);
        st.AddIndex(index * 4 + 0);
        st.AddIndex(index * 4 + 2);
        st.AddIndex(index * 4 + 3);
    }
}