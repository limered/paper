using valleyfold.ChangeTracking;
using valleyfold.TwoDeeModels;

namespace valleyfold.Folding;

public interface IInteraction
{
    EdgeStrip Draft(Frame frame);
    ChangeRecord ApplyFoldedEdges(Frame frame);
}