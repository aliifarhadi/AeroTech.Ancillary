using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.V121Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V122ProfileConformanceTests
{
    private static string[] Members<TEnum>()
        where TEnum : struct, Enum
        => Enum.GetValues<TEnum>().Select(value => $"{value}={Convert.ToInt32(value)}").ToArray();

    [Fact]
    public void CT01_the_twenty_four_variant_codes_map_to_exactly_nine_closed_profiles()
    {
        var registry = AncillaryVariant.All.OrderBy(variant => variant.Code, StringComparer.Ordinal).ToList();

        Assert.Equal(Enumerable.Range(1, 24).Select(number => $"A{number:00}"), registry.Select(variant => variant.Code));
        Assert.Equal(
            new[]
            {
                "Baggage=A01,A02,A03,A04,A05,A06", "Seat=A07,A08,A09", "Upgrade=A10", "Meal=A11,A12", "Pet=A13,A14", "AssistedTravel=A15,A16,A17,A18,A19",
                "AirportService=A20,A21,A22", "Priority=A23", "Connectivity=A24"
            },
            registry.GroupBy(variant => variant.Profile).OrderBy(profile => profile.Key).Select(profile => $"{profile.Key}={string.Join(",", profile.Select(variant => variant.Code))}"));
        Assert.Null(AncillaryVariant.Find("A25"));
        Assert.Null(AncillaryVariant.Find(null));
        Assert.All(registry, variant => Assert.NotEmpty(variant.PricingUnits));
        Assert.All(registry, variant => Assert.NotEmpty(variant.ServiceDateBases));
    }

    [Fact]
    public void CT37_CT38_existing_enum_numbers_are_unchanged_and_new_values_are_appended()
    {
        Assert.Equal(new[] { "Sector=1", "Portion=2", "Journey=3", "Order=4" }, Members<ServiceCoverageScope>());
        Assert.Equal(new[] { "PerPassenger=1", "PerRoom=2", "PerItem=3", "PerVehicle=4", "PerSeat=5", "PerPiece=6", "PerKilogram=7" }, Members<PricingUnit>());
        Assert.Equal(new[] { "PreOrder=1", "PostTicketed=2", "Both=3", "LegacyUnspecified=4", "OnBoard=5" }, Members<PurchaseStage>());
        Assert.Equal(new[] { "Standard=1", "Baggage=2", "Seat=3" }, Members<ProvisionApplicationType>());
        Assert.Equal(new[] { "ExtraPiece=1", "WeightPackage=2", "Overweight=3", "Oversize=4", "SpecialEquipment=5" }, Members<BaggageChargeKind>());
        Assert.Equal(new[] { "None=1", "EmdAssociated=2", "EmdStandalone=3" }, Members<AncillaryDocumentType>());
        Assert.Equal(new[] { "Unlimited=1", "Local=2", "Supplier=3", "FlightFlow=4" }, Members<InventoryAuthority>());
        Assert.Equal(new[] { "PerOrder=1", "PerFlightOccurrence=2", "PerServiceDate=3", "PerPortion=4" }, Members<PassengerUsageLimitScope>());
        Assert.Equal(new[] { "FlightDeparture=1", "ServiceStart=2", "CheckIn=3", "CoverageStart=4", "Activation=5" }, Members<ServiceDateBasis>());
        Assert.Equal(
            new[] { "Baggage=1", "Seat=2", "Upgrade=3", "Meal=4", "Pet=5", "AssistedTravel=6", "AirportService=7", "Priority=8", "Connectivity=9" },
            Members<AncillaryProfile>());
        Assert.Equal(new[] { "Filed=1", "ExternalQuote=2", "Free=3", "NotAvailable=4", "LegacyUnspecified=5" }, Members<PriceOrigin>());
        Assert.Equal(new[] { "AddedToBase=1", "IncludedInBase=2", "LegacyUnknown=3" }, Members<TaxTreatment>());
        Assert.Equal(new[] { "SimpleOptIn=1", "QuantityChoice=2", "TypedForm=3", "SeatMapSelection=4", "ExternalQuote=5" }, Members<SelectionKind>());
        Assert.Equal(new[] { "PurchasedUnit=1", "Kilogram=2" }, Members<UsageConsumptionUnit>());
        Assert.Equal(new[] { "NoAncillaryDocument=1", "Emd=2", "TicketOrExchange=3" }, Members<DocumentRouting>());
    }

    [Fact]
    public void CT02_CT05_a_definition_holds_exactly_one_matching_specification_and_a_later_version_never_changes_its_variant()
    {
        static AncillaryServiceDefinition Define(ServiceDefinitionProfileArgs profile)
            => AncillaryServiceDefinition.Define(
                1001,
                Airline,
                2001,
                "PRIORITY_BOARDING",
                1,
                "PRB",
                ServiceSubCodeSource.CarrierDefined,
                new ServiceDefinitionClassificationArgs("F", "TS", null, null, null),
                profile,
                PricingUnit.PerPassenger,
                ServiceDateBasis.FlightDeparture,
                "Priority boarding",
                null,
                DocumentDefinition.Create(AncillaryDocumentType.EmdAssociated, "G", "PRB"),
                BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
                null,
                null,
                Now);

        var own = V122Fixtures.Profile(AncillaryVariant.PriorityBoardingCheckin);
        var foreign = V122Fixtures.Specification(AncillaryVariant.OnboardWifi);

        BusinessAssert.Throws(16216, 422, () => Define(own with { Specification = own.Specification with { Connectivity = foreign.Connectivity } }));
        BusinessAssert.Throws(16216, 422, () => Define(own with { Specification = foreign }));
        BusinessAssert.Throws(16216, 422, () => Define(own with { Specification = new ServiceSpecificationArgs() }));
        BusinessAssert.Throws(16202, 422, () => Define(own with { VariantCode = AncillaryVariant.OnboardWifi }));
        BusinessAssert.Throws(16202, 422, () => Define(own with { VariantCode = "A99" }));

        var definition = Define(own);

        Assert.Equal((AncillaryProfile.Priority, AncillaryVariant.PriorityBoardingCheckin, true), (definition.Profile, definition.VariantCode, definition.IsClassified));
        Assert.Equal(
            new object?[] { null, null, null, null, null, null, null, null },
            new object?[] { definition.Baggage, definition.Seat, definition.Upgrade, definition.Meal, definition.Pet, definition.AssistedTravel, definition.AirportService, definition.Connectivity });
        Assert.NotNull(definition.Priority);

        definition.Activate(Supplier(), Now);

        var revision = definition.Revise(1002, 2, Now.AddDays(1));

        Assert.Equal((definition.Profile, definition.VariantCode, 2), (revision.Profile, revision.VariantCode, revision.Version));
        Assert.NotSame(definition.Priority, revision.Priority);
        Assert.Equal(definition.Priority!.PriorityZoneCode, revision.Priority!.PriorityZoneCode);
        BusinessAssert.Throws(16217, 409, () => revision.Change(
            revision.SupplierId,
            revision.ServiceSubCode,
            revision.SubCodeSource,
            new ServiceDefinitionClassificationArgs(revision.ServiceTypeCode, revision.GroupCode, revision.SubGroupCode, revision.Description1Code, revision.Description2Code),
            V122Fixtures.Profile(AncillaryVariant.FreeSpecialMeal),
            revision.PricingUnit!.Value,
            revision.ServiceDateBasis!.Value,
            revision.CommercialName,
            revision.Description,
            DocumentDefinition.Create(revision.Document.Type, revision.Document.Rfic, revision.Document.Rfisc),
            BookingDefinition.Create(revision.Booking.Method, revision.Booking.SsrCode, revision.Booking.SsimCode),
            revision.SalesEffectiveFrom,
            revision.SalesDiscontinueOn));
        Assert.Equal(AncillaryVariant.PriorityBoardingCheckin, revision.VariantCode);
    }

    [Fact]
    public void CT03_a_rule_of_another_family_is_refused_at_publication()
    {
        var petRule = Provision(Groups() with { PetRule = new ProvisionPetRuleArgs(null, null, null, ConfirmationRequirement.SubjectToConfirmation) }, disposition: CommercialDisposition.Free);
        var medical = Provision(Groups() with { AssistedTravelRule = new ProvisionAssistedTravelRuleArgs(null, null, true) }, disposition: CommercialDisposition.Free);
        var airport = Provision(Groups() with { AirportServiceRule = new ProvisionAirportServiceRuleArgs("T1", null, null, null, null, null) }, disposition: CommercialDisposition.Free);
        var baggage = Provision(applicationType: ProvisionApplicationType.Baggage, disposition: CommercialDisposition.Free);

        Assert.All(new[] { petRule, medical, airport, baggage }, provision => BusinessAssert.Throws(16321, 409, () => provision.Activate(CarrierDefinition(), null, Now)));
        Assert.All(new[] { petRule, medical, airport, baggage }, provision => Assert.Equal(ProvisionStatus.Draft, provision.Status));
    }

    [Fact]
    public void CT07_CT08_a_published_provision_has_one_point_of_sale_and_an_accepted_quantity_of_at_least_one()
    {
        var two = Provision(Groups(salesRestrictions: Sales(pointsOfSale: [V122Fixtures.PointOfSale, 9002])), disposition: CommercialDisposition.Free);
        var one = Provision(disposition: CommercialDisposition.Free);

        BusinessAssert.Throws(16319, 409, () => two.Activate(CarrierDefinition(), null, Now));
        one.Activate(CarrierDefinition(), null, Now);

        Assert.Equal((ProvisionStatus.Draft, ProvisionStatus.Active), (two.Status, one.Status));
        BusinessAssert.Throws(16302, 422, () => QuantityRule.Create(AncillaryQuantityUnit.Piece, 0, 6));
        Assert.Equal((1, 6), (QuantityRule.Create(AncillaryQuantityUnit.Piece, 1, 6).MinQuantity, QuantityRule.Create(AncillaryQuantityUnit.Piece, 1, 6).MaxQuantity));
    }

    [Fact]
    public void CT18_PR13_PR14_the_price_origin_follows_the_outcome_and_an_external_quote_needs_its_recorded_authority()
    {
        var definition = CarrierDefinition(variant: null);

        AncillaryProvision Origin(CommercialDisposition disposition, PriceOrigin origin, string? key)
            => AncillaryProvision.Define(
                5001,
                1001,
                10,
                ServiceCoverageScope.Sector,
                PurchaseStage.Both,
                origin,
                key,
                QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
                ProvisionApplicationType.Standard,
                CommercialOutcome.Create(disposition, false, false),
                SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
                AvailabilityDefinition.Create(false),
                FulfillmentDefinition.Create("Ancillary"),
                Rules(null, ProvisionApplicationType.Standard),
                new SequentialIdGenerator(),
                Now);

        BusinessAssert.Throws(16320, 422, () => Origin(CommercialDisposition.Paid, PriceOrigin.Free, null));
        BusinessAssert.Throws(16320, 422, () => Origin(CommercialDisposition.Free, PriceOrigin.Filed, null));
        BusinessAssert.Throws(16320, 422, () => Origin(CommercialDisposition.NotAvailable, PriceOrigin.ExternalQuote, "QuotePartnerA"));
        BusinessAssert.Throws(16320, 422, () => Origin(CommercialDisposition.Paid, PriceOrigin.LegacyUnspecified, null));
        BusinessAssert.Throws(16302, 422, () => Origin(CommercialDisposition.Paid, PriceOrigin.ExternalQuote, null));
        BusinessAssert.Throws(16302, 422, () => Origin(CommercialDisposition.Paid, PriceOrigin.Filed, "QuotePartnerA"));

        var quoted = Origin(CommercialDisposition.Paid, PriceOrigin.ExternalQuote, "QuotePartnerA");

        BusinessAssert.Throws(16322, 409, () => quoted.Activate(definition, null, Now));
        BusinessAssert.Throws(16322, 409, () => quoted.Activate(definition, "OtherPartner", Now));
        quoted.Activate(definition, "QuotePartnerA", Now);

        Assert.Equal((ProvisionStatus.Active, PriceOrigin.ExternalQuote, "QuotePartnerA"), (quoted.Status, quoted.PriceOrigin, quoted.QuoteProviderKey));

        var retired = M1Fixtures.ExternalSupplier();

        retired.Retire(Now);

        Assert.Equal("LoungePartnerA", M1Fixtures.ExternalSupplier().QuoteAuthorityKey());
        Assert.Null(M1Fixtures.LocalSupplier().QuoteAuthorityKey());
        Assert.Null(retired.QuoteAuthorityKey());
    }

    [Fact]
    public void CT21_PR05_PR06_PR15_an_included_tax_is_reported_and_never_added_twice_and_a_tax_without_a_treatment_is_refused()
    {
        AncillaryPriceComponentArgs Tax(string code, decimal amount, TaxTreatment? treatment)
            => new(AncillaryPriceLineCategory.Tax, code, null, null, null, amount, Eur, null, null, treatment);

        AncillaryPricing Filed(params AncillaryPriceComponentArgs[] components)
            => AncillaryPricing.Define(7001, 5001, PricingUnit.PerItem, 1, [new AncillaryPricingRateArgs(null, null, null, 100m, Eur, components)], FiledPrice.Scales, new SequentialIdGenerator(), Now);

        var rate = Filed(
            Tax("VAT", 9m, TaxTreatment.AddedToBase),
            Tax("INC", 5m, TaxTreatment.IncludedInBase),
            new AncillaryPriceComponentArgs(AncillaryPriceLineCategory.Fee, "TKT", null, null, null, 7m, Eur, FeeApplicationUnit.Ticket, null)).Rates.Single();

        Assert.Equal((109m, 5m, false, "TKT"), (rate.UnitTotal.Amount, rate.IncludedTaxes.Amount, rate.IsUnitTotalComplete, rate.UnappliedFees.Single().Code));
        BusinessAssert.Throws(16516, 422, () => Filed(Tax("VAT", 9m, null)));
        BusinessAssert.Throws(16516, 422, () => Filed(Tax("VAT", 9m, TaxTreatment.LegacyUnknown)));
        BusinessAssert.Throws(16518, 422, () => Filed(Tax("INC", 101m, TaxTreatment.IncludedInBase)));
        BusinessAssert.Throws(16502, 422, () => Filed(new AncillaryPriceComponentArgs(AncillaryPriceLineCategory.Fee, "SVC", null, null, null, 1m, Eur, FeeApplicationUnit.Item, null, TaxTreatment.AddedToBase)));
    }

    [Fact]
    public void CT11_a_counting_family_never_mixes_purchased_units_with_kilograms()
    {
        var kilograms = new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerPortion, 20, "XBAG_WEIGHT", UsageConsumptionUnit.Kilogram, 10m);
        var units = new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerPortion, 2, "XBAG_WEIGHT");
        var verified = ("XBAG_WEIGHT", InventoryReferenceCheck.Verified);

        BusinessAssert.Throws(16602, 422, () => P2Fixtures.Policy(P2Fixtures.Args(InventoryAuthority.Unlimited, limits: kilograms with { UnitsPerPurchase = null })));
        BusinessAssert.Throws(16602, 422, () => P2Fixtures.Policy(P2Fixtures.Args(InventoryAuthority.Unlimited, limits: units with { UnitsPerPurchase = 10m })));

        var ten = P2Fixtures.Policy(P2Fixtures.Args(InventoryAuthority.Unlimited, limits: kilograms));
        var five = P2Fixtures.Policy(P2Fixtures.Args(InventoryAuthority.Unlimited, limits: kilograms with { UnitsPerPurchase = 5m }), "XBAG_WEIGHT_5KG", 6002);
        var mixed = P2Fixtures.Policy(P2Fixtures.Args(InventoryAuthority.Unlimited, limits: units), "XBAG_PIECE", 6003);

        ten.Activate(P2Fixtures.Evidence(families: verified), 1, P2Fixtures.Now);
        five.Activate(
            P2Fixtures.Evidence(families: verified) with { CountingFamilyUnits = new Dictionary<string, IReadOnlyList<UsageConsumptionUnit>> { ["XBAG_WEIGHT"] = [UsageConsumptionUnit.Kilogram] } },
            1,
            P2Fixtures.Now);
        BusinessAssert.Throws(16606, 409, () => mixed.Activate(
            P2Fixtures.Evidence(families: verified) with { CountingFamilyUnits = new Dictionary<string, IReadOnlyList<UsageConsumptionUnit>> { ["XBAG_WEIGHT"] = [UsageConsumptionUnit.Kilogram] } },
            1,
            P2Fixtures.Now));

        var limit = ten.PassengerUsageLimits.Single();
        var key = limit.KeyFor(new PassengerUsageSubject("PAX-1", null, null, null, null, "OUTBOUND"));

        Assert.Equal((UsageConsumptionUnit.Kilogram, 10m, 20), (limit.ConsumptionUnit, limit.UnitsPerPurchase, limit.MaxUnits));
        Assert.Equal(("XBAG_WEIGHT", PassengerUsageLimitScope.PerPortion, "PAX-1", "OUTBOUND"), (key.CountingFamilyCode, key.LimitScope, key.StableTravellerIdentity, key.PortionRef));
        BusinessAssert.Throws(16602, 422, () => limit.KeyFor(new PassengerUsageSubject("PAX-1", null, null, 81234, null, null)));
    }
}
