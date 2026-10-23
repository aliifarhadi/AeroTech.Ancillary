namespace AeroTech.Ancillary.Shopping.Options
{
    public sealed record SelectionFieldDescriptor(
        string Name,
        string Control,
        bool Required,
        IReadOnlyList<SelectionFieldChoice>? Choices = null,
        string? Unit = null,
        decimal? Min = null,
        decimal? Max = null);
}
