using System.Collections.Generic;
using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public record ChangeRecord()
{
    public Id PickedVertex { get; init; }
    public Vector3 FoldLineA { get; init; }
    public Vector3 FoldLineB { get; init; }
    public List<Id> AddedVertices { get; init; } = new();
    public List<Id> AddedEdges { get; init; } = new();
}