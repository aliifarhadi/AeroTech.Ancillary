using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class AssistedTravelSpecification
    {
        private const string Name = nameof(AssistedTravelSpecification);

        private static readonly Dictionary<string, AssistanceKind> Kinds = new(StringComparer.Ordinal)
        {
            [AncillaryVariant.Wheelchair] = AssistanceKind.Wheelchair,
            [AncillaryVariant.DisabilityAssistance] = AssistanceKind.DisabilityAssistance,
            [AncillaryVariant.MedicalEquipment] = AssistanceKind.MedicalEquipment,
            [AncillaryVariant.Bassinet] = AssistanceKind.Bassinet,
            [AncillaryVariant.UnaccompaniedMinor] = AssistanceKind.UnaccompaniedMinor
        };

        private AssistedTravelSpecification()
        {
        }

        public AssistanceKind AssistanceKind { get; private set; }

        public WheelchairDetails? Wheelchair { get; private set; }

        public DisabilityAssistanceDetails? DisabilityAssistance { get; private set; }

        public MedicalEquipmentDetails? MedicalEquipment { get; private set; }

        public BassinetDetails? Bassinet { get; private set; }

        public UnaccompaniedMinorDetails? UnaccompaniedMinor { get; private set; }

        internal static AssistedTravelSpecification Create(AncillaryVariant variant, AssistedTravelSpecificationArgs args)
        {
            var kind = Kinds[variant.Code];

            Require(args.AssistanceKind == kind, nameof(AssistanceKind));
            Require(args.Wheelchair is not null == (kind == AssistanceKind.Wheelchair), nameof(Wheelchair));
            Require(args.DisabilityAssistance is not null == (kind == AssistanceKind.DisabilityAssistance), nameof(DisabilityAssistance));
            Require(args.MedicalEquipment is not null == (kind == AssistanceKind.MedicalEquipment), nameof(MedicalEquipment));
            Require(args.Bassinet is not null == (kind == AssistanceKind.Bassinet), nameof(Bassinet));
            Require(args.UnaccompaniedMinor is not null == (kind == AssistanceKind.UnaccompaniedMinor), nameof(UnaccompaniedMinor));

            return new AssistedTravelSpecification
            {
                AssistanceKind = kind,
                Wheelchair = args.Wheelchair is null ? null : WheelchairDetails.Create(args.Wheelchair),
                DisabilityAssistance = args.DisabilityAssistance is null ? null : DisabilityAssistanceDetails.Create(args.DisabilityAssistance),
                MedicalEquipment = args.MedicalEquipment is null ? null : MedicalEquipmentDetails.Create(args.MedicalEquipment),
                Bassinet = args.Bassinet is null ? null : BassinetDetails.Create(args.Bassinet),
                UnaccompaniedMinor = args.UnaccompaniedMinor is null ? null : UnaccompaniedMinorDetails.Create(args.UnaccompaniedMinor)
            };
        }

        public AssistedTravelSpecificationArgs ToArgs()
            => new(
                AssistanceKind,
                Wheelchair?.ToArgs(),
                DisabilityAssistance?.ToArgs(),
                MedicalEquipment?.ToArgs(),
                Bassinet?.ToArgs(),
                UnaccompaniedMinor?.ToArgs());

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
