using System.Collections.Generic;
using System.Linq;
using valleyfold.TwoDeeModels;

namespace valleyfold.ChangeTracking;

public class ChangeMemory
{
    private readonly List<ChangeRecord> _changes = new();

    public IReadOnlyList<ChangeRecord> Changes => _changes;

    public void AddChange(ChangeRecord changeRecord)
    {
        ConnectChangeToAffectedChanges(changeRecord);
        _changes.Add(changeRecord);
    }

    private void ConnectChangeToAffectedChanges(ChangeRecord changeRecord)
    {
        foreach (var addedVertex in changeRecord.AddedVertices)
        foreach (var change in _changes)
            if (ContainsEdgeWithVertex(change, addedVertex))
                change.WasSplitBy.Add(_changes.Count);
    }

    private static bool ContainsEdgeWithVertex(ChangeRecord cr, Id addedVertex)
    {
        var frame = Statics.Frame;
        foreach (var edge in cr.AddedEdges)
        {
            var edgeToCheck = frame.Edges[edge];
            if (edgeToCheck.Vertices[0] == addedVertex ||
                edgeToCheck.Vertices[1] == addedVertex)
                return true;
        }

        return false;
    }

    public ChangeRecord[] ChangesToUnfold()
    {
        var result = new List<ChangeRecord>();
        var foldedEdges = _changes.Where(change => !change.Unfolded);
        foreach (var foldedEdge in foldedEdges)
        {
            if (foldedEdge.WasSplitBy.Count == 0)
            {
                result.Add(foldedEdge);
                continue;
            }

            if (AllChildrenUnfolded(foldedEdge)) result.Add(foldedEdge);
        }

        return result.ToArray();
    }

    private bool AllChildrenUnfolded(ChangeRecord change)
    {
        foreach (var child in change.WasSplitBy)
            if (!_changes[child].Unfolded)
                return false;
        return true;
    }

    public ChangeRecord ChangeContainingEdge(Id edgeId)
    {
        return _changes.Find(change => change.AddedEdges.Contains(edgeId));
    }

    public void AddEdgeToExistingChange(Id existingEdge, Id newEdgeId)
    {
        var change = ChangeContainingEdge(existingEdge);
        if (change is null) return;

        change.AddedEdges.Add(newEdgeId);
    }

    public void Clear()
    {
        _changes.Clear();
    }
}