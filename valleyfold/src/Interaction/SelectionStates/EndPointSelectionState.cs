using Godot;
using valleyfold.Folding;
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
    
    private static void ResetFoldInteraction(SelectionContext ctx)
    {
        ctx.PreviewLine?.ChangeVisibility(false);
        ctx.PreviewLine?.Draw();
    }

    private static void ConfirmFoldInteraction(SelectionContext ctx)
    {
        if (ctx.PickedVertex == -1) return;
        FoldInteractionApplier.ApplyVertexValleyFold(ctx.PickedVertex, ctx.MousePosition.Current);
        ctx.PreviewLine?.ChangeVisibility(false);
        ctx.PreviewLine?.Draw();
    }

    public ISelectionState OnProcess(SelectionContext ctx)
    {
        var frame3d = Statics.Frame3d;

        UpdateFoldPreview(frame3d, ctx);

        return this;
    }

    private static void UpdateFoldPreview(
        Frame3D frame3d,
        SelectionContext ctx)
    {
        if (ctx.PickedVertex == -1) return;

        var mousePosition = ctx.MousePosition.Current;
        var centerPoint = frame3d.Vertices[ctx.PickedVertex].Coord.Lerp(mousePosition, 0.5f);
        var direction = (mousePosition - frame3d.Vertices[ctx.PickedVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);
        ctx.PreviewLine.UpdatePosition(centerPoint, perpendicular);

        ctx.PreviewLine.Draw();
    }
}