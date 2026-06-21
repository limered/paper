using System.Collections.Generic;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Folding;
using valleyfold.ThreeDeeModels;
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

    // Collaborators — populated by ThreeDeePaperSelector from Statics in
    // production; populated directly by xUnit in tests. The selection states
    // read these instead of reaching into Statics, so the state machine is
    // exercisable through this one seam (F6).
    public Frame Frame { get; set; }
    public Frame3D Frame3d { get; set; }
    public FoldAnimator FoldAnimator { get; set; }
    public ChangeMemory ChangeMemory { get; set; }

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