using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V11VariantMatrixAcceptanceTests
{
    private const long Flight = 301;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V11VariantMatrixAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 1_140_000;

    private async Task<long> ProvisionAsync(string code, bool withPolicy, bool immediate = true)
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var variant = V122Catalog.Case(code);
        var definition = await _harness.Proof.DefinitionAsync(immediate
            ? V122Catalog.Define(variant, airlineId, supplierId) with { Booking = variant.Booking with { ConfirmationRequirement = ConfirmationRequirement.Immediate } }
            : V122Catalog.Define(variant, airlineId, supplierId));
        var catalogRule = V122Catalog.Rule(variant, definition.Id);
        var filedRule = catalogRule.Origin == PriceOrigin.ExternalQuote ? catalogRule with { Origin = PriceOrigin.Filed, QuoteProviderKey = null } : catalogRule;
        var rule = await _harness.Proof.RuleAsync(
            filedRule,
            filedRule.Origin == PriceOrigin.Filed && variant.Disposition == CommercialDisposition.Paid
                ? provisionId => new TestDefinePricingRatesCommand(provisionId, [new(null, null, null, new MoneyInput(25m, Eur), null)])
                : null);

        if (withPolicy)
            await _harness.ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, variant.Reference, definition.Id, InventoryAuthority.Unlimited));

        return rule.Provision.Id;
    }

    private static ISelectionOptions? OptionsOf(string code)
        => code switch
        {
            "A04" => new BaggageSelectionOptions(null, null, new SelectedDimensionsInput(100m, 60m, 40m), null),
            "A16" => new AssistedTravelHoldSelectionOptions("BLND"),
            "A22" => new AirportServiceSelectionOptions(new DateTimeOffset(2027, 4, 6, 8, 30, 0, TimeSpan.FromHours(3.5)), 1),
            _ => null
        };

    private TestHoldAncillaryServicesCommand Hold(long orderId, string code, long provisionId)
        => M1Commands.Hold(
            $"V11-M-{orderId}",
            orderId,
            _clock.Now.AddMinutes(30),
            new HoldSelection(V122Catalog.ProfileOf(code), code, provisionId, orderId * 10 + 1, 71, [Flight], 1, OptionsOf(code)));

    [Theory]
    [InlineData("A02")]
    [InlineData("A04")]
    [InlineData("A05")]
    [InlineData("A16")]
    [InlineData("A21")]
    [InlineData("A22")]
    public async Task V11_matrix_a_supported_variant_is_held_with_its_options_and_read_back_in_its_family(string code)
    {
        var provisionId = await ProvisionAsync(code, withPolicy: true);
        var orderId = NextOrderId;
        await using var scope = new AncillaryScope(_database, _clock);

        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, code, provisionId));

        await using var reader = new AncillaryScope(_database, _clock);
        var read = await reader.GetHoldById.ExecuteAsync(hold.HoldId);
        var json = JsonSerializer.Serialize(read, HostJson.Options);

        Assert.Equal(AncillaryReservationUnitStatus.Held, Assert.Single(read.Units()).Status);
        Assert.Contains($"\"variantCode\":\"{code}\"", json, StringComparison.Ordinal);
        Assert.Contains($"\"provisionId\":\"{provisionId}\"", json, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("A17", false, "needs a supplier confirmation")]
    [InlineData("A19", true, "variant A19")]
    public async Task V11_matrix_a_gated_variant_is_refused_before_any_write(string code, bool immediate, string reason)
    {
        var provisionId = await ProvisionAsync(code, withPolicy: false, immediate);
        var orderId = NextOrderId;
        await using var scope = new AncillaryScope(_database, _clock);

        var refused = await BusinessAssert.ThrowsAsync(16419, 409, () => scope.HoldAncillaryServices.HoldAsync(Hold(orderId, code, provisionId)));

        Assert.Contains(reason, refused.Message, StringComparison.Ordinal);
        Assert.Equal(
            new[] { "0" },
            await _harness.RowsAsync($"SELECT CONCAT(COUNT(*), '') AS Value FROM Ancillary.AncillaryReservations WHERE OrderId = {orderId}"));
    }
}
