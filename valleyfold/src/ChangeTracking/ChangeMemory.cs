using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace valleyfold.ChangeTracking;

public class ChangeMemory
{
    private readonly List<ChangeRecord> _changes = new();

    public ReadOnlyCollection<ChangeRecord> Changes => new(_changes);

    public void AddChange(ChangeRecord changeRecord)
    {
        _changes.Add(changeRecord);
    }
}