using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using DomainIds = AeroTech.Ancillary.Domain.ConformanceTests.Fakes.SequentialIdGenerator;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class ShoppingLab
{
    public const int Airline = 1;
    public const long Pos = 9001;
    public const long OtherPos = 9002;
    public const int Eur = 47;
    public const int Usd = 155;
    public const int Jpy = 75;
    public const int Kwd = 82;
    public const string Quote = "QuotePartnerA";

    public static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly DomainIds _ids = new();
    private readonly Dictionary<long, Supplier> _suppliers = [];

    public InMemoryCatalog Catalog { get; } = new();

    public AncillaryShoppingEngine Engine => new(Catalog, Catalog, Catalog, Catalog, Catalog, Catalog, Catalog, Catalog, Catalog);

    public AncillaryServiceDefinition Definition(
        string code,
        string? reference = null,
        Func<ServiceSpecificationInput, ServiceSpecificationInput>? specification = null,
        PricingUnit? pricingUnit = null,
        ServiceDateBasis? basis = null,
        bool activate = true,
        DateOnly? salesFrom = null,
        DateOnly? salesUntil = null,
        int owner = Airline,
        ConfirmationRequirement? confirmation = null)
    {
        var variant = V122Catalog.Case(code);
        var external = V122Catalog.NeedsExternalSupplier(variant);
        var supplier = Supplier.Register(
            _ids.NewId(),
            owner,
            external ? "Partner" : "Dot Air",
            external ? SupplierFulfillmentKind.External : SupplierFulfillmentKind.Local,
            external ? Quote : null,
            Now);
        var input = specification is null ? variant.Specification : specification(variant.Specification);
        var definition = AncillaryServiceDefinition.Define(
            _ids.NewId(),
            owner,
            supplier.Id,
            reference ?? variant.Reference,
            1,
            variant.SubCode,
            ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", variant.GroupCode, null, null, null),
            new ServiceDefinitionProfileArgs(variant.Profile, code, variant.Routing, ServiceSpecificationInputMapper.ToArgs(input)),
            pricingUnit ?? variant.PricingUnit,
            basis ?? variant.ServiceDateBasis,
            variant.Name,
            null,
            DocumentDefinition.Create(variant.Document.Type, variant.Document.Rfic, variant.Document.Rfisc),
            BookingDefinition.Create(
                variant.Booking.Method,
                variant.Booking.SsrCode,
                variant.Booking.SsimCode,
                confirmation ?? variant.Booking.ConfirmationRequirement ?? ConfirmationRequirement.Immediate),
            salesFrom,
            salesUntil,
            Now);

        if (activate)
            definition.Activate(supplier, Now);

        _suppliers[definition.Id] = supplier;
        Catalog.Definitions.Add(definition);

        return definition;
    }

    public AncillaryProvision Provision(
        AncillaryServiceDefinition definition,
        int sequence = 10,
        ProvisionRulesArgs? rules = null,
        ServiceCoverageScope? scope = null,
        PurchaseStage? stage = null,
        CommercialDisposition? disposition = null,
        PriceOrigin? origin = null,
        int? maxQuantity = null,
        bool mustCheck = false,
        long pos = Pos,
        bool activate = true)
    {
        var variant = V122Catalog.Case(definition.VariantCode!);
        var outcome = disposition ?? variant.Disposition;
        var priceOrigin = origin ?? (disposition is null
            ? variant.PriceOrigin
            : disposition switch
            {
                CommercialDisposition.Paid => PriceOrigin.Filed,
                CommercialDisposition.Free => PriceOrigin.Free,
                _ => PriceOrigin.NotAvailable
            });
        var authored = rules ?? Rules();
        var sales = authored.SalesRestrictions is { } restrictions
            ? restrictions with { PointOfSaleIds = [pos] }
            : new ProvisionSalesRestrictionsArgs(null, null, [pos], [], []);
        var baggage = authored.BaggageApplication ?? (definition.Baggage is { } specification
            ? new ProvisionBaggageApplicationArgs(
                null,
                null,
                null,
                specification.PackageWeightKg,
                WeightUnit.Kg,
                null,
                BaggagePurchaseApplication.Prepaid,
                null,
                specification.ChargeKind,
                specification.AllowanceConcept)
            : null);
        var provision = AncillaryProvision.Define(
            _ids.NewId(),
            definition.Id,
            sequence,
            scope ?? variant.CoverageScope,
            stage ?? (definition.Connectivity is { DeliveryStage: PurchaseStage.OnBoard } ? PurchaseStage.OnBoard : PurchaseStage.Both),
            priceOrigin,
            priceOrigin == PriceOrigin.ExternalQuote ? Quote : null,
            QuantityRule.Create(variant.QuantityUnit, 1, maxQuantity ?? variant.MaxQuantity),
            variant.ApplicationType,
            CommercialOutcome.Create(outcome, outcome == CommercialDisposition.Paid && definition.Document.Type != AncillaryDocumentType.None, false),
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(mustCheck),
            FulfillmentDefinition.Create("Ancillary"),
            authored with
            {
                SalesRestrictions = sales,
                BaggageApplication = baggage,
                SeatApplication = authored.SeatApplication ?? (definition.Seat is null ? null : new ProvisionSeatApplicationArgs([], ["E"]))
            },
            _ids,
            Now);

        if (activate)
            provision.Activate(definition, _suppliers[definition.Id].QuoteAuthorityKey(), Now);

        Catalog.Provisions.Add(provision);

        return provision;
    }

    public AncillaryPricing Price(AncillaryServiceDefinition definition, AncillaryProvision provision, params AncillaryPricingRateArgs[] rates)
    {
        var scales = Catalog.CurrencyScales;
        var pricing = AncillaryPricing.Define(_ids.NewId(), provision.Id, definition.PricingUnit!.Value, 1, rates, scales, _ids, Now);

        pricing.Activate(scales, Now);
        Catalog.Pricings.Add(pricing);

        return pricing;
    }

    public (AncillaryServiceDefinition Definition, AncillaryProvision Provision) Sellable(string code, decimal amount = 100m, ProvisionRulesArgs? rules = null)
    {
        var definition = Definition(code);
        var provision = Provision(definition, rules: rules);

        if (provision.PriceOrigin == PriceOrigin.Filed)
            Price(definition, provision, Rate(amount));

        return (definition, provision);
    }

    public AncillaryInventoryPolicy Policy(
        AncillaryServiceDefinition definition,
        InventoryAuthority authority,
        LocalInventoryPattern? pattern = null,
        FlightCountConsumption? count = null,
        FlightWeightConsumption? weight = null,
        AirportSlotConsumption? slot = null,
        InventoryRecordStatus status = InventoryRecordStatus.Active,
        IReadOnlyList<FlightInventorySource>? flightSources = null,
        IReadOnlyList<SlotInventorySource>? slotSources = null,
        params PassengerUsageLimitArgs[] limits)
    {
        var providerKey = authority switch
        {
            InventoryAuthority.Supplier => Quote,
            InventoryAuthority.FlightFlow => "FlightFlow",
            _ => null
        };
        var policy = AncillaryInventoryPolicy.Define(
            _ids.NewId(),
            definition.OwnerAirlineId,
            definition.ServiceDefinitionRef,
            new InventoryPolicyArgs(definition.Id, authority, pattern, providerKey, count, weight, slot, limits),
            _ids,
            Now);

        if (status != InventoryRecordStatus.Draft)
            policy.Activate(
                new InventoryPolicyEvidence(
                    definition.Id,
                    definition.PricingUnit,
                    providerKey,
                    InventoryReferenceCheck.Verified,
                    InventoryReferenceCheck.Verified,
                    count?.CountUnit,
                    InventoryReferenceCheck.Verified,
                    InventoryReferenceCheck.Verified,
                    limits.ToDictionary(limit => limit.CountingFamilyCode, _ => InventoryReferenceCheck.Verified)),
                policy.Version,
                Now);

        if (status == InventoryRecordStatus.Suspended)
            policy.Suspend(policy.Version, Now);

        Catalog.Inventory.Add(new InventoryConfiguration(policy, flightSources ?? [], slotSources ?? []));

        return policy;
    }

    public static ProvisionRulesArgs Rules(
        ProvisionPassengerEligibilityArgs? passengers = null,
        ProvisionSalesRestrictionsArgs? sales = null,
        ProvisionGeographyArgs? geography = null,
        ProvisionFlightApplicationArgs? flights = null,
        ProvisionFareApplicationArgs? fares = null,
        ProvisionTravelDateArgs? dates = null,
        ProvisionDayTimeApplicationArgs? dayTime = null,
        ProvisionAdvancePurchaseArgs? advance = null,
        ProvisionBaggageApplicationArgs? baggage = null,
        ProvisionSeatApplicationArgs? seats = null,
        ProvisionPetRuleArgs? pet = null,
        ProvisionAssistedTravelRuleArgs? assisted = null,
        ProvisionAirportServiceRuleArgs? airport = null)
        => new(passengers, sales, geography, flights, fares, dates, dayTime, advance, baggage, seats, pet, assisted, airport);

    public static AncillaryPricingRateArgs Rate(
        decimal amount,
        int currency = Eur,
        PassengerTypeCode? passengerType = null,
        int? ageFrom = null,
        int? ageTo = null,
        params AncillaryPriceComponentArgs[] components)
        => new(passengerType, ageFrom, ageTo, amount, currency, components);

    public static AncillaryPriceComponentArgs Tax(string code, decimal amount, TaxTreatment treatment, int currency = Eur)
        => new(AncillaryPriceLineCategory.Tax, code, null, null, null, amount, currency, null, null, treatment);

    public static AncillaryPriceComponentArgs Fee(string code, decimal amount, FeeApplicationUnit unit, int currency = Eur)
        => new(AncillaryPriceLineCategory.Fee, code, null, null, null, amount, currency, unit, null);
}
