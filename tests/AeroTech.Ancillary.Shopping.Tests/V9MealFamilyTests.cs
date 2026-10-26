using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V9MealFamilyTests
{
    private const int NotSellable = 16820;

    private static FactCompleteness OfferCompleteness()
    {
        var offer = new FlightOffer(V8Lab.Offer, ShoppingLab.Now, null, null, ShoppingLab.Eur, null, 0m, 0m, 0m, [], [], [], [], [], null, null);

        return FlightOfferShoppingContextMapper.Facts(offer, new Dictionary<long, long>()).Completeness;
    }

    private static AncillaryOfferRow Row(IEnumerable<AncillaryOfferRow> rows, string variantCode, string travellerRef = "T1")
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode && row.Candidate.CandidateIdentity.TravellerRef == travellerRef);

    [Fact]
    public void M01_OfferSourceHasVerifiedAndEmptyExistingServices()
    {
        var completeness = OfferCompleteness();

        Assert.True(completeness.ExistingServicesComplete);
        Assert.False(completeness.FareBenefitsComplete);
    }

    [Fact]
    public async Task M02_MealOfAFamilyIsSignedOnAnOfferSource()
    {
        var lab = new V8Lab();
        lab.Sellable("A11", amount: 0m);
        var context = V8Lab.Context() with { CoverageCompleteness = OfferCompleteness() };

        var meal = Row(await lab.ShopAsync(context), "A11");
        var unverified = await lab.ShopAsync(context with { CoverageCompleteness = context.CoverageCompleteness with { ExistingServicesComplete = false } });

        Assert.StartsWith("d_", meal.OfferItemId);
        Assert.Equal("MAIN_MEAL", meal.Definition.Meal!.ExclusiveMealFamilyCode);
        Assert.DoesNotContain(unverified, row => row.Candidate.VariantCode == "A11");
    }

    [Fact]
    public async Task M03_TwoMealsOfOneFamilyForOneTravellerAndUnitAreRefusedAtSubmit()
    {
        var lab = new V8Lab();
        lab.Sellable("A11", amount: 0m);
        lab.Sellable("A12", amount: 12m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);
        var special = V8Lab.Direct(Row(rows, "A11").OfferItemId!);
        var preorder = V8Lab.Direct(Row(rows, "A12").OfferItemId!);

        var refused = await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.SubmitAsync([special, preorder], context));

        Assert.StartsWith("selections[1]: ", refused.Message);
        Assert.Contains("MAIN_MEAL", refused.Message);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
        Assert.Single(await lab.Resolver.ResolveAsync((await lab.SubmitAsync([preorder], context)).SelectionId));
    }

    [Fact]
    public async Task M04_SameFamilyForDifferentTravellersResolves()
    {
        var lab = new V8Lab();
        lab.Sellable("A11", amount: 0m);
        var context = V8Lab.Context(travellers: [Trip.Adult("T1"), Trip.Adult("T2")]);
        var rows = await lab.ShopAsync(context);
        var selectionId = (await lab.SubmitAsync([V8Lab.Direct(Row(rows, "A11", "T1").OfferItemId!), V8Lab.Direct(Row(rows, "A11", "T2").OfferItemId!)], context)).SelectionId;

        var meals = await lab.Resolver.ResolveAsync(selectionId);

        Assert.Equal(["T1", "T2"], meals.Select(meal => meal.TravellerRef));
    }

    [Fact]
    public async Task M05_ResolveStillRefusesTwoMealsOfOneFamilyInOneSelection()
    {
        var lab = new V8Lab();
        lab.Sellable("A11", amount: 0m);
        lab.Sellable("A12", amount: 12m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);
        var lines = new[] { Row(rows, "A11"), Row(rows, "A12") }
            .Select(row => new SelectionSnapshotLineArgs(
                row.OfferItemId!,
                row.Candidate.Price.PricingRateId!.Value,
                row.Candidate.CandidateIdentity.CoverageScope,
                row.Candidate.CandidateIdentity.TravellerRef,
                row.Candidate.CandidateIdentity.CoverageRef,
                new string('o', 43),
                new string('s', 43),
                1,
                null))
            .ToList();
        lab.Shop.Catalog.Snapshots.Add(SelectionSnapshot.Create("s_".PadRight(24, '1'), V8Lab.Offer, lines, ShoppingLab.Now, ShoppingLab.Now.AddMinutes(15)));

        var refused = await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.Resolver.ResolveAsync("s_".PadRight(24, '1')));

        Assert.Contains("MAIN_MEAL", refused.Message);
    }
}
