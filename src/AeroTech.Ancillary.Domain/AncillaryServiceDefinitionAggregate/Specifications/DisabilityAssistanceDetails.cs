using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class DisabilityAssistanceDetails
    {
        private const string Name = nameof(DisabilityAssistanceDetails);
        private const int SsrCodeLength = 4;

        private static readonly string[] SsrCodes = ["BLND", "DEAF", "DPNA"];

        private readonly List<SpecificationCode> _allowedSsrCodes = new();

        private DisabilityAssistanceDetails()
        {
        }

        public AssistanceCommunicationMethod? RequiredCommunicationMethod { get; private set; }

        public IReadOnlyCollection<SpecificationCode> AllowedSsrCodes => _allowedSsrCodes.AsReadOnly();

        internal static DisabilityAssistanceDetails Create(DisabilityAssistanceDetailsArgs args)
        {
            var codes = SpecificationRules.Codes(args.AllowedSsrCodes, SsrCodeLength, Name, nameof(AllowedSsrCodes));

            SpecificationRules.Require(codes.Count > 0 && codes.All(row => SsrCodes.Contains(row.Code)), Name, nameof(AllowedSsrCodes));
            SpecificationRules.Require(
                args.RequiredCommunicationMethod is null || Enum.IsDefined(args.RequiredCommunicationMethod.Value),
                Name,
                nameof(RequiredCommunicationMethod));

            var details = new DisabilityAssistanceDetails { RequiredCommunicationMethod = args.RequiredCommunicationMethod };

            details._allowedSsrCodes.AddRange(codes);

            return details;
        }

        public DisabilityAssistanceDetailsArgs ToArgs() => new(_allowedSsrCodes.Select(row => row.Code).ToList(), RequiredCommunicationMethod);
    }
}
