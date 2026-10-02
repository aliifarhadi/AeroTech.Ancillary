using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate
{
    public static class IndustrySubCodeReference
    {
        public static IReadOnlyList<IndustrySubCode> Entries { get; } =
        [
            new IndustrySubCode("0CC", "BG", null, "B1", null, "FIRST EXCESS BAG", "C", AncillaryDocumentType.EmdAssociated, 1),
            new IndustrySubCode("0BX", "LG", null, null, null, "LOUNGE ACCESS", "E", AncillaryDocumentType.EmdStandalone, 1)
        ];

        public static IndustrySubCode? Find(string code)
            => Entries.FirstOrDefault(entry => entry.Code == code);
    }
}
