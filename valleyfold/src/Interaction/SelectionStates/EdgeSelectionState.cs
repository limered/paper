using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Interaction.SelectionStates;

public class EdgeSelectionState : ISelectionState
{
    public ISelectionState OnInput(SelectionContext ctx, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return this;
        if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            ConfirmUnfoldInteraction(ctx);
        }
        return this;
    }
    
    private static void ConfirmUnfoldInteraction(SelectionContext ctx)
    {
        if(ctx.HoveredEdgeIds == null || !ctx.HoveredEdgeIds.Any()) return;
        var edge = Statics.Frame.Edges[ctx.HoveredEdgeIds.First()];
        var changeContainingEdge = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if (changeContainingEdge is null) return;
        
        changeContainingEdge.Unfolded = true;
        for (var i = 0; i < changeContainingEdge.AddedEdges.Count; i++)
        {
            var edgeToChange = Statics.Frame.Edges[changeContainingEdge.AddedEdges[i]];
            edgeToChange.Assignment = Assignment.F;
            edgeToChange.FoldAngle = 0f;
        }

        var newChange = new ChangeRecord
        {
            ChangeType = ChangeType.Unfold,
            Unfolded = true,
        };
        Statics.ChangeMemory.AddChange(newChange);
        EventBus.Emit(new PaperFoldedEvent());
        ctx.HoveredEdgeIds = null;
    }

    public ISelectionState OnProcess(SelectionContext ctx)
    {
        ShowLastFoldedEdges();
        HoverEdgeToUnfold(ctx);
        return this;
    }
    
    private static void HoverEdgeToUnfold(SelectionContext ctx)
    {
        var frame3d = Statics.Frame3d;
        
        var nearestEdges = EdgeQueries.NearestEdgesToPoint3d(frame3d, ctx.MousePosition.Current);
        if (!nearestEdges.Any()) return;
        var (_, edge) = EdgeQueries.NearestPointOnEdgeToPoint(frame3d, ctx.MousePosition.Current, nearestEdges);
        if (!edge.IsUnfoldable() || !IsLastFolded(edge)) return;
        
        var change = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if(change is null) return;

        ctx.HoveredEdgeIds = change.AddedEdges;
    }
    
    private static bool IsLastFolded(Edge edge)
    {
        var lastUnfolded = Statics.ChangeMemory.LastStillFoldedChange();
        return lastUnfolded?.AddedEdges.Contains(edge.Id) ?? false;
    }
    
    private static void ShowLastFoldedEdges()
    {
        var frame = Statics.Frame;
        frame.UnmarkEdges();
        
        var lastFolded = Statics.ChangeMemory.LastStillFoldedChange();
        if (lastFolded is null) return;
        for (var i = 0; i < lastFolded.AddedEdges.Count; i++)
        {
            var addedEdgeId = lastFolded.AddedEdges[i];
            frame.Edges[addedEdgeId].IsSelected = true;
        }
    }
}