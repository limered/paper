using Godot;

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
    public static readonly Color BehindEdge = new("9eb5c0"); // light blue-grey for occluded creases
}
