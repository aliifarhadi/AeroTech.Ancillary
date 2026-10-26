using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V8ScopeTests
{
    private static readonly ShoppingFlight F1 = Trip.Flight("F1", 101);
    private static readonly ShoppingFlight F2 = Trip.Flight("F2", 102);
    private static readonly ShoppingFlight F3 = Trip.Flight("F3", 103);

    private static AncillaryShoppingContext TwoTravellers(IReadOnlyList<ShoppingFlight> flights, IReadOnlyList<ShoppingPortion> portions)
        => V8Lab.Context(flights, portions, [Trip.Adult("T1"), Trip.Adult("T2")]);

    private static AncillaryShoppingContext TwoPortions()
        => TwoTravellers([F1, F2, F3], [Trip.Portion("P1", 1, F1, F2), Trip.Portion("P2", 2, F3)]);

    private static IReadOnlyList<string> Ids(IEnumerable<AncillaryOfferRow> rows)
        => rows.Select(row => row.OfferItemId!).ToList();

    [Fact]
    public async Task S01_OrderUnitDirectGivesOneRowAndOneSignedId()
    {
        var lab = new V8Lab();
        lab.Sellable("A24", specification: spec => spec with { Connectivity = spec.Connectivity! with { MaxDevices = null } }, scope: ServiceCoverageScope.Order);

        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context()));

        Assert.Equal(SelectionScope.PerOrder, row.SelectionScope);
        Assert.StartsWith("d_", row.OfferItemId);
        Assert.Null(row.Options);
        Assert.False(row.QuantityPolicy.Selectable);
    }

    [Fact]
    public async Task S02_OrderUnitIsNotBuiltForAQuantityVariant()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", scope: ServiceCoverageScope.Order, maxQuantity: 2);

        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
    }

    [Fact]
    public async Task S03_TravellerJourneyDirectGivesOneRowPerTraveller()
    {
        var lab = new V8Lab();
        lab.Sellable("A24", specification: spec => spec with { Connectivity = spec.Connectivity! with { MaxDevices = null } }, scope: ServiceCoverageScope.Journey);

        var rows = await lab.ShopAsync(TwoTravellers([F1], [Trip.Portion("P1", 1, F1)]));

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Equal(SelectionScope.PerTraveller, row.SelectionScope));
        Assert.Equal(["T1", "T2"], rows.Select(row => row.Candidate.CandidateIdentity.TravellerRef).Order());
        Assert.Equal(2, Ids(rows).Distinct().Count());
    }

    [Fact]
    public async Task S04_TravellerJourneyQuantityGivesOneIdAndRangePerTraveller()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", scope: ServiceCoverageScope.Journey, maxQuantity: 2);

        var rows = await lab.ShopAsync(TwoTravellers([F1], [Trip.Portion("P1", 1, F1)]));

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Equal((true, 1, 2), (row.QuantityPolicy.Selectable, row.QuantityPolicy.Min, row.QuantityPolicy.Max)));
        Assert.Equal(2, Ids(rows).Distinct().Count());
    }

    [Fact]
    public async Task S05_TravellerPortionGivesOneRowPerTravellerAndPortion()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 2);

        var rows = await lab.ShopAsync(TwoPortions());

        Assert.Equal(4, rows.Count);
        Assert.All(rows, row => Assert.Equal(SelectionScope.PerTravellerPerPortion, row.SelectionScope));
    }

    [Fact]
    public async Task S06_TravellerFlightGivesOneRowPerTravellerAndFlight()
    {
        var lab = new V8Lab();
        lab.Sellable("A05");

        var rows = await lab.ShopAsync(TwoPortions());

        Assert.Equal(6, rows.Count);
        Assert.All(rows, row => Assert.Equal(SelectionScope.PerTravellerPerFlight, row.SelectionScope));
    }

    [Fact]
    public async Task S07_ConnectingPortionGivesOneRowNotOnePerLeg()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 2);

        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context([F1, F2], [Trip.Portion("P1", 1, F1, F2)])));

        Assert.Equal("P1", row.Candidate.CandidateIdentity.CoverageRef);
        Assert.Equal(["F1", "F2"], row.Candidate.FlightRefs);
    }

    [Fact]
    public async Task S08_SameRateTwoTravellersGetDifferentIds()
    {
        var lab = new V8Lab();
        lab.Sellable("A05");

        var rows = (await lab.ShopAsync(TwoTravellers([F1], [Trip.Portion("P1", 1, F1)]))).ToList();

        Assert.Equal(rows[0].Candidate.Price.PricingRateId, rows[1].Candidate.Price.PricingRateId);
        Assert.NotEqual(rows[0].OfferItemId, rows[1].OfferItemId);
    }

    [Fact]
    public async Task S09_SameRateTwoFlightsGetDifferentIds()
    {
        var lab = new V8Lab();
        lab.Sellable("A05");

        var rows = (await lab.ShopAsync(V8Lab.Context([F1, F2], [Trip.Portion("P1", 1, F1), Trip.Portion("P2", 2, F2)]))).ToList();

        Assert.Equal(2, rows.Count);
        Assert.Equal(rows[0].Candidate.Price.PricingRateId, rows[1].Candidate.Price.PricingRateId);
        Assert.NotEqual(rows[0].OfferItemId, rows[1].OfferItemId);
    }

    [Fact]
    public async Task S10_SameRateTwoPortionsGetDifferentIds()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 1);

        var rows = (await lab.ShopAsync(V8Lab.Context([F1, F2], [Trip.Portion("P1", 1, F1), Trip.Portion("P2", 2, F2)]))).ToList();

        Assert.Equal(2, rows.Count);
        Assert.Equal(rows[0].Candidate.Price.PricingRateId, rows[1].Candidate.Price.PricingRateId);
        Assert.NotEqual(rows[0].OfferItemId, rows[1].OfferItemId);
    }

    [Fact]
    public async Task S11_OneIdCarriesTheAllowedQuantityRange()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 2);

        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context()));
        var signed = (await lab.Codec.ReadAsync(row.OfferItemId, SelectionIdKind.Direct)).Selection!;

        Assert.Equal((1, 2), (signed.MinQuantity, signed.MaxQuantity));
        Assert.Equal((row.QuantityPolicy.Min, row.QuantityPolicy.Max), (signed.MinQuantity!.Value, signed.MaxQuantity!.Value));
    }

    [Fact]
    public async Task S12_QuantityZeroIsNeverSigned()
    {
        var lab = new V8Lab();
        var (_, provision, _) = lab.Sellable("A01", maxQuantity: 2);
        var context = V8Lab.Context();

        var row = Assert.Single(await lab.ShopAsync(context));

        Assert.Equal(1, row.QuantityPolicy.Min);
        Assert.Null(lab.Signer.SignDirect(context, row.Candidate, provision, 0, 2));
    }

    [Fact]
    public async Task S13_ZeroRateIsSignedWithItsRealRateAndTotalZero()
    {
        var lab = new V8Lab();
        lab.Sellable("A11", amount: 0m);

        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context()));
        var signed = (await lab.Codec.ReadAsync(row.OfferItemId, SelectionIdKind.Direct)).Selection!;

        Assert.Equal(0m, row.UnitAmount);
        Assert.True(signed.PricingRateId > 0);
        Assert.Equal(row.Candidate.Price.PricingRateId, signed.PricingRateId);
    }

    [Fact]
    public async Task S14_NoSectorRowWithoutATraveller()
    {
        var lab = new V8Lab();
        lab.Sellable("A05");

        var rows = await lab.ShopAsync(TwoPortions());

        Assert.All(rows, row => Assert.False(string.IsNullOrEmpty(row.Candidate.CandidateIdentity.TravellerRef)));
    }

    [Fact]
    public async Task S15_UnknownTravellerInTheUnitGetsNoId()
    {
        var lab = new V8Lab();
        var (_, provision, _) = lab.Sellable("A05");
        var context = V8Lab.Context();
        var row = Assert.Single(await lab.ShopAsync(context));
        var forged = row.Candidate with { Scope = row.Candidate.Scope with { TravellerRefs = ["T9"] } };

        Assert.Null(lab.Signer.SignDirect(context, forged, provision, 1, 1));
    }

    [Fact]
    public async Task S16_FlightOutsideTheOfferGetsNoSnapshot()
    {
        var lab = new V8Lab();
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var row = Assert.Single(await lab.ShopAsync(context));
        var signed = (await lab.Codec.ReadAsync(row.OfferItemId, SelectionIdKind.Reference)).Selection!;
        var forged = lab.Codec.Issue(signed with { CoverageRef = "F9" });

        await BusinessAssert.ThrowsAsync(16802, 404, () => lab.SubmitAsync(
            forged,
            AncillaryProfile.AssistedTravel,
            new AssistedTravelSelectionOptions("WCHR", null, null),
            context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }
}
