using System.Collections.Generic;
using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.Interaction.SelectionStates;

public class SelectionContext
{
    public Id PickedVertex { get; set; }
    public PreviewLine PreviewLine { get; set; }
    public List<Id> HoveredEdgeIds { get; set; }
    public ThreeDeePaperSelector Parent { get; set; }
    public MousePosition MousePosition { get; set; }
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// Currently-selected fold direction. Driven by
    /// <see cref="valleyfold.Ui.FoldModeChange"/>. <see cref="Assignment.V"/>
    /// applies a valley fold on confirm; <see cref="Assignment.M"/> applies
    /// a mountain fold. Other assignments (F, U, B) are routed by
    /// <see cref="ThreeDeePaperSelector"/> into different selection states
    /// and never reach the fold-confirm branch.
    /// </summary>
    public Assignment FoldMode { get; set; } = Assignment.V;
}