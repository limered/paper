using Godot;
using valleyfold.Folding;
using valleyfold.FrameModifications;
using valleyfold.ThreeDeeModels;

namespace valleyfold.Interaction.SelectionStates;

public class EndPointSelectionState : ISelectionState
{
    public ISelectionState OnInput(SelectionContext ctx, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return this;
        switch (mouseEvent.ButtonIndex)
        {
            case MouseButton.Left when mouseEvent.Pressed:
                ConfirmFoldInteraction(ctx);
                return new StartPointSelectionState();
            case MouseButton.Right when mouseEvent.Pressed:
                ResetFoldInteraction(ctx);
                return new StartPointSelectionState();
            default:
                return this;
        }
    }

    public ISelectionState OnProcess(SelectionContext ctx)
    {
        var frame3d = Statics.Frame3d;

        SnapToVertex(frame3d, ctx);
        UpdateFoldPreview(frame3d, ctx);

        return this;
    }

    private static void ResetFoldInteraction(SelectionContext ctx)
    {
        ctx.PreviewLine?.ChangeVisibility(false);
        ctx.PreviewLine?.Draw();
    }

    private static void ConfirmFoldInteraction(SelectionContext ctx)
    {
        if (ctx.PickedVertex == -1) return;
        FoldInteractionApplier.ApplyVertexValleyFold(ctx.PickedVertex, ctx.TargetPosition);
        ctx.PreviewLine?.ChangeVisibility(false);
        ctx.PreviewLine?.Draw();
    }

    private void SnapToVertex(Frame3D frame3d, SelectionContext ctx)
    {
        var nearestVertexId = VertexQueries.NearestVertexIdTo(
            Statics.Frame3d, ctx.MousePosition.Current, ctx.Parent.SnapThreshold);
        if (nearestVertexId == -1 || Input.IsKeyPressed(Key.Shift))
        {
            ctx.TargetPosition = ctx.MousePosition.Current;
        }
        else
        {
            var nearestVertex = frame3d.Vertices[nearestVertexId].Coord;
            ctx.TargetPosition = nearestVertex;
        }
    }

    private static void UpdateFoldPreview(
        Frame3D frame3d,
        SelectionContext ctx)
    {
        if (ctx.PickedVertex == -1) return;

        var targetPosition = ctx.TargetPosition;
        var centerPoint = frame3d.Vertices[ctx.PickedVertex].Coord.Lerp(targetPosition, 0.5f);
        var direction = (targetPosition - frame3d.Vertices[ctx.PickedVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);
        ctx.PreviewLine.UpdatePosition(centerPoint, perpendicular);

        ctx.PreviewLine.Draw();
    }
}