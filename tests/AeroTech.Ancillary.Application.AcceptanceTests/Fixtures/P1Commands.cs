using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record TestRetireSupplierCommand(long SupplierId) : IRetireSupplierCommand;

public sealed record TestChangeServiceDefinitionCommand(
    long ServiceDefinitionId,
    long SupplierId,
    string ServiceSubCode,
    ServiceSubCodeSource SubCodeSource,
    string? ServiceTypeCode,
    string? GroupCode,
    string? SubGroupCode,
    string? Description1Code,
    string? Description2Code,
    string CommercialName,
    string? Description,
    ServiceDefinitionDocumentInput Document,
    ServiceDefinitionBookingInput Booking,
    DateOnly? SalesEffectiveFrom,
    DateOnly? SalesDiscontinueOn,
    PricingUnit PricingUnit = PricingUnit.PerPassenger,
    ServiceDateBasis ServiceDateBasis = ServiceDateBasis.FlightDeparture) : IChangeAncillaryServiceDefinitionCommand
{
    public string? Variant { get; init; }

    public ServiceSpecificationInput? TypedSpecification { get; init; }

    public DocumentRouting? Routing { get; init; }

    public string VariantCode => Variant ?? V122Catalog.VariantFor(PricingUnit, ServiceDateBasis);

    public AncillaryProfile Profile => V122Catalog.ProfileOf(VariantCode);

    public DocumentRouting DocumentRouting => Routing ?? V122Catalog.RoutingOf(Document.Type);

    public ServiceSpecificationInput Specification => TypedSpecification ?? V122Catalog.Specification(VariantCode);
}

public sealed record TestServiceDefinitionLifecycleCommand(long ServiceDefinitionId)
    : ISuspendAncillaryServiceDefinitionCommand,
        IReactivateAncillaryServiceDefinitionCommand,
        IRetireAncillaryServiceDefinitionCommand,
        IReviseAncillaryServiceDefinitionCommand;

public sealed record TestChangeProvisionCommand(
    long ProvisionId,
    int Sequence,
    ServiceCoverageScope CoverageScope,
    ProvisionQuantityInput Quantity,
    ProvisionApplicationType ApplicationType,
    ProvisionOutcomeInput Outcome,
    ProvisionSettlementInput Settlement,
    ProvisionAvailabilityInput Availability,
    ProvisionFulfillmentInput Fulfillment,
    ProvisionPassengerEligibilityInput? PassengerEligibility = null,
    ProvisionSalesRestrictionsInput? SalesRestrictions = null,
    ProvisionGeographyInput? Geography = null,
    ProvisionFlightApplicationInput? FlightApplication = null,
    ProvisionFareApplicationInput? FareApplication = null,
    ProvisionTravelDateInput? TravelDate = null,
    ProvisionDayTimeApplicationInput? DayTimeApplication = null,
    ProvisionAdvancePurchaseInput? AdvancePurchase = null,
    ProvisionBaggageApplicationInput? BaggageApplication = null,
    ProvisionSeatApplicationInput? SeatApplication = null,
    PurchaseStage PurchaseStage = PurchaseStage.Both) : IChangeAncillaryProvisionCommand
{
    public ProvisionSalesRestrictionsInput? SalesRestrictions { get; init; } = SalesRestrictions ?? V122Catalog.SinglePointOfSale;

    public PriceOrigin? Origin { get; init; }

    public PriceOrigin PriceOrigin => Origin ?? V122Catalog.OriginOf(Outcome.Disposition);

    public string? QuoteProviderKey { get; init; }

    public ProvisionPetRuleInput? PetRule { get; init; }

    public ProvisionAssistedTravelRuleInput? AssistedTravelRule { get; init; }

    public ProvisionAirportServiceRuleInput? AirportServiceRule { get; init; }
}

public sealed record TestProvisionLifecycleCommand(long ProvisionId)
    : ISuspendAncillaryProvisionCommand, IReactivateAncillaryProvisionCommand, IRetireAncillaryProvisionCommand;

public static class P1Commands
{
    public const int Eur = 47;
    public const int Irr = 70;
    public const int Thr = 1;
    public const int Ika = 2;
    public const int Mhd = 3;
    public const int Ist = 6;

