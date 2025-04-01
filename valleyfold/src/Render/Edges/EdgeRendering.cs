using System.Collections.Generic;
using Godot;
using valleyfold.TwoDeeModels;
using valleyfold.Ui.Events;
using valleyfold.Utils;

namespace valleyfold.Render.Edges;

public partial class EdgeRendering : Node3D
{
    private const float SelectedLineWidth = 0.02f;
    private const float DeselectedLineWidth = 0.005f;
    public override void _Ready()
    {
        EventBus.Register<ResetPaperEvent>(_ => Clear());
    }

    private void Clear()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
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