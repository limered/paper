namespace valleyfold.Fold;

public record struct Id(int Value)
{
    public static implicit operator int(Id id) => id.Value;
    public static implicit operator Id(int id) => new(id);
}