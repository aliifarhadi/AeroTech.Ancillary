using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class AirportServiceSpecification
    {
        private const string Name = nameof(AirportServiceSpecification);
        private const int TerminalRefMaxLength = 30;
        private const int TimeZoneMaxLength = 64;
        private const int ComponentCodeMaxLength = 30;

        private static readonly Dictionary<string, AirportServiceKind> Kinds = new(StringComparer.Ordinal)
        {
            [AncillaryVariant.Lounge] = AirportServiceKind.Lounge,
            [AncillaryVariant.FastTrack] = AirportServiceKind.FastTrack,
            [AncillaryVariant.CipMeetAssist] = AirportServiceKind.Cip
        };

        private readonly List<SpecificationCode> _includedComponentCodes = new();

        private AirportServiceSpecification()
        {
        }

        public AirportServiceKind Kind { get; private set; }

        public int AirportId { get; private set; }

        public string? TerminalRef { get; private set; }

        public long? FacilityId { get; private set; }

        public AirportServiceDirection Direction { get; private set; }

        public TimeOnly? ServiceWindowStart { get; private set; }

        public TimeOnly? ServiceWindowEnd { get; private set; }

        public string? IanaTimeZone { get; private set; }

        public int? VisitDurationMinutes { get; private set; }

        public int? MaxGuestsPerPrimary { get; private set; }

        public bool RequiresSpecificAppointment { get; private set; }

        public IReadOnlyCollection<SpecificationCode> IncludedComponentCodes => _includedComponentCodes.AsReadOnly();

        internal static AirportServiceSpecification Create(AncillaryVariant variant, AirportServiceSpecificationArgs args)
        {
            var kind = Kinds[variant.Code];
            var components = SpecificationRules.Codes(args.IncludedComponentCodes, ComponentCodeMaxLength, Name, nameof(IncludedComponentCodes));
            var hasWindow = args.ServiceWindowStart is not null || args.ServiceWindowEnd is not null;

            Require(args.Kind == kind, nameof(Kind));
            Require(args.AirportId > 0, nameof(AirportId));
            Require(args.TerminalRef is null || SpecificationRules.IsCode(args.TerminalRef, TerminalRefMaxLength), nameof(TerminalRef));
            Require(args.FacilityId is null or > 0, nameof(FacilityId));
            Require(Enum.IsDefined(args.Direction), nameof(Direction));
            Require(!hasWindow || (args.ServiceWindowStart is not null && args.ServiceWindowEnd is not null), nameof(ServiceWindowEnd));
            Require(!hasWindow || args.ServiceWindowStart != args.ServiceWindowEnd, nameof(ServiceWindowEnd));
            Require(hasWindow ? IsTimeZone(args.IanaTimeZone) : args.IanaTimeZone is null || IsTimeZone(args.IanaTimeZone), nameof(IanaTimeZone));
            Require(args.VisitDurationMinutes is null or > 0, nameof(VisitDurationMinutes));
            Require(args.MaxGuestsPerPrimary is null or >= 0, nameof(MaxGuestsPerPrimary));
            Require(kind == AirportServiceKind.Cip ? components.Count > 0 : components.Count == 0, nameof(IncludedComponentCodes));

            var specification = new AirportServiceSpecification
            {
                Kind = kind,
                AirportId = args.AirportId,
                TerminalRef = args.TerminalRef,
                FacilityId = args.FacilityId,
                Direction = args.Direction,
                ServiceWindowStart = args.ServiceWindowStart,
                ServiceWindowEnd = args.ServiceWindowEnd,
                IanaTimeZone = args.IanaTimeZone,
                VisitDurationMinutes = args.VisitDurationMinutes,
                MaxGuestsPerPrimary = args.MaxGuestsPerPrimary,
                RequiresSpecificAppointment = args.RequiresSpecificAppointment
            };

            specification._includedComponentCodes.AddRange(components);

            return specification;
        }

        public AirportServiceSpecificationArgs ToArgs()
            => new(
                Kind,
                AirportId,
                TerminalRef,
                FacilityId,
                Direction,
                ServiceWindowStart,
                ServiceWindowEnd,
                IanaTimeZone,
                VisitDurationMinutes,
                MaxGuestsPerPrimary,
                _includedComponentCodes.Select(row => row.Code).ToList(),
                RequiresSpecificAppointment);

        private static bool IsTimeZone(string? value)
            => value is { Length: >= 1 and <= TimeZoneMaxLength } && TimeZoneInfo.TryFindSystemTimeZoneById(value, out _);

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
