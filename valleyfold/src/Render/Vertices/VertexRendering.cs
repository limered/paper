using Godot;

namespace valleyfold.Render.Vertices;

public partial class VertexRendering : MultiMeshInstance3D
{
    private Mesh _baseMesh;
    
    public override void _Ready()
    {
        _baseMesh = new BoxMesh();
        ((BoxMesh)_baseMesh).Size = new Vector3(0.5f, 0.5f, 0.5f);
        Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        Multimesh.InstanceCount = 0;
        MaterialOverride = new StandardMaterial3D();
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        
        Multimesh.InstanceCount = frame.Vertices.Count;
        Multimesh.Mesh = _baseMesh;

        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            var trans = new Transform3D().Translated(frame.Vertices[i].Coord);
            Multimesh.SetInstanceTransform(i, trans);
        }
    }
}