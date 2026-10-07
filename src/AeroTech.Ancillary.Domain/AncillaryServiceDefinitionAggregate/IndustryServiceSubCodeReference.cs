using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate
{
    public static class IndustryServiceSubCodeReference
    {
        public static IReadOnlyList<IndustryServiceSubCodeEntry> Entries { get; } =
        [
            new IndustryServiceSubCodeEntry("0BX", "F", "E", "LG", null, null, null, "LOUNGE ACCESS", AncillaryDocumentType.EmdStandalone),
            new IndustryServiceSubCodeEntry("0CC", "C", "C", "BG", null, "B1", null, "FIRST EXCESS BAG", AncillaryDocumentType.EmdAssociated)
        ];

        public static IndustryServiceSubCodeEntry? Find(string code)
            => Entries.FirstOrDefault(entry => entry.Code == code);
    }

    public sealed record IndustryServiceSubCodeEntry(
        string Code,
        string ServiceTypeCode,
        string? Rfic,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        AncillaryDocumentType? DocumentType);
}
