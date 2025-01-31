using System.Linq;
using Godot;
using valleyfold.Fold;

namespace valleyfold.Render.Edges;

public partial class EdgeRendering : Node3D
{
    private const float SelectedLineWidth = 0.02f;
    private const float DeselectedLineWidth = 0.01f;
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        var edgeCount = frame.Edges.Count;
        if(GetChildCount() < edgeCount) AddOrShowLines(frame);
        else if(GetChildCount() > edgeCount) HideLines(frame);
        
        for (var i = 0; i < edgeCount; i++)
        {
            var edge = frame.Edges[i];
            var start = frame.Vertices[edge.Vertices[0]];
            var end = frame.Vertices[edge.Vertices[1]];

            var child = GetChild<EdgeLine>(i);
            child.LinePositions(start.Coord, end.Coord);
            child.LineWidth(edge.IsSelected ? SelectedLineWidth : DeselectedLineWidth);
            child.Draw();
        }
    }

    private void AddOrShowLines(Frame frame)
    {
        var edgeCount = frame.Edges.Count;
        for (var i = GetChildCount() - 1; i < edgeCount; i++)
        {
            var child = GetChildOrNull<MeshInstance3D>(i);
            if (child == null)
            {
                var newLine = new EdgeLine();
                AddChild(newLine);
            }
            else
            {
                child.Show();
            }
        }
    }

    private void HideLines(Frame frame)
    {
        var edgeCount = frame.Edges.Count;
        for (var i = edgeCount - 1; i < GetChildCount(); i++) GetChild<MeshInstance3D>(i).Hide();
    }
}