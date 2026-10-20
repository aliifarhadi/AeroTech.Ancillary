using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Providers.Offer.Wire;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class BackofficeSurfaceParityTests
{
    private const int Irr = 70;
    private const long EmbeddedSalesDate = 1791706372;

    private static readonly DeskScope Desk = new(Airline, Pos, null, null, Now);

    private static ShoppingLab IrrLab()
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        return lab;
    }

    private static (string Name, Type Type)[] Members(Type type)
        => type.GetProperties().Select(property => (property.Name, property.PropertyType)).OrderBy(member => member.Name, StringComparer.Ordinal).ToArray();

    private static IEnumerable<BackofficeAncillaryOfferDto> Of(BackofficeAncillaryOffersDto response, string reference)
        => response.Offers.Concat(response.BlockedOffers).Where(offer => offer.Service.ServiceDefinitionRef == reference);

    [Fact]
    public void PARITY_BO_01_the_request_has_the_offer_id_and_only_the_options_the_priced_offer_id_does_not_carry()
    {
        Assert.Equal(
            new[] { ("OfferId", typeof(string)), ("Options", typeof(BackofficeAncillaryOfferOptions)), ("OrderId", typeof(long?)) },
            Members(typeof(BackofficeGetAncillaryOffersQuery)));
        Assert.Equal(
            new[] { ("CountryId", typeof(long?)), ("PointOfSaleId", typeof(long?)) },
            Members(typeof(BackofficeAncillaryOfferOptions)));
        Assert.Null(new BackofficeGetAncillaryOffersQuery().Options);
        Assert.All(Members(typeof(BackofficeAncillaryOfferOptions)), member => Assert.Null(typeof(BackofficeAncillaryOfferOptions).GetProperty(member.Name)!.GetValue(new BackofficeAncillaryOfferOptions())));
    }

    [Theory]
    [InlineData("1|1|0;9~1_2~2;3|501||70|1791706372", true, 501L)]
    [InlineData("1|1|0;9~1_2~2;2|||70|1791706372", true, null)]
    [InlineData("1|1|0;9~1_2~2", false, null)]
    [InlineData("1|1|0;9~1_2~2;3|-501||70|1791706372", false, null)]
    [InlineData("1|1|0;9~1_2~2;3|customer||70|1791706372", false, null)]
    [InlineData("1|1|0;9~1_2~2;3|501||70", false, null)]
    public void PARITY_BO_01_the_customer_is_read_from_the_commercial_section_of_the_priced_offer_id(string offerId, bool read, long? expected)
        => Assert.Equal((read, expected), (PricedOfferId.TryRead(offerId, out var commercial), commercial?.CustomerId));

    [Fact]
    public async Task PARITY_BO_01_point_of_sale_and_country_follow_the_backoffice_rules_and_the_currency_is_the_offer_currency()
    {
        var unused = new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [88] };
        var named = await Resolver(eligibility: unused).ResolveAsync(new BackofficeAncillaryOfferOptions { PointOfSaleId = 77, CountryId = 5 });
        var asked = new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [88] };
        var eligible = await Resolver(eligibility: asked).ResolveAsync(new BackofficeAncillaryOfferOptions { CountryId = 5 });
        var anyCountry = new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [88] };

        await Resolver(eligibility: anyCountry).ResolveAsync(new BackofficeAncillaryOfferOptions());

        Assert.Equal(77, named.PointOfSaleIds.Single());
        Assert.Null(unused.Asked);
        Assert.Equal(88, eligible.PointOfSaleIds.Single());
        Assert.Equal((Channel.BackOffice, 5L), (asked.Asked!.Channel, asked.Asked.CountryId));
        Assert.Null(anyCountry.Asked!.CountryId);

        var notConnected = await Assert.ThrowsAsync<BusinessException>(() => Resolver().ResolveAsync(new BackofficeAncillaryOfferOptions { CountryId = 5 }));

        Assert.Equal((16808, 409), (notConnected.Code, notConnected.HttpStatus));

        var offer = await OfferWire.ReadAsync(OfferWire.Captured());

        Assert.Equal((Irr, (long?)9000000000000000900), (offer.CurrencyId, offer.CustomerId));
        Assert.Equal(Irr, ContextOf(offer, Desk, new Dictionary<int, int>()).CurrencyId);
    }

    [Fact]
    public async Task PARITY_BO_01_an_identifier_that_is_not_positive_is_refused_by_the_shopping_context()
    {
        var lab = IrrLab();

        lab.Sellable("A24");

        foreach (var scope in new[] { Desk with { PointOfSaleId = 0 }, Desk with { PointOfSaleId = -77 }, Desk with { CustomerId = -501 }, Desk with { CurrencyId = -1 } })
        {
            var failure = await Assert.ThrowsAsync<BusinessException>(() => ShopAsync(lab, scope: scope));

            Assert.Equal((16801, 422), (failure.Code, failure.HttpStatus));
        }
    }

    [Fact]
    public async Task PARITY_BO_02_the_scope_reads_the_airline_user_the_office_claim_the_home_airline_and_the_request_options_only()
    {
        var foreignFacts = new Caller { CustomerId = 4040, TravelAgencyId = 31, TravelAgencyUserId = 32, TravelAgencyOfficeIds = [33], IndividualId = 34, PartnerApiAccessProfileId = 35 };
        var eligibility = new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [88] };
        var defaults = await Resolver(foreignFacts, eligibility: eligibility).ResolveAsync(null);
        var requested = await Resolver(foreignFacts, eligibility: new Eligibility()).ResolveAsync(new BackofficeAncillaryOfferOptions { PointOfSaleId = 77 });

        Assert.Equal((Airline, Channel.BackOffice, 88L), (defaults.OwnerAirlineId, defaults.Channel, defaults.PointOfSaleIds.Single()));
        Assert.Equal(
            (Channel.BackOffice, (long?)null, 9200L, (long?)null),
            (eligibility.Asked!.Channel, eligibility.Asked.CountryId, eligibility.Asked.AirlineOfficeIds.Single(), eligibility.Asked.TravelAgencyOfficeId));
        Assert.Equal((Airline, Channel.BackOffice, 77L), (requested.OwnerAirlineId, requested.Channel, requested.PointOfSaleIds.Single()));
        Assert.Equal(
            new[] { typeof(ICallerContext), typeof(IOperatorAirlineResolver), typeof(IPointOfSaleEligibilityReference) },
            typeof(BackofficeSalesScopeResolver).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));

        foreach (var resolver in new[]
                 {
                     Resolver(new Caller { AirlineUserId = null }),
                     Resolver(new Caller { AirlineOfficeId = null }),
                     Resolver(new Caller { ContextType = CallerContextType.TravelAgency, TravelAgencyId = 31, TravelAgencyOfficeIds = [33] }),
                     Resolver(new Caller { ContextType = CallerContextType.PartnerApi, PartnerApiAccessProfileId = 35 }),
                     Resolver(new Caller { ContextType = CallerContextType.Individual, IndividualId = 34, CustomerId = 4040 })
                 })
        {
            var failure = await Assert.ThrowsAsync<BusinessException>(() => resolver.ResolveAsync(new BackofficeAncillaryOfferOptions { PointOfSaleId = 77 }));

            Assert.Equal((16809, 403), (failure.Code, failure.HttpStatus));
        }
    }

    [Fact]
    public async Task PARITY_BO_03_the_customer_currency_and_sales_date_come_from_the_commercial_section_of_the_priced_offer()
    {
        Assert.EndsWith($";{(int)Channel.PartnerAPI}|9000000000000000900||{Irr}|{EmbeddedSalesDate}", OfferWire.OfferId, StringComparison.Ordinal);

        var lab = IrrLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi), Rate(900_000m, Irr));

        var offer = (await OfferWire.ReadAsync(OfferWire.Captured())) with { SalesChannel = Channel.BackOffice };
        var pricedAt = DateTimeOffset.FromUnixTimeSeconds(EmbeddedSalesDate);
        var context = FlightOfferShoppingContextMapper.Map(
            offer,
            FlightOfferShoppingContextMapper.Facts(offer, new Dictionary<long, long>()),
            Desk,
            Pos,
            new Dictionary<int, int>(),
            new Dictionary<int, string>(),
            pricedAt,
            pricedAt + SourceContextTtl,
            FlightOfferFingerprint.Of(offer));
        var service = Offers(
            Sources(new FlightOfferShoppingSource(
                new FixedOffers(offer),
                new TestOnlyAirportReference(),
                new TestOnlyAirFareReference(),
                new FixedClock { Now = pricedAt },
                new FlightOfferShoppingOptions(SourceContextTtl))), lab);
        var response = BackofficeAncillaryOffersMapper.Map(await service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk));

        Assert.Equal((Pos, (long?)9000000000000000900, Irr, pricedAt), (context.PointOfSaleId, context.CustomerId, context.CurrencyId, context.EvaluatedAtUtc));
        Assert.Equal(OfferWire.OfferId, context.SourceIdentity.TrustedSourceReference);
        Assert.Equal((4, 0), (response.Offers.Count, response.BlockedOffers.Count));
        Assert.Equal((Airline, Pos, pricedAt), (response.Scope.OwnerAirlineId, response.Scope.PointOfSaleId!.Value, response.EvaluatedAtUtc));
        Assert.Equal((pricedAt, Irr, 528010024m), (response.FlightOffer.SourcePricedAt!.Value, response.FlightOffer.CurrencyId, response.FlightOffer.TotalAmount));
        Assert.All(response.Offers, candidate => Assert.Equal((900_000m, Irr), (candidate.Pricing.CompleteUnitTotal, candidate.Pricing.CurrencyId)));
    }

    [Fact]
    public async Task PARITY_BO_04_the_sales_date_of_the_priced_offer_decides_the_sales_rules()
    {
        var lab = IrrLab();
        var wifi = lab.Definition("A24");

        lab.Price(
            wifi,
            lab.Provision(wifi, rules: Rules(sales: new ProvisionSalesRestrictionsArgs(Now.AddDays(-10), Now.AddDays(10), [], [], []))),
            Rate(900_000m, Irr));

        var open = await ShopAsync(lab, scope: Desk);
        var before = await ShopAsync(lab, scope: Desk with { EvaluatedAtUtc = Now.AddDays(-30) });
        var after = await ShopAsync(lab, scope: Desk with { EvaluatedAtUtc = Now.AddDays(30) });

        Assert.Equal((4, Now), (open.Offers.Count, open.EvaluatedAtUtc));
        Assert.Equal((0, Now.AddDays(-30)), (before.Offers.Count, before.EvaluatedAtUtc));
        Assert.Equal((0, Now.AddDays(30)), (after.Offers.Count, after.EvaluatedAtUtc));
        Assert.All(new[] { open, before, after }, response => Assert.Equal(response.EvaluatedAtUtc, response.FlightOffer.SourcePricedAt));
        Assert.Equal(
            Desk.EvaluatedAtUtc + SourceContextTtl,
            ContextOf(await OfferWire.ReadAsync(OfferWire.Captured()), Desk, new Dictionary<int, int>()).SourceIdentity.ValidUntilUtc);
    }

    [Fact]
    public async Task PRICE_01_an_extra_bag_is_its_base_plus_the_added_tax_once_for_each_traveller_and_portion()
    {
        var lab = IrrLab();
        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag), Rate(5_000_000m, Irr, components: [Tax("VAT", 450_000m, TaxTreatment.AddedToBase, Irr)]));

        var response = await ShopAsync(lab, scope: Desk);

        Assert.Equal(
            new[] { ("ADT-1", "B1"), ("ADT-1", "B2"), ("CHD-1", "B1"), ("CHD-1", "B2") },
            response.Offers.Select(offer => (offer.Coverage.TravellerRefs.Single(), offer.Coverage.PortionRefs.Single())).Order());
        Assert.All(response.Offers, offer =>
        {
            Assert.Equal(
                ("Filed", "Complete", Irr, "PerPiece", 5_000_000m, 5_450_000m, true),
                (offer.Pricing.Origin.Name, offer.Pricing.Status.Name, offer.Pricing.CurrencyId, offer.Pricing.PricingUnit!.Name, offer.Pricing.BaseAmount, offer.Pricing.CompleteUnitTotal, offer.Pricing.IsOrderLevelTotalComplete));
            Assert.Equal(("VAT", 450_000m, Irr), (offer.Pricing.AddedTaxes.Single().Code, offer.Pricing.AddedTaxes.Single().Amount, offer.Pricing.AddedTaxes.Single().CurrencyId));
            Assert.Empty(offer.Pricing.IncludedTaxes);
            Assert.Empty(offer.Pricing.AppliedUnitFees);
            Assert.Empty(offer.Pricing.UnappliedFees);
            Assert.Equal(("Piece", "Portion"), (offer.Quantity.Unit.Name, offer.Coverage.Scope.Name));
        });
        Assert.Equal(528010024m, response.FlightOffer.TotalAmount);
    }

    [Fact]
    public async Task PRICE_02_a_ticket_fee_is_listed_but_never_added_to_the_unit_total()
    {
        var lab = IrrLab();
        var wifi = lab.Definition("A24");

        lab.Price(
            wifi,
            lab.Provision(wifi),
            Rate(900_000m, Irr, components: [Tax("VAT", 81_000m, TaxTreatment.AddedToBase, Irr), Fee("TKT", 100_000m, FeeApplicationUnit.Ticket, Irr)]));

        var response = await ShopAsync(lab, scope: Desk);

        Assert.Equal(4, response.Offers.Count);
        Assert.All(response.Offers, offer =>
        {
            Assert.Equal((900_000m, 981_000m, false), (offer.Pricing.BaseAmount, offer.Pricing.CompleteUnitTotal, offer.Pricing.IsOrderLevelTotalComplete));
            Assert.Equal(("VAT", 81_000m), (offer.Pricing.AddedTaxes.Single().Code, offer.Pricing.AddedTaxes.Single().Amount));
            Assert.Equal(
                ("TKT", 100_000m, "Ticket"),
                (offer.Pricing.UnappliedFees.Single().Code, offer.Pricing.UnappliedFees.Single().Amount, offer.Pricing.UnappliedFees.Single().FeeApplicationUnit!.Name));
            Assert.Empty(offer.Pricing.AppliedUnitFees);
            Assert.Contains(ShoppingReasonCodes.FeeNotAppliedAtUnitLevel, offer.Pricing.ReasonCodes);
        });
    }

    [Fact]
    public async Task PRICE_03_an_included_tax_is_stated_once_and_an_offer_without_a_filed_price_has_no_amount()
    {
        var lab = IrrLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi), Rate(1_090_000m, Irr, components: [Tax("VAT", 90_000m, TaxTreatment.IncludedInBase, Irr)]));

        var (quoted, _) = lab.Sellable("A10");
        var response = await ShopAsync(lab, scope: Desk);
        var included = Of(response, wifi.ServiceDefinitionRef).ToList();
        var unpriced = Of(response, quoted.ServiceDefinitionRef).ToList();

        Assert.Equal(4, included.Count);
        Assert.All(included, offer =>
        {
            Assert.Equal((1_090_000m, 1_090_000m, true), (offer.Pricing.BaseAmount, offer.Pricing.CompleteUnitTotal, offer.Pricing.IsOrderLevelTotalComplete));
            Assert.Equal(("VAT", 90_000m), (offer.Pricing.IncludedTaxes.Single().Code, offer.Pricing.IncludedTaxes.Single().Amount));
            Assert.Empty(offer.Pricing.AddedTaxes);
        });
        Assert.NotEmpty(unpriced);
        Assert.All(unpriced, offer =>
        {
            Assert.Equal(("ExternalQuote", "QuoteRequired"), (offer.Pricing.Origin.Name, offer.Pricing.Status.Name));
            Assert.Equal((null, null, null), (offer.Pricing.BaseAmount, offer.Pricing.CompleteUnitTotal, offer.Pricing.CurrencyId));
            Assert.False(offer.Pricing.IsOrderLevelTotalComplete);
            Assert.False(offer.CanPurchase);
        });
    }

    [Fact]
    public async Task PRICE_04_a_per_kilogram_rate_is_shown_as_a_rate_and_is_never_multiplied_by_a_weight_nobody_stated()
    {
        var lab = IrrLab();
        var overweight = lab.Definition("A03", pricingUnit: PricingUnit.PerKilogram);

        lab.Price(overweight, lab.Provision(overweight, quantityUnit: AncillaryQuantityUnit.Kilogram), Rate(250_000m, Irr));

        var response = await ShopAsync(lab, scope: Desk);
        var shown = Of(response, overweight.ServiceDefinitionRef).ToList();
        var context = ContextOf(await OfferWire.ReadAsync(OfferWire.Captured()), Desk, new Dictionary<int, int>());
        var candidate = (await lab.Engine.ShopAsync(context, new ShoppingFilter { IncludeNonSellable = true })).Candidates[0];
        var measured = await lab.Engine.EvaluateSelectionAsync(context, candidate.CandidateIdentity, new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 27m });

        Assert.Equal(4, shown.Count);
        Assert.All(shown, offer =>
        {
            Assert.Equal(("NeedsSelection", "NeedsInput", "TypedForm"), (offer.OfferReadiness.Name, offer.DisplayState.Name, offer.Selection.Kind.Name));
            Assert.Equal(
                ("PerKilogram", "Kilogram", 250_000m, 250_000m, Irr),
                (offer.Pricing.PricingUnit!.Name, offer.Quantity.Unit.Name, offer.Pricing.BaseAmount, offer.Pricing.CompleteUnitTotal, offer.Pricing.CurrencyId));
            Assert.False(offer.CanPurchase);
        });
        Assert.Equal(
            new[] { "BaseAmount", "CompleteUnitTotal" },
            typeof(OfferPricingDto).GetProperties().Where(property => property.PropertyType == typeof(decimal?)).Select(property => property.Name).Order());
        Assert.Null(candidate.Price.RequestedQuantityTotal);
        Assert.Equal((250_000m, null), (measured.Candidate.Price.CompleteUnitTotal, measured.Candidate.Price.RequestedQuantityTotal));
    }

    [Fact]
    public async Task INV_01_a_product_without_a_stock_policy_or_with_an_unlimited_one_is_shown_without_any_stock_claim()
    {
        var lab = new ShoppingLab();
        var (bare, _) = lab.Sellable("A24");
        var (open, _) = lab.Sellable("A11");

        lab.Policy(open, InventoryAuthority.Unlimited);

        var response = await ShopAsync(lab);
        var none = Of(response, bare.ServiceDefinitionRef).ToList();
        var unlimited = Of(response, open.ServiceDefinitionRef).ToList();

        Assert.NotEmpty(none);
        Assert.NotEmpty(unlimited);
        Assert.All(none, offer =>
        {
            Assert.Equal((null, "NotConfigured", "NotChecked", false), (offer.Availability.Authority?.Name, offer.Availability.ConfigState.Name, offer.Availability.CheckedState.Name, offer.Availability.IsGuaranteed));
            Assert.Contains(ShoppingReasonCodes.InventoryNotConfigured, offer.Availability.ReasonCodes);
        });
        Assert.All(unlimited, offer => Assert.Equal(
            ("Unlimited", "Unlimited", "NotChecked", false, false),
            (offer.Availability.Authority?.Name, offer.Availability.ConfigState.Name, offer.Availability.CheckedState.Name, offer.Availability.IsGuaranteed, offer.Availability.RequiresCheckAtReserve)));
        Assert.All(none.Concat(unlimited), offer =>
        {
            Assert.Null(offer.Availability.ConsumptionPreview);
            Assert.False(offer.CanPurchase);
        });
        Assert.False(response.Completeness.LiveSupplierAvailabilityChecked);
    }

    [Fact]
    public async Task INV_02_supplier_stock_is_a_check_still_to_be_made_and_a_seat_has_no_inventory_in_this_service()
    {
        var lab = new ShoppingLab();
        var (wifi, _) = lab.Sellable("A24");
        var (seat, _) = lab.Sellable("A07");

        lab.Policy(wifi, InventoryAuthority.Supplier);

        var response = await ShopAsync(lab);

        Assert.All(Of(response, wifi.ServiceDefinitionRef), offer =>
        {
            Assert.Equal(
                ("Supplier", "DelegatedCheckRequired", "NotChecked", false, true),
                (offer.Availability.Authority?.Name, offer.Availability.ConfigState.Name, offer.Availability.CheckedState.Name, offer.Availability.IsGuaranteed, offer.Availability.RequiresCheckAtReserve));
            Assert.Contains(ShoppingReasonCodes.InventoryDelegatedCheckRequired, offer.Availability.ReasonCodes);
        });
        Assert.All(Of(response, seat.ServiceDefinitionRef), offer =>
        {
            Assert.Null(offer.Availability.Authority);
            Assert.Equal(
                ("NotApplicable", "NotChecked", false),
                (offer.Availability.ConfigState.Name, offer.Availability.CheckedState.Name, offer.Availability.IsGuaranteed));
            Assert.DoesNotContain(ShoppingReasonCodes.BlockedExternalReference, offer.Availability.ReasonCodes);
            Assert.DoesNotContain(ShoppingReasonCodes.InventoryNotConfigured, offer.Availability.ReasonCodes);
        });
        Assert.Equal(4, Of(response, wifi.ServiceDefinitionRef).Count());
        Assert.Equal(4, Of(response, seat.ServiceDefinitionRef).Count());
        Assert.All(response.Offers.Concat(response.BlockedOffers), offer =>
        {
            Assert.Null(offer.Availability.ConsumptionPreview);
            Assert.False(offer.CanPurchase);
            Assert.NotEqual("AvailableAsOf", offer.Availability.CheckedState.Name);
        });
        Assert.False(response.Completeness.LiveSupplierAvailabilityChecked);
    }
}
