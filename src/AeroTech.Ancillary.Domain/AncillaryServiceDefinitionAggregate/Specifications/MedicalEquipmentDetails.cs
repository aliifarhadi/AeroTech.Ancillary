using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class MedicalEquipmentDetails
    {
        private const string Name = nameof(MedicalEquipmentDetails);
        private const int EvidenceCodeMaxLength = 30;

        private static readonly Dictionary<string, MedicalEquipmentKind> ServiceCodes = new(StringComparer.Ordinal)
        {
            ["AOXY"] = MedicalEquipmentKind.Oxygen,
            ["STCR"] = MedicalEquipmentKind.Stretcher,
            ["MEDA"] = MedicalEquipmentKind.MedicalAssistance
        };

        private readonly List<SpecificationCode> _evidenceTypeCodes = new();

        private MedicalEquipmentDetails()
        {
        }

        public string MedicalServiceCode { get; private set; } = default!;

        public bool RequiresMedicalApproval { get; private set; }

        public MedicalEquipmentKind EquipmentKind { get; private set; }

        public decimal? OxygenUnits { get; private set; }

        public IReadOnlyCollection<SpecificationCode> EvidenceTypeCodes => _evidenceTypeCodes.AsReadOnly();

        internal static MedicalEquipmentDetails Create(MedicalEquipmentDetailsArgs args)
        {
            var evidence = SpecificationRules.Codes(args.EvidenceTypeCodes, EvidenceCodeMaxLength, Name, nameof(EvidenceTypeCodes));

            SpecificationRules.Require(
                args.MedicalServiceCode is not null && ServiceCodes.TryGetValue(args.MedicalServiceCode, out var kind) && kind == args.EquipmentKind,
                Name,
                nameof(MedicalServiceCode));
            SpecificationRules.Require(
                args.EquipmentKind == MedicalEquipmentKind.Oxygen ? SpecificationRules.IsMeasure(args.OxygenUnits) : args.OxygenUnits is null,
                Name,
                nameof(OxygenUnits));
            SpecificationRules.Require(!args.RequiresMedicalApproval || evidence.Count > 0, Name, nameof(EvidenceTypeCodes));

            var details = new MedicalEquipmentDetails
            {
                MedicalServiceCode = args.MedicalServiceCode!,
                RequiresMedicalApproval = args.RequiresMedicalApproval,
                EquipmentKind = args.EquipmentKind,
                OxygenUnits = args.OxygenUnits
            };

            details._evidenceTypeCodes.AddRange(evidence);

            return details;
        }

        public MedicalEquipmentDetailsArgs ToArgs()
            => new(MedicalServiceCode, RequiresMedicalApproval, EquipmentKind, OxygenUnits, _evidenceTypeCodes.Select(row => row.Code).ToList());
    }
}
