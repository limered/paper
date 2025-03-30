using valleyfold.TwoDeeModels;

namespace valleyfold.Folding;

public interface IFold
{
    Id Apply(Frame frame);
}