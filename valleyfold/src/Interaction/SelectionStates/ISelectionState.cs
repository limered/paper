using Godot;

namespace valleyfold.Interaction.SelectionStates;

public interface ISelectionState
{
    public ISelectionState OnInput(SelectionContext ctx, InputEvent @event);
    public ISelectionState OnProcess(SelectionContext ctx);
}