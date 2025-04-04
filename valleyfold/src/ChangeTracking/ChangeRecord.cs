using System.Collections.Generic;
using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public enum ChangeType
{
    ValleyFold,
    Unfold
}

public record ChangeRecord
{
    public ChangeType ChangeType { get; set; } = ChangeType.ValleyFold;
    public Id PickedVertex { get; init; }
    public Vector3 FoldLineA { get; init; }
    public Vector3 FoldLineB { get; init; }
    public List<Id> AddedVertices { get; init; } = new();
    public List<Id> AddedEdges { get; init; } = new();
    public bool Unfolded { get; set; }
    public HashSet<Id> WasSplitBy { get; } = new();
}