using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record TestRegisterSupplierCommand(
    int OwnerAirlineId,
    string Name,
    SupplierFulfillmentKind FulfillmentKind,
    string? FulfillmentProviderKey) : IRegisterSupplierCommand;

public sealed record TestDefineServiceDefinitionCommand(
    int OwnerAirlineId,
    long SupplierId,
    string ServiceDefinitionRef,
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
    PricingUnit PricingUnit = PricingUnit.PerPassenger) : IDefineAncillaryServiceDefinitionCommand;

public sealed record TestActivateServiceDefinitionCommand(long ServiceDefinitionId) : IActivateAncillaryServiceDefinitionCommand;

public sealed record TestDefineProvisionCommand(
    long ServiceDefinitionId,
    int Sequence,
    DateTimeOffset? SalesEffectiveFrom,
    DateTimeOffset? SalesDiscontinueAt,
    ServiceCoverageScope CoverageScope,
    ProvisionQuantityInput Quantity,
    ProvisionApplicationInput Application,
    ProvisionOutcomeInput Outcome,
    ProvisionSettlementInput Settlement,
    ProvisionAvailabilityInput Availability,
    ProvisionFulfillmentInput Fulfillment,
    ProvisionPassengerCriteriaInput? Passenger = null,
    ProvisionSalesCriteriaInput? Sales = null,
    ProvisionTravelCriteriaInput? Travel = null,
    ProvisionFareCriteriaInput? Fare = null,
    ProvisionAdvancePurchaseInput? AdvancePurchase = null) : IDefineAncillaryProvisionCommand;

public sealed record TestActivateProvisionCommand(long ProvisionId) : IActivateAncillaryProvisionCommand;

public sealed record TestHoldAncillaryServicesCommand(
    string IdempotencyKey,
    long OrderId,
    string Reference,
    DateTimeOffset? RequestedExpiresAt,
    IReadOnlyList<AncillaryHoldServiceInput> Services) : IHoldAncillaryServicesCommand;

public sealed record TestConfirmAncillaryHoldCommand(long HoldId) : IConfirmAncillaryHoldCommand;

public static class M1Commands
{
    public const int Currency = 70;

    public static TestRegisterSupplierCommand LocalSupplier(int airlineId, string name = "Dot Air")
        => new(airlineId, name, SupplierFulfillmentKind.Local, null);

    public static TestRegisterSupplierCommand ExternalSupplier(int airlineId, string providerKey = "LoungePartnerA")
        => new(airlineId, "Partner Lounge", SupplierFulfillmentKind.External, providerKey);

    public static TestDefineServiceDefinitionCommand LoungeDefinition(int airlineId, long supplierId, string reference = "LNG_IKA_CIP")
        => new(
            airlineId,
            supplierId,
            reference,
            "0BX",
            ServiceSubCodeSource.Industry,
            null,
            null,
            null,
            null,
            null,
            "Lounge access",
            "CIP lounge at departure",
            new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdStandalone, "E", "0BX"),
            new ServiceDefinitionBookingInput(BookingMethod.NoBookingProcessRequired, null, null),
            null,
            null);

    public static TestDefineProvisionCommand LoungeProvision(
        long serviceDefinitionId,
        int sequence = 100,
        int minQuantity = 1,
        int maxQuantity = 1,
        CommercialDisposition disposition = CommercialDisposition.Free)
        => new(
            serviceDefinitionId,
            sequence,
            null,
            null,
            ServiceCoverageScope.Sector,
            new ProvisionQuantityInput(AncillaryQuantityUnit.Each, minQuantity, maxQuantity),
            new ProvisionApplicationInput(ProvisionApplicationType.Standard),
            new ProvisionOutcomeInput(disposition, disposition == CommercialDisposition.Paid, false),
            new ProvisionSettlementInput(ReissueRefundPolicy.NonRefundable, null, false, false),
            new ProvisionAvailabilityInput(false),
            new ProvisionFulfillmentInput("Ancillary"));

    public static TestDefinePricingCommand LoungePricing(
        long provisionId,
        decimal amount = 2500000m,
        FeeApplicationUnit feeApplicationUnit = FeeApplicationUnit.Item)
        => new(
            provisionId,
            Currency,
            feeApplicationUnit,
            [new PricingLineInput(null, null, null, AncillaryPriceLineCategory.Ancillary, null, "Lounge access", null, null, amount)]);

    public static TestHoldAncillaryServicesCommand Hold(
        string idempotencyKey,
        long orderId,
        long serviceDefinitionId,
        long provisionId,
        DateTimeOffset? requestedExpiresAt,
        params (long OrderServiceId, long TravellerId, long FlightId)[] units)
        => new(
            idempotencyKey,
            orderId,
            $"ORD-{orderId}",
            requestedExpiresAt,
            units
                .Select(unit => new AncillaryHoldServiceInput(
                    unit.OrderServiceId,
                    serviceDefinitionId,
                    provisionId,
                    unit.TravellerId,
                    ServiceCoverageScope.Sector,
                    [unit.FlightId],
                    1))
                .ToList());
}