    public static TestDefineServiceDefinitionCommand CarrierDefinition(
        int airlineId,
        long supplierId,
        string reference,
        string serviceSubCode,
        string serviceTypeCode,
        string groupCode,
        string commercialName,
        ServiceDefinitionBookingInput? booking = null,
        ServiceDefinitionDocumentInput? document = null,
        string? subGroupCode = null,
        string? description = null,
        DateOnly? salesEffectiveFrom = null,
        DateOnly? salesDiscontinueOn = null,
        PricingUnit pricingUnit = PricingUnit.PerPassenger,
        ServiceDateBasis serviceDateBasis = ServiceDateBasis.FlightDeparture,
        string? variant = null)
        => new(
            airlineId,
            supplierId,
            reference,
            serviceSubCode,
            ServiceSubCodeSource.CarrierDefined,
            serviceTypeCode,
            groupCode,
            subGroupCode,
            null,
            null,
            commercialName,
            description,
            document ?? new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null),
            booking ?? new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            salesEffectiveFrom,
            salesDiscontinueOn,
            pricingUnit,
            serviceDateBasis)
        {
            Variant = variant
        };

    public static TestDefineServiceDefinitionCommand FirstExcessBagDefinition(int airlineId, long supplierId, string reference = "XBAG_FIRST")
        => new(
            airlineId,
            supplierId,
            reference,
            "0CC",
            ServiceSubCodeSource.Industry,
            null,
            null,
            null,
            null,
            null,
            "First excess bag",
            "One additional checked piece",
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "C", "0CC"),
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "XBAG", null),
            null,
            null,
            PricingUnit.PerPiece);

    public static ServiceDefinitionBookingInput Ssr(string code) => new(BookingMethod.Ssr, code, null);

    public static TestChangeServiceDefinitionCommand Change(long serviceDefinitionId, TestDefineServiceDefinitionCommand source)
        => new(
            serviceDefinitionId,
            source.SupplierId,
            source.ServiceSubCode,
            source.SubCodeSource,
            source.ServiceTypeCode,
            source.GroupCode,
            source.SubGroupCode,
            source.Description1Code,
            source.Description2Code,
            source.CommercialName,
            source.Description,
            source.Document,
            source.Booking,
            source.SalesEffectiveFrom,
            source.SalesDiscontinueOn,
            source.PricingUnit,
            source.ServiceDateBasis)
        {
            Variant = source.Variant,
            TypedSpecification = source.TypedSpecification,
            Routing = source.Routing
        };

    public static TestDefineProvisionCommand Provision(
        long serviceDefinitionId,
        int sequence,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        ServiceCoverageScope coverageScope = ServiceCoverageScope.Sector,
        AncillaryQuantityUnit quantityUnit = AncillaryQuantityUnit.Each,
        int minQuantity = 1,
        int maxQuantity = 1,
        ProvisionApplicationType applicationType = ProvisionApplicationType.Standard,
        bool bookingRequired = false)
        => new(
            serviceDefinitionId,
            sequence,
            coverageScope,
            new ProvisionQuantityInput(quantityUnit, minQuantity, maxQuantity),
            applicationType,
            new ProvisionOutcomeInput(disposition, disposition == CommercialDisposition.Paid, bookingRequired),
            new ProvisionSettlementInput(ReissueRefundPolicy.NonRefundable, null, false, false),
            new ProvisionAvailabilityInput(false),
            new ProvisionFulfillmentInput("Ancillary"));

    public static TestChangeProvisionCommand Change(long provisionId, TestDefineProvisionCommand source)
        => new(
            provisionId,
            source.Sequence,
            source.CoverageScope,
            source.Quantity,
            source.ApplicationType,
            source.Outcome,
            source.Settlement,
            source.Availability,
            source.Fulfillment,
            source.PassengerEligibility,
            source.SalesRestrictions,
            source.Geography,
            source.FlightApplication,
            source.FareApplication,
            source.TravelDate,
            source.DayTimeApplication,
            source.AdvancePurchase,
            source.BaggageApplication,
            source.SeatApplication,
            source.PurchaseStage)
        {
            Origin = source.Origin,
            QuoteProviderKey = source.QuoteProviderKey,
            PetRule = source.PetRule,
            AssistedTravelRule = source.AssistedTravelRule,
            AirportServiceRule = source.AirportServiceRule
        };

    public static ProvisionPassengerEligibilityInput Passengers(params PassengerTypeCode[] passengerTypeCodes) => new(passengerTypeCodes);

    public static ProvisionRoutePairInput Pair(
        int originAirportId,
        int destinationAirportId,
        RoutePairDirection direction = RoutePairDirection.Directional)
        => new(originAirportId, destinationAirportId, direction);

    public static ProvisionBaggageApplicationInput Baggage(
        decimal? weight,
        int? firstExcessPiece = null,
        int? lastExcessPiece = null,
        BaggageTravelApplication? travelApplication = null,
        BaggagePurchaseApplication purchaseApplication = BaggagePurchaseApplication.Prepaid,
        BaggageRuleDeference? ruleDeference = null,
        int? freePieces = null,
        WeightUnit weightUnit = WeightUnit.Kg,
        BaggageChargeKind? chargeKind = BaggageChargeKind.ExtraPiece,
        BaggageAllowanceConcept? allowanceConcept = BaggageAllowanceConcept.Piece)
        => new(
            freePieces,
            firstExcessPiece,
            lastExcessPiece,
            weight,
            weightUnit,
            travelApplication,
            purchaseApplication,
            ruleDeference,
            chargeKind,
            allowanceConcept);

    public static ProvisionSeatApplicationInput Seat(string[]? seatNumbers, string[]? seatCharacteristicCodes) => new(seatNumbers, seatCharacteristicCodes);
}
