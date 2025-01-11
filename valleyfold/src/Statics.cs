using System.Collections.Generic;
using Godot;
using valleyfold.v1.Models;
using Corner = valleyfold.v1.Models.Corner;

namespace valleyfold;

public static class Statics
{
    public static PaperFace StartingPaper =>
        new(new List<Corner>
        {
            new() { Position = new Vector3(-1, 0, -1) },
            new() { Position = new Vector3(-1, 0, 1) },
            new() { Position = new Vector3(1, 0, 1) },
            new() { Position = new Vector3(1, 0, -1) }
        }, true);
} 