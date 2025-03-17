using valleyfold.TwoDeeModels;

namespace valleyfold.Folding;

public interface IFold
{
    (Id start, Id end) Apply(Frame frame);
}