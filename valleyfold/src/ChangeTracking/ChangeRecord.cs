using System.Collections.Generic;
using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public record struct ChangeRecord()
{
    public bool PickedByVertex { get; init; } = default;
    public (Id start, Id end) FoldLine { get; init; } = new();

    public Id PickedVertex { get; init; } = default;

    public Id PickedEdge { get; init; } = default;
    public float PickedEdgeT { get; init; } = default;
    public Vector3 StartPoint { get; init; } = default;
    public Vector3 FoldLineA { get; init; } = default;
    public Vector3 FoldLineB { get; init; } = default;
    public List<Id> AddedVertices { get; init; } = new();
}