using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record VariantCase(
    string Code,
    string Reference,
    string SubCode,
    string GroupCode,
    string Name,
    PricingUnit PricingUnit,
    ServiceDateBasis ServiceDateBasis,
    ServiceDefinitionDocumentInput Document,
    DocumentRouting Routing,
    ServiceDefinitionBookingInput Booking,
    CommercialDisposition Disposition,
    PriceOrigin PriceOrigin,
    AncillaryQuantityUnit QuantityUnit,
    int MaxQuantity,
    ServiceCoverageScope CoverageScope,
    InventoryAuthority Authority)
{
    public AncillaryProfile Profile => V122Catalog.ProfileOf(Code);

    public ServiceSpecificationInput Specification => V122Catalog.Specification(Code);

    public ProvisionApplicationType ApplicationType => AncillaryVariant.Find(Code)!.ApplicationType;

    public override string ToString() => Code;
}

public static class V122Catalog
{
    public const long PointOfSale = 9001;
    public const long OtherPointOfSale = 9002;
    public const string QuoteProvider = "QuotePartnerA";

    public static readonly IReadOnlyList<VariantCase> Cases =
    [
        new(
            "A01", "EXTRA_BAG", "XB1", "BG", "Extra checked bag", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "XB1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "XBAG", null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Piece, 6, ServiceCoverageScope.Portion, InventoryAuthority.Unlimited),
        new(
            "A02", "WEIGHT_PACK_10", "XW1", "BG", "Extra weight package 10 kg", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "XW1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "XBAG", null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 2, ServiceCoverageScope.Portion, InventoryAuthority.Unlimited),
        new(
            "A03", "OVERWEIGHT_32", "XO1", "BG", "Overweight bag 23 to 32 kg", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "XO1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Piece, 3, ServiceCoverageScope.Sector, InventoryAuthority.Unlimited),
        new(
            "A04", "OVERSIZE_BAG", "XS1", "BG", "Oversize bag", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "XS1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Piece, 2, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A05", "CABIN_BAG", "XC1", "BG", "Extra cabin bag", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "XC1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Piece, 1, ServiceCoverageScope.Sector, InventoryAuthority.Unlimited),
        new(
            "A06", "SPORT_BIKE", "XE1", "BG", "Bicycle", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "XE1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "BIKE", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Portion, InventoryAuthority.Supplier),
        new(
            "A07", "SEAT_STANDARD", "ST1", "SA", "Standard seat", PricingUnit.PerSeat, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "A", "ST1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "RQST", null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.FlightFlow),
        new(
            "A08", "SEAT_PREFERRED", "ST2", "SA", "Extra legroom seat", PricingUnit.PerSeat, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "A", "ST2"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "RQST", null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.FlightFlow),
        new(
            "A09", "SEAT_EXTRA", "ST3", "SA", "Extra seat for comfort", PricingUnit.PerSeat, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.TicketOrExchange,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "EXST", null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.FlightFlow),
        new(
            "A10", "UPGRADE_BUSINESS", "UP1", "UP", "Upgrade to business", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.TicketOrExchange,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Paid, PriceOrigin.ExternalQuote, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.FlightFlow),
        new(
            "A11", "MEAL_VGML", "MV1", "ML", "Vegetarian special meal", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.NoAncillaryDocument,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "VGML", null),
            CommercialDisposition.Free, PriceOrigin.Free, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Unlimited),
        new(
            "A12", "MEAL_PASTA", "MP1", "ML", "Pre-order pasta", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "G", "MP1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "SPML", null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 2, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A13", "PET_CABIN", "PC1", "PT", "Pet in cabin", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "PC1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "PETC", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Portion, InventoryAuthority.Supplier),
        new(
            "A14", "PET_HOLD", "PH1", "PT", "Pet in hold", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "PH1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "AVIH", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Portion, InventoryAuthority.Supplier),
        new(
            "A15", "WHEELCHAIR", "WC1", "SP", "Wheelchair assistance", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.NoAncillaryDocument,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "WCHR", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Free, PriceOrigin.Free, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Unlimited),
        new(
            "A16", "ASSIST_BLIND", "DA1", "SP", "Assistance for blind passenger", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.NoAncillaryDocument,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "BLND", null),
            CommercialDisposition.Free, PriceOrigin.Free, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Unlimited),
        new(
            "A17", "MEDICAL_OXYGEN", "MO1", "SP", "Medical oxygen", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "E", "MO1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "AOXY", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.ExternalQuote, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A18", "BASSINET", "BS1", "SP", "Bassinet", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.NoAncillaryDocument,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "BSCT", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Free, PriceOrigin.Free, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A19", "UNACCOMPANIED_MINOR", "UM1", "SP", "Unaccompanied minor", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "E", "UM1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "UMNR", null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Portion, InventoryAuthority.Supplier),
        new(
            "A20", "LOUNGE_THR", "LG1", "LG", "Lounge Tehran", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdStandalone, "E", "LG1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A21", "FAST_TRACK_THR", "FT1", "LG", "Fast track Tehran", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdStandalone, "E", "FT1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A22", "CIP_IKA", "CP1", "LG", "CIP meet and assist", PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdStandalone, "E", "CP1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null, ConfirmationRequirement.SubjectToConfirmation),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Supplier),
        new(
            "A23", "PRIORITY_BOARDING", "PB1", "TS", "Priority boarding", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null), DocumentRouting.NoAncillaryDocument,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Free, PriceOrigin.Free, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Unlimited),
        new(
            "A24", "WIFI_FULL_FLIGHT", "WF1", "IE", "Wi-Fi full flight", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdStandalone, "E", "WF1"), DocumentRouting.Emd,
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            CommercialDisposition.Paid, PriceOrigin.Filed, AncillaryQuantityUnit.Each, 1, ServiceCoverageScope.Sector, InventoryAuthority.Supplier)
    ];

    public static ProvisionSalesRestrictionsInput SinglePointOfSale => new(AllowedPointOfSaleIds: [PointOfSale]);

    public static readonly IReadOnlyDictionary<string, (SelectionKind Kind, string[] Fields)> Selection = new Dictionary<string, (SelectionKind, string[])>
    {
        ["A01"] = (SelectionKind.QuantityChoice, ["Quantity", "TravellerRef", "BoundRef"]),
        ["A02"] = (SelectionKind.QuantityChoice, ["PackageProductRef", "Quantity", "TravellerRef", "BoundRef"]),
        ["A03"] = (SelectionKind.TypedForm, ["BagRef", "MeasuredWeightKg", "TravellerRef", "BoundRef"]),
        ["A04"] = (SelectionKind.TypedForm, ["Dimensions", "TravellerRef", "BoundRef"]),
        ["A05"] = (SelectionKind.QuantityChoice, ["Quantity", "TravellerRef", "BoundRef"]),
        ["A06"] = (SelectionKind.TypedForm, ["EquipmentKind", "Dimensions", "WeightKg", "TravellerRef", "BoundRef"]),
        ["A07"] = (SelectionKind.SeatMapSelection, ["SeatNumber", "TravellerRef", "FlightRef"]),
        ["A08"] = (SelectionKind.SeatMapSelection, ["SeatNumber", "ExitRowTermsAccepted", "TravellerRef", "FlightRef"]),
        ["A09"] = (SelectionKind.SeatMapSelection, ["SeatNumber", "Purpose", "TravellerRef", "FlightRef"]),
        ["A10"] = (SelectionKind.ExternalQuote, ["TargetCabinId", "QuoteRef", "TravellerRef", "FlightRef"]),
        ["A11"] = (SelectionKind.TypedForm, ["MealCode", "TravellerRef", "FlightRef"]),
        ["A12"] = (SelectionKind.QuantityChoice, ["MenuItemRef", "Quantity", "TravellerRef", "FlightRef"]),
        ["A13"] = (SelectionKind.TypedForm, ["AnimalType", "CombinedWeightKg", "CarrierDimensions", "DocumentAcknowledgements", "TravellerRef", "BoundRef"]),
        ["A14"] = (SelectionKind.TypedForm, ["AnimalType", "CombinedWeightKg", "CarrierDimensions", "DocumentAcknowledgements", "SizeBracket", "TravellerRef", "BoundRef"]),
        ["A15"] = (SelectionKind.TypedForm, ["AssistanceSsrCode", "TravellerRef", "FlightRef"]),
        ["A16"] = (SelectionKind.TypedForm, ["AssistanceSsrCode", "TravellerRef", "FlightRef"]),
        ["A17"] = (SelectionKind.TypedForm, ["EquipmentCode", "EvidenceDocumentRefs", "OxygenUnits", "TravellerRef", "FlightRef"]),
        ["A18"] = (SelectionKind.TypedForm, ["InfantRef", "GuardianRef", "FlightRef"]),
        ["A19"] = (SelectionKind.TypedForm, ["GuardianHandoffContact", "GuardianPickupContact", "ChildRef", "BoundRef"]),
        ["A20"] = (SelectionKind.TypedForm, ["AirportId", "FacilityRef", "TimeWithOffset", "GuestCount", "TravellerRef"]),
        ["A21"] = (SelectionKind.TypedForm, ["AirportId", "FacilityRef", "TimeWithOffset", "TravellerRef"]),
        ["A22"] = (SelectionKind.TypedForm, ["AirportId", "FacilityRef", "TimeWithOffset", "GuestCount", "TravellerRef"]),
        ["A23"] = (SelectionKind.SimpleOptIn, ["OptIn", "TravellerRef", "FlightRef"]),
        ["A24"] = (SelectionKind.TypedForm, ["PlanCode", "DeviceCount", "TravellerRef", "FlightRef"])
    };

    public static VariantCase Case(string code) => Cases.Single(variant => variant.Code == code);

    public static bool NeedsExternalSupplier(VariantCase variant)
        => variant.PriceOrigin == PriceOrigin.ExternalQuote || variant.Authority == InventoryAuthority.Supplier;

    public static TestDefineServiceDefinitionCommand Define(VariantCase variant, int airlineId, long supplierId)
        => new(
            airlineId,
            supplierId,
            variant.Reference,
            variant.SubCode,
            ServiceSubCodeSource.CarrierDefined,
            "F",
            variant.GroupCode,
            null,
            null,
            null,
            variant.Name,
            null,
            variant.Document,
            variant.Booking,
            null,
            null,
            variant.PricingUnit,
            variant.ServiceDateBasis)
        {
            Variant = variant.Code,
            TypedSpecification = variant.Specification,
            Routing = variant.Routing
        };

    public static TestDefineProvisionCommand Rule(VariantCase variant, long serviceDefinitionId, int sequence = 10)
        => new(
            serviceDefinitionId,
            sequence,
            variant.CoverageScope,
            new ProvisionQuantityInput(variant.QuantityUnit, 1, variant.MaxQuantity),
            variant.ApplicationType,
            new ProvisionOutcomeInput(
                variant.Disposition,
                variant.Disposition == CommercialDisposition.Paid && variant.Document.Type != AncillaryDocumentType.None,
                false),
            new ProvisionSettlementInput(ReissueRefundPolicy.NonRefundable, null, false, false),
            new ProvisionAvailabilityInput(variant.Authority != InventoryAuthority.Unlimited || variant.Disposition == CommercialDisposition.Free),
            new ProvisionFulfillmentInput("Ancillary"),
            new ProvisionPassengerEligibilityInput([Messages.AirPrice.Enums.PassengerTypeCode.ADT, Messages.AirPrice.Enums.PassengerTypeCode.CHD]),
            new ProvisionSalesRestrictionsInput(AllowedPointOfSaleIds: [PointOfSale]),
            FareApplication: variant.ServiceDateBasis == ServiceDateBasis.FlightDeparture ? new ProvisionFareApplicationInput(AllowedCabinClassIds: [1]) : null,
            TravelDate: new ProvisionTravelDateInput([new ProvisionDatePeriodInput(new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31))]),
            AdvancePurchase: new ProvisionAdvancePurchaseInput(24, Messages.AirPrice.Enums.TimeUnit.Hours),
            BaggageApplication: BaggageRule(variant),
            SeatApplication: variant.Profile == AncillaryProfile.Seat ? new ProvisionSeatApplicationInput(null, ["E"]) : null)
        {
            Origin = variant.PriceOrigin,
            QuoteProviderKey = variant.PriceOrigin == PriceOrigin.ExternalQuote ? QuoteProvider : null,
            PetRule = variant.Code switch
            {
                "A13" => new ProvisionPetRuleInput("EU_ENTRY", 16, 6m, ConfirmationRequirement.SubjectToConfirmation),
                "A14" => new ProvisionPetRuleInput(null, null, 60m, ConfirmationRequirement.SubjectToConfirmation),
                _ => null
            },
            AssistedTravelRule = variant.Code switch
            {
                "A15" => new ProvisionAssistedTravelRuleInput(4320, null, null),
                "A17" => new ProvisionAssistedTravelRuleInput(2880, null, true),
                "A19" => new ProvisionAssistedTravelRuleInput(1440, MinorConnectionPolicy.DirectOnly, null),
                _ => null
            },
            AirportServiceRule = variant.Code switch
            {
                "A20" => new ProvisionAirportServiceRuleInput("T1", AirportServiceDirection.Departure, new TimeOnly(6, 0), new TimeOnly(20, 0), null, 1),
                "A22" => new ProvisionAirportServiceRuleInput(null, AirportServiceDirection.Arrival, null, null, 7001, 1),
                _ => null
            }
        };

    public static TestDefineProvisionCommand Fit(TestDefineProvisionCommand command, PricingUnit pricingUnit)
        => pricingUnit switch
        {
            PricingUnit.PerSeat => command with { ApplicationType = ProvisionApplicationType.Seat, SeatApplication = new ProvisionSeatApplicationInput(null, ["W"]) },
            PricingUnit.PerPiece => command with { ApplicationType = ProvisionApplicationType.Baggage, BaggageApplication = P1Commands.Baggage(23m, 1, 1) },
            PricingUnit.PerKilogram => command with
            {
                ApplicationType = ProvisionApplicationType.Baggage,
                BaggageApplication = P1Commands.Baggage(32m, chargeKind: BaggageChargeKind.Overweight, allowanceConcept: null)
            },
            _ => command
        };

    private static ProvisionBaggageApplicationInput? BaggageRule(VariantCase variant)
        => variant.Code switch
        {
            "A01" => P1Commands.Baggage(23m, 1, 6),
            "A02" => P1Commands.Baggage(10m, chargeKind: BaggageChargeKind.WeightPackage, allowanceConcept: BaggageAllowanceConcept.Weight),
            "A03" => P1Commands.Baggage(32m, chargeKind: BaggageChargeKind.Overweight, allowanceConcept: null),
            "A04" => P1Commands.Baggage(32m, chargeKind: BaggageChargeKind.Oversize, allowanceConcept: null),
            "A05" => P1Commands.Baggage(8m, 1, 1),
            "A06" => P1Commands.Baggage(32m, chargeKind: BaggageChargeKind.SpecialEquipment, allowanceConcept: null),
            _ => null
        };

    public static AncillaryProfile ProfileOf(string variantCode) => AncillaryVariant.Find(variantCode)?.Profile ?? 0;

    public static DocumentRouting RoutingOf(AncillaryDocumentType documentType)
        => documentType == AncillaryDocumentType.None ? DocumentRouting.NoAncillaryDocument : DocumentRouting.Emd;

    public static PriceOrigin OriginOf(CommercialDisposition disposition)
        => disposition switch
        {
            CommercialDisposition.Paid => PriceOrigin.Filed,
            CommercialDisposition.Free => PriceOrigin.Free,
            _ => PriceOrigin.NotAvailable
        };

    public static string VariantFor(PricingUnit pricingUnit, ServiceDateBasis serviceDateBasis)
        => (pricingUnit, serviceDateBasis) switch
        {
            (PricingUnit.PerPiece, _) => AncillaryVariant.ExtraCheckedBag,
            (PricingUnit.PerKilogram, _) => AncillaryVariant.Overweight,
            (PricingUnit.PerSeat, _) => AncillaryVariant.StandardSeat,
            (PricingUnit.PerItem, _) => AncillaryVariant.OnboardWifi,
            (_, ServiceDateBasis.ServiceStart) => AncillaryVariant.Lounge,
            _ => AncillaryVariant.PriorityBoardingCheckin
        };

    public static ServiceSpecificationInput Specification(string variantCode)
        => variantCode switch
        {
            "A01" => new(Baggage: new BaggageSpecificationInput(BaggageChargeKind.ExtraPiece, BaggageAllowanceConcept.Piece, null, 23m, null, null, null, null, null, null)),
            "A02" => new(Baggage: new BaggageSpecificationInput(BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 10m, null, null, null, null, null, null, null)),
            "A03" => new(Baggage: new BaggageSpecificationInput(BaggageChargeKind.Overweight, null, null, 32m, 23m, 32m, null, null, null, BaggageChargeCombination.Separate)),
            "A04" => new(Baggage: new BaggageSpecificationInput(
                BaggageChargeKind.Oversize, null, null, 32m, null, null, new DimensionsCmInput(120m, 80m, 60m), 203m, null, BaggageChargeCombination.MutuallyExclusive)),
            "A05" => new(Baggage: new BaggageSpecificationInput(
                BaggageChargeKind.ExtraPiece, BaggageAllowanceConcept.Piece, null, 8m, null, null, new DimensionsCmInput(55m, 40m, 23m), null, null, null)),
            "A06" => new(Baggage: new BaggageSpecificationInput(
                BaggageChargeKind.SpecialEquipment, null, null, 32m, null, null, new DimensionsCmInput(200m, 80m, 40m), null, "BIKE", null)),
            "A07" => new(Seat: new SeatSpecificationInput(SeatPurpose.Standard, [], [], false, false, null, null, false)),
            "A08" => new(Seat: new SeatSpecificationInput(SeatPurpose.Preferred, ["E", "L"], [1], true, false, null, null, false)),
            "A09" => new(Seat: new SeatSpecificationInput(SeatPurpose.ExtraSeat, [], [1], false, true, ExtraSeatPurpose.PassengerComfort, 1, true)),
            "A10" => new(Upgrade: new UpgradeSpecificationInput(1, 2, UpgradeKind.DynamicQuote, true, [5])),
            "A11" => new(Meal: new MealSpecificationInput(MealKind.SpecialRequest, "VGML", null, "VEGETARIAN", 1440, "MAIN_MEAL")),
            "A12" => new(Meal: new MealSpecificationInput(MealKind.PaidPreorder, null, "MENU_PASTA_01", "VEGETARIAN", 2880, "MAIN_MEAL")),
            "A13" => new(Pet: new PetSpecificationInput(
                PetTransportMode.Cabin, [new PetAnimalInput(PetAnimalType.Cat, null), new PetAnimalInput(PetAnimalType.Dog, null)], 8m, new DimensionsCmInput(55m, 40m, 23m), 12,
                ["ENTRY_RULES_ACK"], [])),
            "A14" => new(Pet: new PetSpecificationInput(
                PetTransportMode.Hold, [new PetAnimalInput(PetAnimalType.Cat, null), new PetAnimalInput(PetAnimalType.Dog, null)], 75m, new DimensionsCmInput(125m, 75m, 85m), 12,
                ["HEALTH_CERT"], [new PetSizeBracketInput("MEDIUM", 8m, 32m), new PetSizeBracketInput("LARGE", 32m, 75m)])),
            "A15" => new(AssistedTravel: new AssistedTravelSpecificationInput(AssistanceKind.Wheelchair, Wheelchair: new WheelchairDetailsInput(["WCHC", "WCHR", "WCHS"], "RAMP", 2880))),
            "A16" => new(AssistedTravel: new AssistedTravelSpecificationInput(
                AssistanceKind.DisabilityAssistance, DisabilityAssistance: new DisabilityAssistanceDetailsInput(["BLND", "DEAF", "DPNA"], AssistanceCommunicationMethod.Verbal))),
            "A17" => new(AssistedTravel: new AssistedTravelSpecificationInput(
                AssistanceKind.MedicalEquipment, MedicalEquipment: new MedicalEquipmentDetailsInput("AOXY", true, MedicalEquipmentKind.Oxygen, 2m, ["MEDIF"]))),
            "A18" => new(AssistedTravel: new AssistedTravelSpecificationInput(AssistanceKind.Bassinet, Bassinet: new BassinetDetailsInput(11m, 8, ["BULKHEAD"], true))),
            "A19" => new(AssistedTravel: new AssistedTravelSpecificationInput(
                AssistanceKind.UnaccompaniedMinor, UnaccompaniedMinor: new UnaccompaniedMinorDetailsInput(5, 12, true, MinorConnectionPolicy.DirectOnly, []))),
            "A20" => new(AirportService: new AirportServiceSpecificationInput(
                AirportServiceKind.Lounge, P1Commands.Thr, "T1", null, AirportServiceDirection.Departure, new TimeOnly(6, 0), new TimeOnly(22, 0), "Asia/Tehran", 180, 1, [], false)),
            "A21" => new(AirportService: new AirportServiceSpecificationInput(
                AirportServiceKind.FastTrack, P1Commands.Thr, "T1", null, AirportServiceDirection.Departure, new TimeOnly(5, 0), new TimeOnly(23, 0), "Asia/Tehran", null, 0, [], false)),
            "A22" => new(AirportService: new AirportServiceSpecificationInput(
                AirportServiceKind.Cip, P1Commands.Ika, "CIP", 7001, AirportServiceDirection.Any, null, null, null, 120, 2, ["FAST_TRACK", "LOUNGE", "PORTER"], true)),
            "A23" => new(Priority: new PrioritySpecificationInput(PriorityKind.Boarding, "ZONE1", null, "FLEX_BENEFIT", [P1Commands.Thr])),
            "A24" => new(Connectivity: new ConnectivitySpecificationInput(ConnectivityPlanKind.FullFlight, null, null, 2, [1], PurchaseStage.PreOrder, "WIFIPROVIDER")),
            _ => new()
        };
}
