using valleyfold.Fold;

namespace valleyfold.Folding;

public interface IInteraction
{
    EdgeStrip Draft(Frame frame);
    void ApplyFoldedEdges(Frame frame);
}