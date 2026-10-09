using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate
{
    public sealed class AncillaryVariant
    {
        public const string ExtraCheckedBag = "A01";
        public const string ExtraWeightPackage = "A02";
        public const string Overweight = "A03";
        public const string Oversize = "A04";
        public const string CabinBag = "A05";
        public const string SpecialEquipment = "A06";
        public const string StandardSeat = "A07";
        public const string PreferredSeat = "A08";
        public const string ExtraSeat = "A09";
        public const string CabinUpgrade = "A10";
        public const string FreeSpecialMeal = "A11";
        public const string PaidPreorderMeal = "A12";
        public const string PetInCabin = "A13";
        public const string PetInHold = "A14";
        public const string Wheelchair = "A15";
        public const string DisabilityAssistance = "A16";
        public const string MedicalEquipment = "A17";
        public const string Bassinet = "A18";
        public const string UnaccompaniedMinor = "A19";
        public const string Lounge = "A20";
        public const string FastTrack = "A21";
        public const string CipMeetAssist = "A22";
        public const string PriorityBoardingCheckin = "A23";
        public const string OnboardWifi = "A24";

        private static readonly ServiceDateBasis[] FlightDated = [ServiceDateBasis.FlightDeparture];
        private static readonly ServiceDateBasis[] FlightOrServiceDated = [ServiceDateBasis.ServiceStart, ServiceDateBasis.FlightDeparture];

        private static readonly Dictionary<string, AncillaryVariant> Registry = new AncillaryVariant[]
        {
            new(ExtraCheckedBag, AncillaryProfile.Baggage, "Extra checked bag", ProvisionApplicationType.Baggage, FlightDated, PricingUnit.PerPiece),
            new(ExtraWeightPackage, AncillaryProfile.Baggage, "Extra weight package", ProvisionApplicationType.Baggage, FlightDated, PricingUnit.PerItem),
            new(Overweight, AncillaryProfile.Baggage, "Overweight bag", ProvisionApplicationType.Baggage, FlightDated, PricingUnit.PerPiece, PricingUnit.PerKilogram),
            new(Oversize, AncillaryProfile.Baggage, "Oversize bag", ProvisionApplicationType.Baggage, FlightDated, PricingUnit.PerPiece),
            new(CabinBag, AncillaryProfile.Baggage, "Cabin bag", ProvisionApplicationType.Baggage, FlightDated, PricingUnit.PerPiece),
            new(SpecialEquipment, AncillaryProfile.Baggage, "Sports and special equipment", ProvisionApplicationType.Baggage, FlightDated, PricingUnit.PerItem, PricingUnit.PerPiece),
            new(StandardSeat, AncillaryProfile.Seat, "Standard seat", ProvisionApplicationType.Seat, FlightDated, PricingUnit.PerSeat),
            new(PreferredSeat, AncillaryProfile.Seat, "Preferred seat", ProvisionApplicationType.Seat, FlightDated, PricingUnit.PerSeat),
            new(ExtraSeat, AncillaryProfile.Seat, "Extra seat", ProvisionApplicationType.Seat, FlightDated, PricingUnit.PerSeat),
            new(CabinUpgrade, AncillaryProfile.Upgrade, "Cabin upgrade", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger, PricingUnit.PerSeat),
            new(FreeSpecialMeal, AncillaryProfile.Meal, "Free special meal", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger, PricingUnit.PerItem),
            new(PaidPreorderMeal, AncillaryProfile.Meal, "Paid pre-order meal", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerItem, PricingUnit.PerPassenger),
            new(PetInCabin, AncillaryProfile.Pet, "Pet in cabin", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerItem),
            new(PetInHold, AncillaryProfile.Pet, "Pet in hold", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerItem),
            new(Wheelchair, AncillaryProfile.AssistedTravel, "Wheelchair", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger),
            new(DisabilityAssistance, AncillaryProfile.AssistedTravel, "Disability assistance", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger),
            new(MedicalEquipment, AncillaryProfile.AssistedTravel, "Medical equipment", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger, PricingUnit.PerItem),
            new(Bassinet, AncillaryProfile.AssistedTravel, "Bassinet", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger, PricingUnit.PerItem),
            new(UnaccompaniedMinor, AncillaryProfile.AssistedTravel, "Unaccompanied minor", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger),
            new(Lounge, AncillaryProfile.AirportService, "Lounge", ProvisionApplicationType.Standard, FlightOrServiceDated, PricingUnit.PerPassenger),
            new(FastTrack, AncillaryProfile.AirportService, "Fast track", ProvisionApplicationType.Standard, FlightOrServiceDated, PricingUnit.PerPassenger),
            new(CipMeetAssist, AncillaryProfile.AirportService, "CIP meet and assist", ProvisionApplicationType.Standard, FlightOrServiceDated, PricingUnit.PerPassenger, PricingUnit.PerItem),
            new(PriorityBoardingCheckin, AncillaryProfile.Priority, "Priority boarding and check-in", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerPassenger),
            new(OnboardWifi, AncillaryProfile.Connectivity, "Onboard Wi-Fi", ProvisionApplicationType.Standard, FlightDated, PricingUnit.PerItem, PricingUnit.PerPassenger)
        }.ToDictionary(variant => variant.Code, StringComparer.Ordinal);

        private AncillaryVariant(
            string code,
            AncillaryProfile profile,
            string name,
            ProvisionApplicationType applicationType,
            IReadOnlyList<ServiceDateBasis> serviceDateBases,
            params PricingUnit[] pricingUnits)
        {
            Code = code;
            Profile = profile;
            Name = name;
            ApplicationType = applicationType;
            ServiceDateBases = serviceDateBases;
            PricingUnits = pricingUnits;
        }

        public string Code { get; }

        public AncillaryProfile Profile { get; }

        public string Name { get; }

        public ProvisionApplicationType ApplicationType { get; }

        public IReadOnlyList<ServiceDateBasis> ServiceDateBases { get; }

        public IReadOnlyList<PricingUnit> PricingUnits { get; }

        public static IReadOnlyCollection<AncillaryVariant> All => Registry.Values;

        public static AncillaryVariant? Find(string? code)
            => code is not null && Registry.TryGetValue(code, out var variant) ? variant : null;
    }
}
