using System.Linq;
using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace valleyfold.Interaction.SelectionStates;

public class StartPointSelectionState : ISelectionState
{
    public ISelectionState OnInput(SelectionContext ctx, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return this;
        if (mouseEvent.ButtonIndex != MouseButton.Left || !mouseEvent.Pressed) return this;
        
        StartFoldInteraction(ctx);
        return new EndPointSelectionState();
    }
    
    private static void StartFoldInteraction(SelectionContext ctx)
    {
        if (ctx.PickedVertex == -1) return;
        if (ctx.PreviewLine is null)
        {
            ctx.PreviewLine = new PreviewLine();
            ctx.Parent.AddChild(ctx.PreviewLine);
        }

        ctx.PreviewLine.ChangeVisibility(true);
        ctx.PreviewLine.LineColor(Colors.Aqua);
        ctx.PreviewLine.Draw();
    }
    

    public ISelectionState OnProcess(SelectionContext ctx)
    {
        var frame = ctx.Frame;
        var frame3d = ctx.Frame3d;
        
        frame.UnmarkVertices();
        
        ctx.PickedVertex = MarkVertexInFrame(frame3d, ctx);
        frame3d.ImportMetadataFromFrame(frame);
        
        return this;
    }
    
    private static Id MarkVertexInFrame(Frame3D frame3d, SelectionContext ctx)
    {
        if (frame3d.Vertices is null || !frame3d.Vertices.Any()) return -1;
        var closestId = 0;
        var closestDistance = float.MaxValue;
        for (var i = 0; i < frame3d.Vertices.Count; i++)
        {
            var vertex = frame3d.Vertices[i].Coord;
            var distance = vertex.DistanceTo(ctx.MousePosition.Current);
            if (!(distance < closestDistance)) continue;
            
            closestDistance = distance;
            closestId = i;
        }
        
        if(closestDistance > ctx.Parent.PickingThreshold) return -1;
        
        ctx.Frame.Vertices[closestId].IsSelected = true;
        return closestId;
    }
}