using System.Collections.Generic;
using Godot;
using valleyfold.Models;

namespace valleyfold;

public static class Statics
{
    public static PaperFace StartingPaper =>
        new(new List<Vector3>
            {
                new(-1, 0, -1),
                new(-1, 0, 1),
                new(1, 0, 1),
                new(1, 0, -1),
            }, true);
}