using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public sealed class ServiceSpecificationInputValidator : AbstractValidator<ServiceSpecificationInput>
    {
        public ServiceSpecificationInputValidator()
        {
            RuleFor(input => input.Baggage!).SetValidator(new BaggageSpecificationInputValidator()).When(input => input.Baggage is not null);
            RuleFor(input => input.Seat!).SetValidator(new SeatSpecificationInputValidator()).When(input => input.Seat is not null);
            RuleFor(input => input.Upgrade!).SetValidator(new UpgradeSpecificationInputValidator()).When(input => input.Upgrade is not null);
            RuleFor(input => input.Meal!).SetValidator(new MealSpecificationInputValidator()).When(input => input.Meal is not null);
            RuleFor(input => input.Pet!).SetValidator(new PetSpecificationInputValidator()).When(input => input.Pet is not null);
            RuleFor(input => input.AssistedTravel!).SetValidator(new AssistedTravelSpecificationInputValidator()).When(input => input.AssistedTravel is not null);
            RuleFor(input => input.AirportService!).SetValidator(new AirportServiceSpecificationInputValidator()).When(input => input.AirportService is not null);
            RuleFor(input => input.Priority!).SetValidator(new PrioritySpecificationInputValidator()).When(input => input.Priority is not null);
            RuleFor(input => input.Connectivity!).SetValidator(new ConnectivitySpecificationInputValidator()).When(input => input.Connectivity is not null);
        }
    }

    public sealed class BaggageSpecificationInputValidator : AbstractValidator<BaggageSpecificationInput>
    {
        public BaggageSpecificationInputValidator()
        {
            RuleFor(input => input.ChargeKind).IsInEnum();
            RuleFor(input => input.AllowanceConcept).IsInEnum().When(input => input.AllowanceConcept is not null);
            RuleFor(input => input.EquipmentKind).MaximumLength(20);
            RuleFor(input => input.ChargeCombination).IsInEnum().When(input => input.ChargeCombination is not null);
        }
    }

    public sealed class SeatSpecificationInputValidator : AbstractValidator<SeatSpecificationInput>
    {
        public SeatSpecificationInputValidator()
        {
            RuleFor(input => input.SeatPurpose).IsInEnum();
            RuleForEach(input => input.SeatCharacteristicCodes).NotEmpty().MaximumLength(25);
            RuleFor(input => input.ExtraSeatPurpose).IsInEnum().When(input => input.ExtraSeatPurpose is not null);
        }
    }

    public sealed class UpgradeSpecificationInputValidator : AbstractValidator<UpgradeSpecificationInput>
    {
        public UpgradeSpecificationInputValidator()
        {
            RuleFor(input => input.AllowedUpgradeKind).IsInEnum();
        }
    }

    public sealed class MealSpecificationInputValidator : AbstractValidator<MealSpecificationInput>
    {
        public MealSpecificationInputValidator()
        {
            RuleFor(input => input.MealKind).IsInEnum();
            RuleFor(input => input.MealCode).MaximumLength(4);
            RuleFor(input => input.MenuItemRef).MaximumLength(40);
            RuleFor(input => input.DietaryCode).MaximumLength(20);
            RuleFor(input => input.ExclusiveMealFamilyCode).MaximumLength(30);
        }
    }

    public sealed class PetAnimalInputValidator : AbstractValidator<PetAnimalInput>
    {
        public PetAnimalInputValidator()
        {
            RuleFor(input => input.AnimalType).IsInEnum();
            RuleFor(input => input.OtherCode).MaximumLength(10);
        }
    }

    public sealed class PetSizeBracketInputValidator : AbstractValidator<PetSizeBracketInput>
    {
        public PetSizeBracketInputValidator()
        {
            RuleFor(input => input.Code).MaximumLength(20);
        }
    }

    public sealed class PetSpecificationInputValidator : AbstractValidator<PetSpecificationInput>
    {
        public PetSpecificationInputValidator()
        {
            RuleFor(input => input.TransportMode).IsInEnum();
            RuleForEach(input => input.AllowedAnimalTypes).NotNull().SetValidator(new PetAnimalInputValidator());
            RuleFor(input => input.CarrierDimensionsMaxCm).NotNull();
            RuleForEach(input => input.RequiredDocumentCodes).NotEmpty().MaximumLength(30);
            RuleForEach(input => input.AllowedHoldAnimalSizeBrackets).NotNull().SetValidator(new PetSizeBracketInputValidator());
        }
    }

    public sealed class WheelchairDetailsInputValidator : AbstractValidator<WheelchairDetailsInput>
    {
        public WheelchairDetailsInputValidator()
        {
            RuleForEach(input => input.AllowedSsrCodes).NotEmpty().MaximumLength(4);
            RuleFor(input => input.AssistanceLevelCode).MaximumLength(20);
        }
    }

    public sealed class DisabilityAssistanceDetailsInputValidator : AbstractValidator<DisabilityAssistanceDetailsInput>
    {
        public DisabilityAssistanceDetailsInputValidator()
        {
            RuleForEach(input => input.AllowedSsrCodes).NotEmpty().MaximumLength(4);
            RuleFor(input => input.RequiredCommunicationMethod).IsInEnum().When(input => input.RequiredCommunicationMethod is not null);
        }
    }

    public sealed class MedicalEquipmentDetailsInputValidator : AbstractValidator<MedicalEquipmentDetailsInput>
    {
        public MedicalEquipmentDetailsInputValidator()
        {
            RuleFor(input => input.MedicalServiceCode).MaximumLength(4);
            RuleFor(input => input.EquipmentKind).IsInEnum();
            RuleForEach(input => input.EvidenceTypeCodes).NotEmpty().MaximumLength(30);
        }
    }

    public sealed class BassinetDetailsInputValidator : AbstractValidator<BassinetDetailsInput>
    {
        public BassinetDetailsInputValidator()
        {
            RuleForEach(input => input.CompatibleSeatGroups).NotEmpty().MaximumLength(25);
        }
    }

    public sealed class UnaccompaniedMinorDetailsInputValidator : AbstractValidator<UnaccompaniedMinorDetailsInput>
    {
        public UnaccompaniedMinorDetailsInputValidator()
        {
            RuleFor(input => input.ConnectionPolicy).IsInEnum();
        }
    }

    public sealed class AssistedTravelSpecificationInputValidator : AbstractValidator<AssistedTravelSpecificationInput>
    {
        public AssistedTravelSpecificationInputValidator()
        {
            RuleFor(input => input.AssistanceKind).IsInEnum();
            RuleFor(input => input.Wheelchair!).SetValidator(new WheelchairDetailsInputValidator()).When(input => input.Wheelchair is not null);
            RuleFor(input => input.DisabilityAssistance!).SetValidator(new DisabilityAssistanceDetailsInputValidator()).When(input => input.DisabilityAssistance is not null);
            RuleFor(input => input.MedicalEquipment!).SetValidator(new MedicalEquipmentDetailsInputValidator()).When(input => input.MedicalEquipment is not null);
            RuleFor(input => input.Bassinet!).SetValidator(new BassinetDetailsInputValidator()).When(input => input.Bassinet is not null);
            RuleFor(input => input.UnaccompaniedMinor!).SetValidator(new UnaccompaniedMinorDetailsInputValidator()).When(input => input.UnaccompaniedMinor is not null);
        }
    }

    public sealed class AirportServiceSpecificationInputValidator : AbstractValidator<AirportServiceSpecificationInput>
    {
        public AirportServiceSpecificationInputValidator()
        {
            RuleFor(input => input.Kind).IsInEnum();
            RuleFor(input => input.TerminalRef).MaximumLength(30);
            RuleFor(input => input.Direction).IsInEnum();
            RuleFor(input => input.IanaTimeZone).MaximumLength(64);
            RuleForEach(input => input.IncludedComponentCodes).NotEmpty().MaximumLength(30);
        }
    }

    public sealed class PrioritySpecificationInputValidator : AbstractValidator<PrioritySpecificationInput>
    {
        public PrioritySpecificationInputValidator()
        {
            RuleFor(input => input.Kind).IsInEnum();
            RuleFor(input => input.PriorityZoneCode).MaximumLength(20);
            RuleFor(input => input.PriorityGroupCode).MaximumLength(20);
            RuleFor(input => input.FareBenefitRef).MaximumLength(40);
        }
    }

    public sealed class ConnectivitySpecificationInputValidator : AbstractValidator<ConnectivitySpecificationInput>
    {
        public ConnectivitySpecificationInputValidator()
        {
            RuleFor(input => input.PlanKind).IsInEnum();
            RuleFor(input => input.DeliveryStage).IsInEnum();
            RuleFor(input => input.FulfillmentProviderRef).MaximumLength(50);
        }
    }
}
