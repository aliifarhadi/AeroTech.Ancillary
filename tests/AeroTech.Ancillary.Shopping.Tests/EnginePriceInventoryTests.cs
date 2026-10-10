using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class EnginePriceInventoryTests
{
    private static async Task<(ShoppingLab Lab, CanonicalAncillaryOfferCandidate Candidate)> PricedAsync(
        string code,
        Domain.AncillaryPricingAggregate.Arguments.AncillaryPricingRateArgs[] rates,
        AncillaryShoppingContext? context = null,
        Action<ShoppingLab>? beforeShopping = null)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition), rates);
        beforeShopping?.Invoke(lab);

        var result = await lab.Engine.ShopAsync(context ?? Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });

        return (lab, result.Candidates[0]);
    }

    [Fact]
    public async Task PRICE_01_added_tax_is_added_and_included_tax_is_only_reported()
    {
        var (_, candidate) = await PricedAsync("A24", [Rate(100m, components: [Tax("VAT", 9m, TaxTreatment.AddedToBase), Tax("INC", 5m, TaxTreatment.IncludedInBase)])]);
        var price = candidate.Price;

        Assert.Equal((PriceOrigin.Filed, PriceAssessmentStatus.Complete, Eur, 100m, 109m), (price.Origin, price.Status, price.CurrencyId, price.BaseAmount, price.CompleteUnitTotal));
        Assert.Equal(("VAT", 9m, TaxTreatment.AddedToBase), (price.AddedTaxLines.Single().Code, price.AddedTaxLines.Single().Amount, price.AddedTaxLines.Single().TaxTreatment));
        Assert.Equal(("INC", 5m, TaxTreatment.IncludedInBase), (price.IncludedTaxLines.Single().Code, price.IncludedTaxLines.Single().Amount, price.IncludedTaxLines.Single().TaxTreatment));
        Assert.Empty(price.AppliedUnitFeeLines);
        Assert.Empty(price.UnappliedFeeLines);
        Assert.True(price.IsOrderLevelTotalComplete);
        Assert.Equal(PricingUnit.PerItem, price.PricingUnit);
        Assert.NotNull(price.PricingRevisionId);
        Assert.NotNull(price.PricingRateId);
    }

    [Fact]
    public async Task PRICE_02_a_ticket_fee_stays_unapplied_and_is_never_multiplied_by_travellers_or_flights()
    {
        var lab = new ShoppingLab();
        var meal = lab.Definition("A12");

        lab.Price(
            meal,
            lab.Provision(meal),
            Rate(100m, components: [Tax("VAT", 9m, TaxTreatment.AddedToBase), Fee("ITM", 2m, FeeApplicationUnit.Item), Fee("TKT", 7m, FeeApplicationUnit.Ticket)]));

        var first = Trip.Flight("F1", 101, Trip.Thr, Trip.Ika);
        var second = Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, first.DepartureAt.AddHours(5));
        var third = Trip.Flight("F3", 103, Trip.Ist, Trip.Mhd, first.DepartureAt.AddDays(5), Trip.Istanbul);
        var context = Trip.Context(
            [first, second, third],
            [Trip.Portion("P1", 1, first, second), Trip.Portion("P2", 2, third)],
            [Trip.Adult("T1"), Trip.Adult("T2")]);
        var result = await lab.Engine.ShopAsync(context);

        Assert.Equal(6, result.Candidates.Count);
        Assert.All(result.Candidates, candidate =>
        {
            Assert.Equal((PriceAssessmentStatus.Complete, 111m, false), (candidate.Price.Status, candidate.Price.CompleteUnitTotal, candidate.Price.IsOrderLevelTotalComplete));
            Assert.Equal(("ITM", 2m), (candidate.Price.AppliedUnitFeeLines.Single().Code, candidate.Price.AppliedUnitFeeLines.Single().Amount));
            Assert.Equal(("TKT", 7m, FeeApplicationUnit.Ticket), (candidate.Price.UnappliedFeeLines.Single().Code, candidate.Price.UnappliedFeeLines.Single().Amount, candidate.Price.UnappliedFeeLines.Single().FeeApplicationUnit));
            Assert.Contains(ShoppingReasonCodes.FeeNotAppliedAtUnitLevel, candidate.Price.ReasonCodes);
            Assert.Null(candidate.Price.RequestedQuantityTotal);
        });

        var two = await lab.Engine.EvaluateSelectionAsync(context, result.Candidates[0].CandidateIdentity, new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = 2 });

        Assert.Equal((SelectionEvaluationStatus.Accepted, 222m, 111m, 7m), (two.Status, two.Candidate.Price.RequestedQuantityTotal, two.Candidate.Price.CompleteUnitTotal, two.Candidate.Price.UnappliedFeeLines.Single().Amount));
    }

    [Theory]
    [InlineData(FeeApplicationUnit.PerOneKilogramOver)]
    [InlineData(FeeApplicationUnit.PerFiveKilogramsOver)]
    [InlineData(FeeApplicationUnit.HalfPercentOfFarePerKilogram)]
    [InlineData(FeeApplicationUnit.OnePercentOfFarePerKilogram)]
    [InlineData(FeeApplicationUnit.OneAndHalfPercentOfFarePerKilogram)]
    public async Task PRICE_03_a_proportional_fee_unit_is_never_read_as_an_item_fee(FeeApplicationUnit unit)
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");
        var provision = lab.Provision(wifi);

        try
        {
            lab.Price(wifi, provision, Rate(100m, components: [Fee("OVR", 3m, unit)]));
        }
        catch (BusinessException refusal)
        {
            Assert.Equal(16305, refusal.Code);
            Assert.Empty((await lab.Engine.ShopAsync(Trip.Context())).Candidates);

            return;
        }

        var candidate = (await lab.Engine.ShopAsync(Trip.Context())).One();

        Assert.Equal((PriceAssessmentStatus.Incomplete, OfferReadiness.NeedsVerification), (candidate.Price.Status, candidate.OfferReadiness));
        Assert.Null(candidate.Price.CompleteUnitTotal);
        Assert.Empty(candidate.Price.AppliedUnitFeeLines);
        Assert.Contains(ShoppingReasonCodes.FeeUnitNotSupported, candidate.Price.ReasonCodes);
    }

    [Fact]
    public async Task PRICE_04_the_amount_scale_comes_from_the_currency_reference_and_an_unknown_or_wrong_scale_blocks_the_price()
    {
        var rates = new[] { Rate(100.25m, Eur), Rate(1500m, Jpy), Rate(12.345m, Kwd) };

        foreach (var (currency, amount) in new[] { (Eur, 100.25m), (Jpy, 1500m), (Kwd, 12.345m) })
        {
            var (_, candidate) = await PricedAsync("A24", rates, Trip.Context(currencyId: currency));

            Assert.Equal((PriceAssessmentStatus.Complete, currency, amount), (candidate.Price.Status, candidate.Price.CurrencyId, candidate.Price.CompleteUnitTotal));
        }

        var (_, unknown) = await PricedAsync("A24", rates, Trip.Context(currencyId: Kwd), lab => lab.Catalog.CurrencyScales.Remove(Kwd));
        var (_, wrong) = await PricedAsync("A24", rates, Trip.Context(currencyId: Kwd), lab => lab.Catalog.CurrencyScales[Kwd] = 2);
        var (_, zero) = await PricedAsync("A24", rates, Trip.Context(currencyId: Eur), lab => lab.Catalog.CurrencyScales[Eur] = 0);

        Assert.Equal((PriceAssessmentStatus.Incomplete, OfferReadiness.NeedsVerification), (unknown.Price.Status, unknown.OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.CurrencyScaleUnknown, unknown.Price.ReasonCodes);
        Assert.Null(unknown.Price.CompleteUnitTotal);
        Assert.Contains(ShoppingReasonCodes.AmountScaleNotAllowed, wrong.Price.ReasonCodes);
        Assert.Null(wrong.Price.CompleteUnitTotal);
        Assert.Contains(ShoppingReasonCodes.AmountScaleNotAllowed, zero.Price.ReasonCodes);

        var (_, other) = await PricedAsync("A24", [Rate(100m, Eur)], Trip.Context(currencyId: Usd));

        Assert.Equal((PriceAssessmentStatus.Unavailable, OfferReadiness.Unavailable), (other.Price.Status, other.OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.RateNotFound, other.Price.ReasonCodes);
        Assert.Null(other.Price.CompleteUnitTotal);
    }

    [Fact]
    public async Task PRICE_05_filed_without_price_is_not_sold_free_has_no_money_lines_and_a_quoted_price_has_no_amount()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");
        var wheelchair = lab.Definition("A15");
        var upgrade = lab.Definition("A10");

        lab.Provision(wifi);
        lab.Provision(wheelchair);
        lab.Provision(upgrade);

        var ordinary = await lab.Engine.ShopAsync(Trip.Context());
        var all = await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });
        var unpriced = all.One(wifi.ServiceDefinitionRef);
        var free = all.One(wheelchair.ServiceDefinitionRef);
        var quoted = all.One(upgrade.ServiceDefinitionRef);

        Assert.DoesNotContain(ordinary.Candidates, candidate => candidate.ServiceDefinitionRef == wifi.ServiceDefinitionRef);
        Assert.Equal((PriceAssessmentStatus.Unavailable, OfferReadiness.Unavailable), (unpriced.Price.Status, unpriced.OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.PricingNotActive, unpriced.Price.ReasonCodes);
        Assert.Equal((PriceOrigin.Free, PriceAssessmentStatus.Complete), (free.Price.Origin, free.Price.Status));
        Assert.Null(free.Price.BaseAmount);
        Assert.Null(free.Price.CompleteUnitTotal);
        Assert.Null(free.Price.CurrencyId);
        Assert.Null(free.Price.PricingRevisionId);
        Assert.Empty(free.Price.AddedTaxLines.Concat(free.Price.IncludedTaxLines).Concat(free.Price.AppliedUnitFeeLines).Concat(free.Price.UnappliedFeeLines));
        Assert.Equal((PriceOrigin.ExternalQuote, PriceAssessmentStatus.QuoteRequired, OfferReadiness.NeedsQuote), (quoted.Price.Origin, quoted.Price.Status, quoted.OfferReadiness));
        Assert.Equal(Quote, quoted.Price.QuoteProviderKey);
        Assert.Null(quoted.Price.BaseAmount);
        Assert.Null(quoted.Price.CompleteUnitTotal);
        Assert.Null(quoted.Price.ExternalQuoteRef);
        Assert.Null(quoted.Price.QuoteExpiresAtUtc);
    }

    [Fact]
    public async Task PRICE_06_one_rate_is_chosen_by_currency_passenger_type_and_age_and_an_ambiguous_choice_is_refused()
    {
        var lab = new ShoppingLab();
        var meal = lab.Definition("A12", pricingUnit: PricingUnit.PerPassenger);

        lab.Price(
            meal,
            lab.Provision(meal),
            Rate(30m, passengerType: AeroTech.Messages.AirPrice.Enums.PassengerTypeCode.ADT),
            Rate(20m, passengerType: AeroTech.Messages.AirPrice.Enums.PassengerTypeCode.CHD, ageFrom: 6, ageTo: 12),
            Rate(10m, passengerType: AeroTech.Messages.AirPrice.Enums.PassengerTypeCode.CHD, ageFrom: 2, ageTo: 6));

        async Task<PriceAssessment> ForAsync(ShoppingTraveller traveller)
            => (await lab.Engine.ShopAsync(Trip.Context(travellers: [traveller]), new ShoppingFilter { IncludeNonSellable = true })).One().Price;

        Assert.Equal(30m, (await ForAsync(Trip.Adult())).CompleteUnitTotal);
        Assert.Equal(20m, (await ForAsync(Trip.Child("T1"))).CompleteUnitTotal);
        Assert.Equal(10m, (await ForAsync(Trip.Child("T1", new DateOnly(2022, 1, 1)))).CompleteUnitTotal);
        Assert.Equal(20m, (await ForAsync(Trip.Child("T1", new DateOnly(2020, 12, 1)))).CompleteUnitTotal);

        var infant = await ForAsync(Trip.Child("T1", new DateOnly(2025, 6, 1)));
        var unknownAge = await ForAsync(Trip.Child("T1") with { DateOfBirth = null });

        Assert.Equal(PriceAssessmentStatus.Unavailable, infant.Status);
        Assert.Contains(ShoppingReasonCodes.RateNotFound, infant.ReasonCodes);
        Assert.Equal(PriceAssessmentStatus.Incomplete, unknownAge.Status);
        Assert.Contains(ShoppingReasonCodes.AgeNotVerified, unknownAge.ReasonCodes);
        Assert.Null(unknownAge.CompleteUnitTotal);

        var doubled = new ShoppingLab();
        var wifi = doubled.Definition("A24");
        var provision = doubled.Provision(wifi);

        doubled.Price(wifi, provision, Rate(10m));
        doubled.Price(wifi, provision, Rate(12m));

        var ambiguous = (await doubled.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true })).One();

        Assert.Equal((PriceAssessmentStatus.Unavailable, OfferReadiness.Unavailable), (ambiguous.Price.Status, ambiguous.OfferReadiness));
        Assert.Null(ambiguous.Price.CompleteUnitTotal);
    }

    [Fact]
    public async Task PRICE_07_document_and_booking_metadata_come_from_the_definition_and_no_refund_amount_is_computed()
    {
        var lab = new ShoppingLab();

        foreach (var code in new[] { "A01", "A10", "A15", "A20" })
            lab.Sellable(code);

        var result = await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });

        foreach (var candidate in result.Candidates)
        {
            var definition = lab.Catalog.Definitions.Single(row => row.Id == candidate.DefinitionVersionId);
            var provision = lab.Catalog.Provisions.Single(row => row.Id == candidate.ProvisionId);

            Assert.Equal(
                (definition.DocumentRouting, definition.Document.Type, definition.Document.Rfic, definition.Document.Rfisc, provision.Outcome.DocumentRequired),
                (candidate.Document.Routing, candidate.Document.DocumentType, candidate.Document.Rfic, candidate.Document.Rfisc, candidate.Document.DocumentRequired));
            Assert.Equal((definition.Booking.Method, definition.Booking.SsrCode), (candidate.Booking.Method, candidate.Booking.SsrCode));
            Assert.Equal(ProviderConfirmationStatus.NotRequested, candidate.Confirmation.ProviderConfirmationStatus);
        }

        Assert.Equal(DocumentRouting.TicketOrExchange, result.Candidates.Single(candidate => candidate.VariantCode == "A10").Document.Routing);
        Assert.Equal(DocumentRouting.NoAncillaryDocument, result.Candidates.Single(candidate => candidate.VariantCode == "A15").Document.Routing);
        Assert.DoesNotContain(
            typeof(PriceAssessment).GetProperties().Concat(typeof(CanonicalAncillaryOfferCandidate).GetProperties()),
            property => property.Name.Contains("Refund", StringComparison.Ordinal) || property.Name.Contains("Penalty", StringComparison.Ordinal));
    }

    [Fact]
    public async Task INVENTORY_01_no_policy_is_not_configured_and_unlimited_is_never_guaranteed()
    {
        var lab = new ShoppingLab();
        var (bare, _) = lab.Sellable("A23");
        var unlimited = lab.Definition("A11");

        lab.Provision(unlimited);
        lab.Policy(unlimited, InventoryAuthority.Unlimited);

        var result = await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });
        var none = result.One(bare.ServiceDefinitionRef).Availability;
        var open = result.One(unlimited.ServiceDefinitionRef).Availability;

        Assert.Equal((null, InventoryCapacityReadState.NotConfigured, AvailabilityCheckState.NotChecked, false), (none.InventoryAuthority, none.ConfigState, none.CheckedState, none.IsGuaranteed));
        Assert.Contains(ShoppingReasonCodes.InventoryNotConfigured, none.ReasonCodes);
        Assert.Equal(
            (InventoryAuthority.Unlimited, InventoryCapacityReadState.Unlimited, AvailabilityCheckState.NotChecked, false, false),
            (open.InventoryAuthority, open.ConfigState, open.CheckedState, open.IsGuaranteed, open.RequiresCheckAtReserve));
        Assert.Null(open.ConsumptionPreview);
        Assert.Null(open.AvailabilityAsOfUtc);

        var mustCheck = new ShoppingLab();
        var meal = mustCheck.Definition("A11");

        mustCheck.Provision(meal, mustCheck: true);

        var unchecked_ = (await mustCheck.Engine.ShopAsync(Trip.Context())).One();

        Assert.True(unchecked_.Availability.RequiresCheckAtReserve);
        Assert.Contains(ShoppingReasonCodes.AvailabilityMustBeChecked, unchecked_.ReasonCodes);
    }

    [Fact]
    public async Task INVENTORY_02_supplier_and_flightflow_authority_are_delegated_checks_without_any_local_bucket()
    {
        var lab = new ShoppingLab();
        var pet = lab.Definition("A13");
        var seat = lab.Definition("A07");

        lab.Price(pet, lab.Provision(pet), Rate(80m));
        lab.Price(seat, lab.Provision(seat), Rate(15m));
        lab.Policy(pet, InventoryAuthority.Supplier);
        lab.Policy(seat, InventoryAuthority.FlightFlow);

        var result = await lab.Engine.ShopAsync(Trip.Context());
        var supplier = result.One(pet.ServiceDefinitionRef).Availability;
        var flightFlow = result.One(seat.ServiceDefinitionRef).Availability;

        Assert.Equal(
            (InventoryAuthority.Supplier, InventoryCapacityReadState.DelegatedCheckRequired, AvailabilityCheckState.NotChecked, false, true),
            (supplier.InventoryAuthority, supplier.ConfigState, supplier.CheckedState, supplier.IsGuaranteed, supplier.RequiresCheckAtReserve));
        Assert.Equal(
            (InventoryAuthority.FlightFlow, InventoryCapacityReadState.DelegatedCheckRequired, AvailabilityCheckState.SourceUnavailable, false, true),
            (flightFlow.InventoryAuthority, flightFlow.ConfigState, flightFlow.CheckedState, flightFlow.IsGuaranteed, flightFlow.RequiresCheckAtReserve));
        Assert.Contains(ShoppingReasonCodes.BlockedExternalReference, flightFlow.ReasonCodes);
        Assert.Null(supplier.ConsumptionPreview);
        Assert.Null(flightFlow.ConsumptionPreview);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic is { Code: ShoppingReasonCodes.BlockedExternalReference, EvidenceKind: ShoppingEvidenceKind.Blocked });

        lab.Catalog.FlightFlowAnswer = InventoryReferenceCheck.Verified;

        var connected = (await lab.Engine.ShopAsync(Trip.Context())).One(seat.ServiceDefinitionRef).Availability;

        Assert.Equal((InventoryCapacityReadState.DelegatedCheckRequired, AvailabilityCheckState.NotChecked, false), (connected.ConfigState, connected.CheckedState, connected.IsGuaranteed));
    }

    [Fact]
    public async Task INVENTORY_03_local_capacity_is_configured_not_guaranteed_closed_or_blocked_by_its_missing_reference()
    {
        const long resource = 990001;

        async Task<CanonicalAncillaryOfferCandidate> WithAsync(InventoryReferenceCheck reference, params FlightInventorySource[] sources)
        {
            var lab = new ShoppingLab { Catalog = { ResourceAnswer = reference } };
            var pet = lab.Definition("A13");

            lab.Price(pet, lab.Provision(pet, scope: ServiceCoverageScope.Sector), Rate(80m));
            lab.Policy(
                pet,
                InventoryAuthority.Local,
                LocalInventoryPattern.FlightCount,
                FlightCountConsumption.Create(resource, 1, InventoryCountUnit.AnimalCarrier),
                flightSources: sources);

            return (await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true })).One();
        }

        var open = new FlightInventorySource(InventoryResourceKind.FlightCount, 101, resource, InventoryRecordStatus.Active, false);
        var configured = await WithAsync(InventoryReferenceCheck.Verified, open);
        var blocked = await WithAsync(InventoryReferenceCheck.SourceUnavailable, open);
        var closed = await WithAsync(InventoryReferenceCheck.Verified, open with { ClosedForSale = true });
        var missing = await WithAsync(InventoryReferenceCheck.Verified);
        var otherFlight = await WithAsync(InventoryReferenceCheck.Verified, open with { FlightId = 202 });
        var unknownResource = await WithAsync(InventoryReferenceCheck.NotFound, open);

        Assert.Equal(
            (InventoryAuthority.Local, InventoryCapacityReadState.ConfiguredNotGuaranteed, AvailabilityCheckState.NotChecked, false, true),
            (configured.Availability.InventoryAuthority, configured.Availability.ConfigState, configured.Availability.CheckedState, configured.Availability.IsGuaranteed, configured.Availability.RequiresCheckAtReserve));
        Assert.Equal((1, InventoryCountUnit.AnimalCarrier), (configured.Availability.ConsumptionPreview!.CountUnits, configured.Availability.ConsumptionPreview.CountUnit));
        Assert.Equal((InventoryCapacityReadState.ConfiguredNotGuaranteed, AvailabilityCheckState.SourceUnavailable), (blocked.Availability.ConfigState, blocked.Availability.CheckedState));
        Assert.Contains(ShoppingReasonCodes.BlockedExternalReference, blocked.ReasonCodes);
        Assert.Equal((InventoryCapacityReadState.ClosedForSale, OfferReadiness.Unavailable), (closed.Availability.ConfigState, closed.OfferReadiness));
        Assert.Equal(InventoryCapacityReadState.NotConfigured, missing.Availability.ConfigState);
        Assert.Contains(ShoppingReasonCodes.InventorySourceNotConfigured, missing.ReasonCodes);
        Assert.Equal(InventoryCapacityReadState.NotConfigured, otherFlight.Availability.ConfigState);
        Assert.Equal(AvailabilityCheckState.Unknown, unknownResource.Availability.CheckedState);
        Assert.Contains(ShoppingReasonCodes.InventoryReferenceNotFound, unknownResource.ReasonCodes);
        Assert.All(new[] { configured, blocked, closed, missing, otherFlight, unknownResource }, candidate => Assert.False(candidate.Availability.IsGuaranteed));
    }

    [Fact]
    public async Task INVENTORY_04_a_ten_kilogram_package_is_one_priced_item_and_remaining_entitlement_needs_complete_evidence()
    {
        var lab = new ShoppingLab { Catalog = { FamilyAnswer = InventoryReferenceCheck.Verified } };
        var package = lab.Definition("A02");
        var limit = new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerPortion, 30, "XBAG_KG", UsageConsumptionUnit.Kilogram, 10m);

        lab.Price(package, lab.Provision(package), Rate(35m));

        var policy = lab.Policy(package, InventoryAuthority.Unlimited, limits: limit);
        var key = policy.PassengerUsageLimits.Single().KeyFor(new PassengerUsageSubject("PAX-T1", null, null, null, null, "P1"));
        var shown = (await lab.Engine.ShopAsync(Trip.Context())).One();
        var cumulative = shown.Quantity.CumulativeLimits.Single();

        Assert.Equal((AncillaryQuantityUnit.Each, 1, 2, true), (shown.Quantity.Unit, shown.Quantity.MinPerSelection, shown.Quantity.MaxPerSelection, shown.Quantity.ZeroIsDeselect));
        Assert.Equal(("XBAG_KG", PassengerUsageLimitScope.PerPortion, 30, UsageConsumptionUnit.Kilogram, 10m, true), (cumulative.CountingFamilyCode, cumulative.LimitScope, cumulative.MaxUnits, cumulative.ConsumptionUnit, cumulative.UnitsPerPurchase, cumulative.FamilyVerified));
        Assert.Null(cumulative.VerifiedConsumed);
        Assert.Null(cumulative.VerifiedRemaining);
        Assert.Contains(ShoppingReasonCodes.UsageNotVerified, shown.ReasonCodes);
        Assert.Equal(35m, shown.Price.CompleteUnitTotal);

        VerifiedUsageEvidence Evidence(decimal consumed, bool complete = true)
            => new() { UsageSubjectKey = key, UnitsConsumed = consumed, IsCompleteForScope = complete, AsOfUtc = Now, SourceAuthority = "TEST-LEDGER" };

        var context = Trip.Context() with { UsageEvidence = [Evidence(20m)] };
        var one = await lab.Engine.EvaluateSelectionAsync(context, shown.CandidateIdentity, new AncillarySelection { PackageProductRef = "PACK10", Quantity = 1 });
        var two = await lab.Engine.EvaluateSelectionAsync(context, shown.CandidateIdentity, new AncillarySelection { PackageProductRef = "PACK10", Quantity = 2 });
        var incomplete = await lab.Engine.EvaluateSelectionAsync(
            Trip.Context() with { UsageEvidence = [Evidence(20m, false)] },
            shown.CandidateIdentity,
            new AncillarySelection { PackageProductRef = "PACK10", Quantity = 2 });

        Assert.Equal((SelectionEvaluationStatus.Accepted, 35m, 20m, 10m), (one.Status, one.Candidate.Price.RequestedQuantityTotal, one.Candidate.Quantity.CumulativeLimits.Single().VerifiedConsumed, one.Candidate.Quantity.CumulativeLimits.Single().VerifiedRemaining));
        Assert.Equal(SelectionEvaluationStatus.Rejected, two.Status);
        Assert.Contains(ShoppingReasonCodes.QuantityExceedsVerifiedRemaining, two.Candidate.ReasonCodes);
        Assert.Equal((SelectionEvaluationStatus.Accepted, 70m), (incomplete.Status, incomplete.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(incomplete.Candidate.Quantity.CumulativeLimits.Single().VerifiedRemaining);

        lab.Catalog.FamilyAnswer = InventoryReferenceCheck.SourceUnavailable;

        var unverifiedFamily = (await lab.Engine.ShopAsync(context)).One();

        Assert.False(unverifiedFamily.Quantity.CumulativeLimits.Single().FamilyVerified);
        Assert.Contains(ShoppingReasonCodes.CountingFamilyNotVerified, unverifiedFamily.ReasonCodes);
    }

    [Fact]
    public async Task INVENTORY_05_two_products_on_one_physical_resource_share_it_and_no_remaining_stock_is_inferred()
    {
        const long resource = 990001;

        var lab = new ShoppingLab { Catalog = { ResourceAnswer = InventoryReferenceCheck.Verified } };
        var cabin = lab.Definition("A13");
        var hold = lab.Definition("A14");
        var source = new FlightInventorySource(InventoryResourceKind.FlightCount, 101, resource, InventoryRecordStatus.Active, false);

        foreach (var definition in new[] { cabin, hold })
        {
            lab.Price(definition, lab.Provision(definition, scope: ServiceCoverageScope.Sector), Rate(80m));
            lab.Policy(
                definition,
                InventoryAuthority.Local,
                LocalInventoryPattern.FlightCount,
                FlightCountConsumption.Create(resource, definition.Id == cabin.Id ? 1 : 2, InventoryCountUnit.AnimalCarrier),
                flightSources: [source]);
        }

        var result = await lab.Engine.ShopAsync(Trip.Context());

        Assert.Equal(new int?[] { 1, 2 }, result.Candidates.Select(candidate => candidate.Availability.ConsumptionPreview!.CountUnits));
        Assert.All(result.Candidates, candidate =>
        {
            Assert.Equal(InventoryCapacityReadState.ConfiguredNotGuaranteed, candidate.Availability.ConfigState);
            Assert.False(candidate.Availability.IsGuaranteed);
            Assert.True(candidate.Availability.RequiresCheckAtReserve);
        });
        Assert.DoesNotContain(
            typeof(AvailabilityAssessment).GetProperties().Concat(typeof(ConsumptionPreview).GetProperties()),
            property => property.Name.Contains("Remaining", StringComparison.Ordinal) || property.Name.Contains("Total", StringComparison.Ordinal));
    }
}
