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
    DateOnly? SalesDiscontinueOn) : IChangeAncillaryServiceDefinitionCommand;

public sealed record TestServiceDefinitionLifecycleCommand(long ServiceDefinitionId)
    : ISuspendAncillaryServiceDefinitionCommand,
        IReactivateAncillaryServiceDefinitionCommand,
        IRetireAncillaryServiceDefinitionCommand,
        IReviseAncillaryServiceDefinitionCommand;

public sealed record TestChangeProvisionCommand(
    long ProvisionId,
    int Sequence,
    DateTimeOffset? SalesEffectiveFrom,
    DateTimeOffset? SalesDiscontinueAt,
    ServiceCoverageScope CoverageScope,
    ProvisionPassengerCriteriaInput? Passenger,
    ProvisionSalesCriteriaInput? Sales,
    ProvisionTravelCriteriaInput? Travel,
    ProvisionFareCriteriaInput? Fare,
    ProvisionAdvancePurchaseInput? AdvancePurchase,
    ProvisionQuantityInput Quantity,
    ProvisionApplicationInput Application,
    ProvisionOutcomeInput Outcome,
    ProvisionFeeInput? Fee,
    ProvisionSettlementInput Settlement,
    ProvisionAvailabilityInput Availability,
    ProvisionFulfillmentInput Fulfillment) : IChangeAncillaryProvisionCommand;

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
        DateOnly? salesDiscontinueOn = null)
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
            salesDiscontinueOn);

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
            null);

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
            source.SalesDiscontinueOn);

    public static TestDefineProvisionCommand Provision(
        long serviceDefinitionId,
        int sequence,
        decimal amount = 25m,
        int currencyId = Eur,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        ServiceCoverageScope coverageScope = ServiceCoverageScope.Sector,
        AncillaryQuantityUnit quantityUnit = AncillaryQuantityUnit.Each,
        int minQuantity = 1,
        int maxQuantity = 1,
        DateTimeOffset? salesEffectiveFrom = null,
        DateTimeOffset? salesDiscontinueAt = null,
        ProvisionPassengerCriteriaInput? passenger = null,
        ProvisionSalesCriteriaInput? sales = null,
        ProvisionTravelCriteriaInput? travel = null,
        ProvisionFareCriteriaInput? fare = null,
        ProvisionAdvancePurchaseInput? advancePurchase = null,
        ProvisionApplicationInput? application = null,
        IReadOnlyList<ProvisionPriceLineInput>? priceLines = null)
    {
        var paid = disposition == CommercialDisposition.Paid;

        return new TestDefineProvisionCommand(
            serviceDefinitionId,
            sequence,
            salesEffectiveFrom,
            salesDiscontinueAt,
            coverageScope,
            new ProvisionQuantityInput(quantityUnit, minQuantity, maxQuantity),
            application ?? new ProvisionApplicationInput(ProvisionApplicationType.Standard),
            new ProvisionOutcomeInput(disposition, paid, false),
            paid
                ? new ProvisionFeeInput(
                    FeeApplicationUnit.Item,
                    currencyId,
                    priceLines ?? [new ProvisionPriceLineInput(AncillaryPriceLineCategory.Ancillary, null, "Service", amount)])
                : null,
            new ProvisionSettlementInput(ReissueRefundPolicy.NonRefundable, null, false, false),
            new ProvisionAvailabilityInput(false),
            new ProvisionFulfillmentInput("Ancillary"),
            passenger,
            sales,
            travel,
            fare,
            advancePurchase);
    }

    public static TestChangeProvisionCommand Change(long provisionId, TestDefineProvisionCommand source)
        => new(
            provisionId,
            source.Sequence,
            source.SalesEffectiveFrom,
            source.SalesDiscontinueAt,
            source.CoverageScope,
            source.Passenger,
            source.Sales,
            source.Travel,
            source.Fare,
            source.AdvancePurchase,
            source.Quantity,
            source.Application,
            source.Outcome,
            source.Fee,
            source.Settlement,
            source.Availability,
            source.Fulfillment);

    public static ProvisionPassengerCriteriaInput Passengers(params PassengerTypeCode[] passengerTypeCodes) => new(passengerTypeCodes);

    public static ProvisionRoutePairInput Pair(
        int originAirportId,
        int destinationAirportId,
        RoutePairDirection direction = RoutePairDirection.Directional)
        => new(originAirportId, destinationAirportId, direction);

    public static ProvisionApplicationInput Baggage(
        decimal? weight,
        int? firstExcessPiece = null,
        int? lastExcessPiece = null,
        BaggageTravelApplication? travelApplication = null,
        BaggagePurchaseApplication purchaseApplication = BaggagePurchaseApplication.Prepaid,
        BaggageRuleDeference? ruleDeference = null,
        int? freePieces = null,
        WeightUnit weightUnit = WeightUnit.Kg)
        => new(
            ProvisionApplicationType.Baggage,
            new ProvisionBaggageApplicationInput(
                freePieces,
                firstExcessPiece,
                lastExcessPiece,
                weight,
                weightUnit,
                travelApplication,
                purchaseApplication,
                ruleDeference));

    public static ProvisionApplicationInput Seat(string[]? seatNumbers, string[]? seatCharacteristicCodes)
        => new(ProvisionApplicationType.Seat, Seat: new ProvisionSeatApplicationInput(seatNumbers, seatCharacteristicCodes));

    public static IReadOnlyList<ProvisionPriceLineInput> Price(decimal amount, string name = "Service")
        => [new ProvisionPriceLineInput(AncillaryPriceLineCategory.Ancillary, null, name, amount)];
}
