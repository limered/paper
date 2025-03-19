using valleyfold.ChangeTracking;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace valleyfold;

public static class Statics
{
    public static Frame Frame { get; set; } 
    public static Game Game { get; set; }

    public static ChangeMemory ChangeMemory { get; } = new();
    public static Frame3D Frame3d { get; } = new();
} 