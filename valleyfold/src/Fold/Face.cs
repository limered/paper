using System.Collections.Generic;

namespace valleyfold.Fold;

public class Face
{
    public List<Id> Vertices = new();

    public bool IsBased = true;
    public bool IsUp = true;
}
