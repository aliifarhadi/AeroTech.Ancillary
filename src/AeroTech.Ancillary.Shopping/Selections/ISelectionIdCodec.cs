using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public interface ISelectionIdCodec
    {
        string Issue(SignedSelection selection);

        Task<SignedSelectionReading> ReadAsync(string? id, SelectionIdKind kind);
    }
}
