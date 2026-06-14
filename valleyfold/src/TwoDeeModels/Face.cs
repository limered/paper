using System.Collections.Generic;

namespace valleyfold.TwoDeeModels;

public class Face
{
    /// <summary>
    /// Monotonically-assigned identifier (see <see cref="Frame.AddFace"/>).
    /// Stable across the face's lifetime, but NOT equal to the face's current
    /// index in <see cref="Frame.Faces"/> (splits remove faces, shifting indices).
    /// A face constructed without going through <see cref="Frame.AddFace"/>
    /// keeps the default <see cref="Id"/> value; tests that construct faces by
    /// hand should assign explicit Ids.
    /// </summary>
    public Id Id;

    public List<Id> Vertices = new();

    public bool IsBased = true;
    public bool IsUp = true;
}
