using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class AssistedTravelSelection
    {
        private const string Name = nameof(AssistedTravelSelection);
        private const int SsrCodeLength = 4;

        private AssistedTravelSelection()
        {
        }

        public string AssistanceSsrCode { get; private set; } = default!;

        internal static AssistedTravelSelection Create(AssistedTravelSelectionArgs args)
        {
            Require(args.AssistanceSsrCode is { Length: SsrCodeLength } && SpecificationRules.IsCode(args.AssistanceSsrCode, SsrCodeLength), nameof(AssistanceSsrCode));

            return new AssistedTravelSelection { AssistanceSsrCode = args.AssistanceSsrCode! };
        }

        public AssistedTravelSelectionArgs ToArgs() => new(AssistanceSsrCode);

        internal bool IsChosenBy(AssistedTravelSelectionArgs? chosen) => AssistanceSsrCode == chosen?.AssistanceSsrCode;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{Name}.{field}");
        }
    }
}
