using System.Collections.Generic;
using Godot;
using valleyfold.Templates;
using valleyfold.ThreeDeeModels;
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

    public void Render(List<Edge> edges, List<Vector3> vertices, Frame3D frame3d)
    {
        var edgeCount = edges.Count;
        if(GetChildCount() < edgeCount) AddOrShowLines(edges);
        else if(GetChildCount() > edgeCount) HideLines(edges);
        
        for (var i = 0; i < edgeCount; i++)
        {
            var edge = edges[i];
            // Edge nudges to the max layer of its incident faces, so the
            // crease between two layers renders attached to the topmost.
            var nudge = Vector3.Up * (frame3d.LayerOfEdge(edge) * Frame3D.LayerEpsilon);
            var start = vertices[edge.Vertices[0]] + nudge;
            var end = vertices[edge.Vertices[1]] + nudge;

            var child = GetChild<EdgeLine>(i);
            child.ClearPositions();
            foreach (var (s, e) in OrigamiDashPattern.BuildForAssignment(start, end, edge.Assignment))
                child.AddPositions(s, e);
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