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
}