namespace AeroTech.Ancillary.Shopping.Options
{
    public sealed record SelectionOptionsForm(string VariantCode, IReadOnlyList<SelectionFieldDescriptor> Fields);
}
