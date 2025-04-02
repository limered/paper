namespace valleyfold.Interaction;

public class InteractionState
{
    public PickingMode Current { get; private set; } = PickingMode.StartPoint;

    public void GoToState(PickingMode state)
    {
        Current = state;
    }
}