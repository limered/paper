using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Interaction.SelectionStates;

public class RefoldSelectionState : ISelectionState
{
    public ISelectionState OnInput(SelectionContext ctx, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return this;
        if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            ConfirmRefoldInteraction(ctx);
        }
        return this;
    }

    private static void ConfirmRefoldInteraction(SelectionContext ctx)
    {
        if (Statics.FoldAnimator.IsAnimating) return;
        if (ctx.HoveredEdgeIds == null || !ctx.HoveredEdgeIds.Any()) return;
        var edge = Statics.Frame.Edges[ctx.HoveredEdgeIds.First()];
        var change = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if (change is null || !change.Unfolded) return;

        var restored = change.ChangeType == ChangeType.MountainFold
            ? Assignment.M
            : Assignment.V;

        // Animate 0 → target on the in-flight change; flip Unfolded false
        // and restore edge assignments at completion. PaperRenderer makes
        // an exception for the in-flight change so it renders during the
        // ramp despite Unfolded still being true.
        Statics.FoldAnimator.Start(change, 0f, 1f, () =>
        {
            change.Unfolded = false;
            foreach (var addedEdgeId in change.AddedEdges)
            {
                var e = Statics.Frame.Edges[addedEdgeId];
                e.Assignment = restored;
                e.FoldAngle = change.TargetAngle;
            }
        });
        ctx.HoveredEdgeIds = null;
    }

    public ISelectionState OnProcess(SelectionContext ctx)
    {
        ShowRefoldableEdges();
        HoverEdgeToRefold(ctx);
        return this;
    }

    private static void HoverEdgeToRefold(SelectionContext ctx)
    {
        var frame3d = Statics.Frame3d;
        var nearestEdges = EdgeQueries.NearestEdgesToPoint3d(frame3d, ctx.MousePosition.Current);
        if (!nearestEdges.Any()) return;
        var (_, edge) = EdgeQueries.NearestPointOnEdgeToPoint(frame3d, ctx.MousePosition.Current, nearestEdges);
        if (!edge.IsRefoldable()) return;
        var change = Statics.ChangeMemory.ChangeContainingEdge(edge.Id);
        if (change is null || !change.Unfolded) return;
        if (!Statics.ChangeMemory.ChangesToRefold().Contains(change)) return;
        ctx.HoveredEdgeIds = change.AddedEdges;
    }

    private static void ShowRefoldableEdges()
    {
        var frame = Statics.Frame;
        frame.UnmarkEdges();
        var refoldable = Statics.ChangeMemory.ChangesToRefold();
        foreach (var cr in refoldable)
            for (var i = 0; i < cr.AddedEdges.Count; i++)
                frame.Edges[cr.AddedEdges[i]].IsSelected = true;
    }
}
