using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class WheelchairDetails
    {
        private const string Name = nameof(WheelchairDetails);
        private const int SsrCodeLength = 4;
        private const int LevelCodeMaxLength = 20;

        private static readonly string[] SsrCodes = ["WCHR", "WCHS", "WCHC"];

        private readonly List<SpecificationCode> _allowedSsrCodes = new();

        private WheelchairDetails()
        {
        }

        public string? AssistanceLevelCode { get; private set; }

        public int LeadTimeMinutes { get; private set; }

        public IReadOnlyCollection<SpecificationCode> AllowedSsrCodes => _allowedSsrCodes.AsReadOnly();

        internal static WheelchairDetails Create(WheelchairDetailsArgs args)
        {
            var codes = SpecificationRules.Codes(args.AllowedSsrCodes, SsrCodeLength, Name, nameof(AllowedSsrCodes));

            SpecificationRules.Require(codes.Count > 0 && codes.All(row => SsrCodes.Contains(row.Code)), Name, nameof(AllowedSsrCodes));
            SpecificationRules.Require(
                args.AssistanceLevelCode is null || SpecificationRules.IsCode(args.AssistanceLevelCode, LevelCodeMaxLength),
                Name,
                nameof(AssistanceLevelCode));
            SpecificationRules.Require(args.LeadTimeMinutes >= 0, Name, nameof(LeadTimeMinutes));

            var details = new WheelchairDetails { AssistanceLevelCode = args.AssistanceLevelCode, LeadTimeMinutes = args.LeadTimeMinutes };

            details._allowedSsrCodes.AddRange(codes);

            return details;
        }

        public WheelchairDetailsArgs ToArgs() => new(_allowedSsrCodes.Select(row => row.Code).ToList(), AssistanceLevelCode, LeadTimeMinutes);
    }
}
