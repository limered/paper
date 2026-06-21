using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
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
        if (ctx.FoldAnimator.IsAnimating) return;
        if(ctx.HoveredEdgeIds == null || !ctx.HoveredEdgeIds.Any()) return;
        var edge = ctx.Frame.Edges[ctx.HoveredEdgeIds.First()];
        var change = ctx.ChangeMemory.ChangeContainingEdge(edge.Id);
        if (change is null) return;

        // Animate target → 0 on the in-flight change; flip Unfolded and
        // reset edge assignments at completion so the renderer's skip
        // condition lines up with the post-animation steady state.
        ctx.FoldAnimator.Start(change, 1f, 0f, () =>
        {
            change.Unfolded = true;
            for (var i = 0; i < change.AddedEdges.Count; i++)
            {
                var edgeToChange = ctx.Frame.Edges[change.AddedEdges[i]];
                edgeToChange.Assignment = Assignment.F;
                edgeToChange.FoldAngle = 0f;
            }
            ctx.ChangeMemory.AddChange(new ChangeRecord
            {
                ChangeType = ChangeType.Unfold,
                Unfolded = true,
            });
        });
        ctx.HoveredEdgeIds = null;
    }

    public ISelectionState OnProcess(SelectionContext ctx)
    {
        ShowLastFoldedEdges(ctx);
        HoverEdgeToUnfold(ctx);
        return this;
    }
    
    private static void HoverEdgeToUnfold(SelectionContext ctx)
    {
        var frame3d = ctx.Frame3d;
        
        var nearestEdges = EdgeQueries.NearestEdgesToPoint3d(frame3d, ctx.MousePosition.Current);
        if (!nearestEdges.Any()) return;
        var (_, edge) = EdgeQueries.NearestPointOnEdgeToPoint(frame3d, ctx.MousePosition.Current, nearestEdges);
        if (!edge.IsUnfoldable() || !IsLastFolded(ctx, edge)) return;
        
        var change = ctx.ChangeMemory.ChangeContainingEdge(edge.Id);
        if(change is null) return;

        ctx.HoveredEdgeIds = change.AddedEdges;
    }
    
    private static bool IsLastFolded(SelectionContext ctx, Edge edge)
    {
        var lastUnfolded = ctx.ChangeMemory.ChangesToUnfold();
        return lastUnfolded?.FirstOrDefault(cr => cr.AddedEdges.Contains(edge.Id)) is not null;
    }
    
    private static void ShowLastFoldedEdges(SelectionContext ctx)
    {
        var frame = ctx.Frame;
        frame.UnmarkEdges();
        
        var lastFolded = ctx.ChangeMemory.ChangesToUnfold();
        if (lastFolded is null || lastFolded.Length == 0) return;
        foreach (var changeRecord in lastFolded)
        {
            for (var i = 0; i < changeRecord.AddedEdges.Count; i++)
            {
                var addedEdgeId = changeRecord.AddedEdges[i];
                frame.Edges[addedEdgeId].IsSelected = true;
            }
        }
    }
}