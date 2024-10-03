using System.Collections.Generic;
using Godot;
using valleyfold.Models;
using Corner = valleyfold.Models.Corner;

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