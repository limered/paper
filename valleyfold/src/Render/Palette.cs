using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.Render;

/// <summary>
/// Named colours sampled from <c>valleyfold/palette/paper-pixels-1x.png</c>.
/// Only the swatches actually used by the renderer are exposed — the full
/// palette image is the source of truth; add a name here when something
/// new needs to reference a colour.
/// </summary>
public static class Palette
{
    public static readonly Color BorderEdge = new("a97e5c"); // warm paper-edge brown
    public static readonly Color MountainEdge = new("a05e5e"); // muted terracotta
    public static readonly Color ValleyEdge = new("6d838e"); // cool blue-grey
    public static readonly Color UnspecifiedEdge = new("937b6a"); // neutral taupe (F / U)
    public static readonly Color BehindEdge = new("9eb5c0"); // light blue-grey for occluded creases

    /// <summary>
    /// Pick the edge colour for a given assignment. Borders read as solid
    /// paper outline; M/V follow the Yoshizawa convention of warm vs cool;
    /// F and U fall back to a neutral tone.
    /// </summary>
    public static Color ForAssignment(Assignment assignment) => assignment switch
    {
        Assignment.B => BorderEdge,
        Assignment.M => MountainEdge,
        Assignment.V => ValleyEdge,
        _ => UnspecifiedEdge,
    };
}
