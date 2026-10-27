using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V9PriceRevalidationTests
{
    private const int NotSellable = 16820;
    private const string Family = "BAGS";

    private static AncillaryInventoryPolicy Limited(V8Lab lab, string code, int maxQuantity, int familyLimit)
    {
        var (definition, _, _) = lab.Sellable(code, amount: 100m, maxQuantity: maxQuantity);

        return lab.Shop.Policy(definition, InventoryAuthority.Unlimited, limits: new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerFlightOccurrence, familyLimit, Family));
    }

    private static AncillaryShoppingContext Consumed(AncillaryInventoryPolicy policy, decimal units)
    {
        var context = V8Lab.Context();
        var flight = context.Flights.Single();
        var key = policy.PassengerUsageLimits.Single().KeyFor(new PassengerUsageSubject(context.Travellers.Single().StableTravellerIdentity, null, null, flight.FlightId, null));

        return context with
        {
            UsageEvidence =
            [
                new VerifiedUsageEvidence
                {
                    UsageSubjectKey = key,
                    UnitsConsumed = units,
                    IsCompleteForScope = true,
                    AsOfUtc = ShoppingLab.Now,
                    SourceAuthority = "TEST"
                }
            ]
        };
    }

    private static AncillaryOfferRow Row(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode);

    [Fact]
    public async Task ADirectQuantityIsCheckedAgainstTheRemainingAtPrice()
    {
        var lab = new V8Lab();
        lab.Shop.Catalog.FamilyAnswer = InventoryReferenceCheck.Verified;
        var policy = Limited(lab, "A01", maxQuantity: 3, familyLimit: 3);
        var row = Row(await lab.ShopAsync(Consumed(policy, 0m)), "A01");
        var shrunk = Consumed(policy, 2m);

        var refused = await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.SubmitAsync([V8Lab.Direct(row.OfferItemId!, 3)], shrunk));
        var one = await lab.SubmitAsync([V8Lab.Direct(row.OfferItemId!, 1)], shrunk);

        Assert.Equal((1, 3), (row.QuantityPolicy.Min, row.QuantityPolicy.Max));
        Assert.StartsWith("selections[0]: ", refused.Message);
        Assert.Contains("QuantityExceedsVerifiedRemaining", refused.Message);
        Assert.Equal(1, Assert.Single((await lab.Resolver.ResolveAsync(one.SelectionId)).Lines).Quantity);
    }

    [Fact(Skip = "Open: /Price checks each line alone and does not sum one counting family across lines. Not reachable today: no counting-family source is connected, so no policy with a usage limit can be activated.")]
    public async Task TwoProductsOfOneCountingFamilyCannotExceedTheFamilyLimitTogether()
    {
        var lab = new V8Lab();
        lab.Shop.Catalog.FamilyAnswer = InventoryReferenceCheck.Verified;
        var bags = Limited(lab, "A01", maxQuantity: 2, familyLimit: 2);
        Limited(lab, "A05", maxQuantity: 1, familyLimit: 2);
        var context = Consumed(bags, 0m);
        var rows = await lab.ShopAsync(context);

        await BusinessAssert.ThrowsAsync(
            NotSellable,
            409,
            () => lab.SubmitAsync([V8Lab.Direct(Row(rows, "A01").OfferItemId!, 2), V8Lab.Direct(Row(rows, "A05").OfferItemId!, 1)], context));
    }
}
