namespace valleyfold.TwoDeeModels;

public enum Assignment
{
    B, // Border
    M, // Mountain Fold
    V, // Valley Fold
    F, // Unfolded ( M/V than open)
    U, // Unspecified Fold
    R // Refold mode sentinel — never stored on an Edge, only used as FoldModeChange.NextFoldMode
}