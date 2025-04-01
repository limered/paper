using System.Collections.Generic;
using System.Collections.ObjectModel;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public class ChangeMemory
{
    private readonly List<ChangeRecord> _changes = new();

    public ReadOnlyCollection<ChangeRecord> Changes => new(_changes);

    public void AddChange(ChangeRecord changeRecord)
    {
        _changes.Add(changeRecord);
    }
    
    public ChangeRecord LastStillFoldedChange()
    {
        return _changes.FindLast(change => !change.Unfolded);
    }
    
    public ChangeRecord ChangeContainingEdge(Id edgeId)
    {
        return _changes.Find(change => change.AddedEdges.Contains(edgeId));
    }
}