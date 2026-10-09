using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class SeatSpecification
    {
        private const string Name = nameof(SeatSpecification);
        private const int CharacteristicCodeMaxLength = 25;

        private readonly List<SpecificationCode> _seatCharacteristicCodes = new();
        private readonly List<SpecificationReference> _applicableCabins = new();

        private SeatSpecification()
        {
        }

        public SeatPurpose SeatPurpose { get; private set; }

        public bool RequiresExitRowEligibility { get; private set; }

        public bool RequiresAdjacentSeat { get; private set; }

        public ExtraSeatPurpose? ExtraSeatPurpose { get; private set; }

        public int? ExtraOccupiedSeatCount { get; private set; }

        public bool RequiresExternalTicketAction { get; private set; }

        public IReadOnlyCollection<SpecificationCode> SeatCharacteristicCodes => _seatCharacteristicCodes.AsReadOnly();

        public IReadOnlyCollection<SpecificationReference> ApplicableCabins => _applicableCabins.AsReadOnly();

        internal static SeatSpecification Create(AncillaryVariant variant, SeatSpecificationArgs args)
        {
            var isExtraSeat = variant.Code == AncillaryVariant.ExtraSeat;
            var purpose = variant.Code switch
            {
                AncillaryVariant.StandardSeat => SeatPurpose.Standard,
                AncillaryVariant.PreferredSeat => SeatPurpose.Preferred,
                _ => SeatPurpose.ExtraSeat
            };
            var characteristics = SpecificationRules.Codes(args.SeatCharacteristicCodes, CharacteristicCodeMaxLength, Name, nameof(SeatCharacteristicCodes));
            var cabins = SpecificationRules.References(args.ApplicableCabinIds?.Select(id => (long)id), Name, nameof(ApplicableCabins));

            Require(args.SeatPurpose == purpose, nameof(SeatPurpose));
            Require(purpose != SeatPurpose.Preferred || characteristics.Count > 0, nameof(SeatCharacteristicCodes));
            Require(isExtraSeat || !args.RequiresAdjacentSeat, nameof(RequiresAdjacentSeat));
            Require(
                isExtraSeat ? args.ExtraSeatPurpose is not null && Enum.IsDefined(args.ExtraSeatPurpose.Value) : args.ExtraSeatPurpose is null,
                nameof(ExtraSeatPurpose));
            Require(isExtraSeat ? args.ExtraOccupiedSeatCount is >= 1 : args.ExtraOccupiedSeatCount is null, nameof(ExtraOccupiedSeatCount));
            Require(isExtraSeat || !args.RequiresExternalTicketAction, nameof(RequiresExternalTicketAction));

            var specification = new SeatSpecification
            {
                SeatPurpose = args.SeatPurpose,
                RequiresExitRowEligibility = args.RequiresExitRowEligibility,
                RequiresAdjacentSeat = args.RequiresAdjacentSeat,
                ExtraSeatPurpose = args.ExtraSeatPurpose,
                ExtraOccupiedSeatCount = args.ExtraOccupiedSeatCount,
                RequiresExternalTicketAction = args.RequiresExternalTicketAction
            };

            specification._seatCharacteristicCodes.AddRange(characteristics);
            specification._applicableCabins.AddRange(cabins);

            return specification;
        }

        public SeatSpecificationArgs ToArgs()
            => new(
                SeatPurpose,
                _seatCharacteristicCodes.Select(row => row.Code).ToList(),
                _applicableCabins.Select(row => (int)row.ReferenceId).ToList(),
                RequiresExitRowEligibility,
                RequiresAdjacentSeat,
                ExtraSeatPurpose,
                ExtraOccupiedSeatCount,
                RequiresExternalTicketAction);

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
