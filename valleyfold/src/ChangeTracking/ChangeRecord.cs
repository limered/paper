using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public record struct ChangeRecord()
{
    public (Id start, Id end) FoldLine { get; init; } = new();
    public Vector3 StartPoint { get; init; } = default;
}