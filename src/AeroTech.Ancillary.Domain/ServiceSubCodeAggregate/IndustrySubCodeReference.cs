using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate
{
    public static class IndustrySubCodeReference
    {
        public static IReadOnlyList<IndustrySubCode> Entries { get; } =
        [
            new IndustrySubCode("0CC", "BG", null, "B1", null, "FIRST EXCESS BAG", "C", AncillaryDocumentType.EmdAssociated, 1)
        ];

        public static IndustrySubCode? Find(string code)
            => Entries.FirstOrDefault(entry => entry.Code == code);
    }
}
