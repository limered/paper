using System.Collections.Generic;
using valleyfold.Folding;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public record struct ChangeRecord()
{
    public IInteraction Interaction { get; } = null;
    public List<Id> FoldedEdges { get; } = new();
}