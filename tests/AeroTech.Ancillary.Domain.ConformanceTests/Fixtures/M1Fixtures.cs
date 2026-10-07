using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class M1Fixtures
{
    public const int Airline = 1;
    public const int Currency = 70;

    public static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    public static Supplier LocalSupplier(long id = 2001, int ownerAirlineId = Airline, string name = "Dot Air")
        => Supplier.Register(id, ownerAirlineId, name, SupplierFulfillmentKind.Local, null, Now);

    public static Supplier ExternalSupplier(long id = 2002, string providerKey = "LoungePartnerA")
        => Supplier.Register(id, Airline, "Partner Lounge", SupplierFulfillmentKind.External, providerKey, Now);

    public static AncillaryServiceDefinition LoungeDefinition(long id = 1001, long supplierId = 2001, string reference = "LNG_IKA_CIP")
        => AncillaryServiceDefinition.Define(
            id,
            Airline,
            supplierId,
            reference,
            1,
            "0BX",
            ServiceSubCodeSource.Industry,
            new ServiceDefinitionClassificationArgs(null, null, null, null, null),
            "Lounge access",
            null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "0BX"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null,
            null,
            Now);

    public static AncillaryProvision LoungeProvision(
        long id = 501,
        long serviceDefinitionId = 1001,
        int sequence = 100,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        FeeApplicationUnit feeApplicationUnit = FeeApplicationUnit.Item,
        int minQuantity = 1,
        int maxQuantity = 1,
        ServiceCoverageScope coverageScope = ServiceCoverageScope.Sector,
        IReadOnlyList<ProvisionPriceLineArgs>? priceLines = null)
        => AncillaryProvision.Define(
            id,
            serviceDefinitionId,
            sequence,
            null,
            null,
            coverageScope,
            PassengerCriteria.Create(null),
            SalesCriteria.Create(null, null, null),
            TravelCriteria.Create(null, null, null, null, null, null, null, null, null, null, null, null, null),
            [],
            FareCriteria.Create(null, null, null, null, null, null),
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Each, minQuantity, maxQuantity),
            ProvisionApplication.Create(ProvisionApplicationType.Standard, null, null),
            CommercialOutcome.Create(disposition, disposition == CommercialDisposition.Paid, false),
            disposition == CommercialDisposition.Paid ? FeeDefinition.Create(Currency, feeApplicationUnit) : null,
            disposition == CommercialDisposition.Paid
                ? priceLines ?? [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, "Lounge access", 2500000m)]
                : [],
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            new SequentialIdGenerator(),
            Now);

    public static AncillaryReservationUnitArgs Unit(
        long orderServiceId = 9001,
        long serviceDefinitionId = 1001,
        long provisionId = 501,
        long? travellerId = 71,
        ServiceCoverageScope coverageScope = ServiceCoverageScope.Sector,
        IReadOnlyList<long>? coveredFlightIds = null,
        int quantity = 1)
        => new(
            orderServiceId,
            serviceDefinitionId,
            provisionId,
            travellerId,
            coverageScope,
            coveredFlightIds ?? [301],
            quantity);
}
