using System.Collections.Generic;
using Godot;
using valleyfold.TwoDeeModels;

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

        // Render(frame.Edges, frame.Vertices.Select(v => v.Coord.Vector3XZ()).ToList());
    }

    public void Render(List<Edge> edges, List<Vector3> vertices)
    {
        var edgeCount = edges.Count;
        if(GetChildCount() < edgeCount) AddOrShowLines(edges);
        else if(GetChildCount() > edgeCount) HideLines(edges);
        
        for (var i = 0; i < edgeCount; i++)
        {
            var edge = edges[i];
            var start = vertices[edge.Vertices[0]];
            var end = vertices[edge.Vertices[1]];

            var child = GetChild<EdgeLine>(i);
            child.LinePositions(start, end);
            child.LineWidth(edge.IsSelected ? SelectedLineWidth : DeselectedLineWidth);
            child.Draw();
        }
    }

    private void AddOrShowLines(List<Edge> edges)
    {
        var edgeCount = edges.Count;
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

    private void HideLines(List<Edge> edges)
    {
        var edgeCount = edges.Count;
        for (var i = edgeCount - 1; i < GetChildCount(); i++) GetChild<MeshInstance3D>(i).Hide();
    }
}