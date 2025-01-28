using Godot;

namespace valleyfold.Render.Vertices;

public partial class VertexRendering : MeshInstance3D
{
    private ArrayMesh _mesh;
    private ShaderMaterial _shader;
    
    public override void _Ready()
    {
        _mesh = new ArrayMesh();
        Mesh = _mesh;

        _shader = new ShaderMaterial();
        _shader.Shader = ResourceLoader.Load<Shader>("res://src/Render/Vertices/vertex.gdshader");
        MaterialOverride = _shader;
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        _mesh.ClearSurfaces();
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetCustomFormat(0, SurfaceTool.CustomFormat.Max); // ToDo add custom attributes for size
        
        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            AddQuad(st, frame.Vertices[i].Coord, i);
        }

        st.Commit(_mesh);
    }

    private static void AddQuad(SurfaceTool st, Vector3 offset, int index)
    {
        const float size = 0.02f;
        
        st.SetUV(new Vector2(0, 0));
        st.AddVertex(new Vector3(-size, 0, -size) + offset);
        
        st.SetUV(new Vector2(1, 0));
        st.AddVertex(new Vector3(size, 0, -size) + offset);
        
        st.SetUV(new Vector2(1, 1));
        st.AddVertex(new Vector3(size, 0, size) + offset);
        
        st.SetUV(new Vector2(0, 1));
        
        st.AddVertex(new Vector3(-size, 0, size) + offset);

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