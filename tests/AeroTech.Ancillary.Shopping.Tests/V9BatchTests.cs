using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Requests;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V9BatchTests
{
    private const int InvalidSignature = 16816;
    private const int Expired = 16817;
    private const int NotFound = 16819;
    private const int NotSellable = 16820;
    private const int InvalidOptions = 16821;

    private static readonly AssistedTravelSelectionOptions Wchr = new("WCHR", null, null);
    private static readonly AssistedTravelSelectionOptions Wchs = new("WCHS", null, null);

    private static ApiPriceAncillaryOffersCommand Command(params AncillarySelectionLine[] lines) => new(V8Lab.Offer, lines);

    private static AncillarySelectionLine Wheelchair(string reference, AssistedTravelSelectionOptions options) => new(reference, Options: new SelectionOptionsInput(AssistedTravel: options));

    private static Task<SelectionSnapshotResult> SubmitAsync(V8Lab lab, LabShoppingSource source, SnapshotStore store, ApiPriceAncillaryOffersCommand command)
        => lab.Submissions(source, store).SubmitAsync(command);

    [Fact]
    public async Task P01_P11_TwoWheelchairsMakeOneSelectionWithOneLoadAndOneSave()
    {
        var (lab, context, references) = await V8Lab.WheelchairsAsync(1);
        var source = new LabShoppingSource([context]);
        var store = new SnapshotStore(lab.Shop.Catalog);

        var result = await SubmitAsync(lab, source, store, Command(Wheelchair(references[0], Wchr), Wheelchair(references[1], Wchs)));

        var snapshot = Assert.Single(lab.Shop.Catalog.Snapshots);
        Assert.Equal(result.SelectionId, snapshot.Id);
        Assert.Equal(snapshot.ExpiresAtUtc, result.ExpiresAtUtc);
        Assert.Matches("^s_[A-Za-z0-9_-]{22}$", result.SelectionId);
        Assert.Equal(["{\"assistanceSsrCode\":\"WCHR\"}", "{\"assistanceSsrCode\":\"WCHS\"}"], snapshot.Lines.Select(line => line.SelectedJson));
        Assert.Equal((1, 1), (source.Loads, store.Saves));
    }

    [Fact]
    public async Task P09_P11_P12_P13_P14_OneRequestTakesTheOptionsOfEveryFamily()
    {
        var lab = new V8Lab();
        lab.Sellable("A03", amount: 50m);
        lab.Sellable("A15", amount: 0m);
        lab.Sellable("A20", amount: 30m);
        lab.Sellable("A23", amount: 0m);
        lab.Sellable("A24", amount: 10m);
        var context = V8Lab.Context();
        var source = new LabShoppingSource([context]);
        var store = new SnapshotStore(lab.Shop.Catalog);
        var rows = await lab.ShopAsync(context);
        string Reference(string code) => rows.Single(row => row.Candidate.VariantCode == code).OfferItemId!;

        await SubmitAsync(lab, source, store, Command(
            new AncillarySelectionLine(Reference("A03"), Options: new SelectionOptionsInput(Baggage: new BaggageSelectionOptions("BAG-1", 24m, null, null))),
            Wheelchair(Reference("A15"), Wchr),
            new AncillarySelectionLine(Reference("A20"), Options: new SelectionOptionsInput(AirportService: new AirportServiceSelectionOptions(null, 1))),
            new AncillarySelectionLine(Reference("A23"), Options: new SelectionOptionsInput(Priority: new PrioritySelectionOptions(true))),
            new AncillarySelectionLine(Reference("A24"), Options: new SelectionOptionsInput(Connectivity: new ConnectivitySelectionOptions(2)))));

        Assert.Equal(
            ["{\"bagRef\":\"BAG-1\",\"measuredWeightKg\":24}", "{\"assistanceSsrCode\":\"WCHR\"}", "{\"guestCount\":1}", "{\"optIn\":true}", "{\"deviceCount\":2}"],
            Assert.Single(lab.Shop.Catalog.Snapshots).Lines.Select(line => line.SelectedJson));
        Assert.Equal((1, 1), (source.Loads, store.Saves));
    }

    [Fact]
    public async Task P02_P03_P07_OneInvalidLineWritesNothing()
    {
        var (lab, context, references) = await V8Lab.WheelchairsAsync(1);
        var store = new SnapshotStore(lab.Shop.Catalog);
        var invented = new AssistedTravelSelectionOptions("XXXX", null, null);

        var second = await BusinessAssert.ThrowsAsync(
            InvalidOptions,
            422,
            () => SubmitAsync(lab, new LabShoppingSource([context]), store, Command(Wheelchair(references[0], Wchr), Wheelchair(references[1], invented))));
        var first = await BusinessAssert.ThrowsAsync(
            InvalidOptions,
            422,
            () => SubmitAsync(lab, new LabShoppingSource([context]), store, Command(Wheelchair(references[0], invented), Wheelchair(references[1], Wchr))));

        Assert.StartsWith("selections[1]: ", second.Message);
        Assert.StartsWith("selections[0]: ", first.Message);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
        Assert.Equal(0, store.Saves);
    }

    [Fact]
    public async Task P04_P16_FiftyLinesLoadOnceAndSaveOnce()
    {
        var (lab, context, references) = await V8Lab.WheelchairsAsync(25);
        var source = new LabShoppingSource([context]);
        var store = new SnapshotStore(lab.Shop.Catalog);

        await SubmitAsync(lab, source, store, Command(references.Select(reference => Wheelchair(reference, Wchr)).ToArray()));

        Assert.Equal(50, Assert.Single(lab.Shop.Catalog.Snapshots).Lines.Count);
        Assert.Equal((1, 1), (source.Loads, store.Saves));
    }

    [Fact]
    public void P05_TheValidatorRefusesTheShapeBeforeAnyWrite()
    {
        var validator = new ApiPriceAncillaryOffersCommandValidator();
        IReadOnlyList<string> Failures(ApiPriceAncillaryOffersCommand command)
            => validator.Validate(command).Errors.Select(error => error.PropertyName).Distinct().ToList();

        Assert.Contains("Selections", Failures(Command()));
        Assert.Contains("Selections.Count", Failures(Command(Enumerable.Range(0, 51).Select(index => Wheelchair($"r_{index}", Wchr)).ToArray())));
        Assert.Contains("Selections", Failures(Command(Wheelchair("r_1", Wchr), Wheelchair("r_1", Wchs))));
        Assert.Contains("Selections", Failures(Command(V8Lab.Direct("d_1"), V8Lab.Direct("d_1"))));
        Assert.Contains("Selections[0].OfferItemId", Failures(Command(Wheelchair(" ", Wchr))));
        Assert.Contains("Selections[0].OfferItemId", Failures(Command(V8Lab.Direct(" "))));
        Assert.Contains("Selections[0].OfferItemId", Failures(Command(new AncillarySelectionLine(Options: new SelectionOptionsInput(AssistedTravel: Wchr)))));
        Assert.Contains("Selections[0].Options", Failures(Command(new AncillarySelectionLine("r_1", Options: new SelectionOptionsInput()))));
        Assert.Contains(
            "Selections[0].Options",
            Failures(Command(new AncillarySelectionLine("r_1", Options: new SelectionOptionsInput(AssistedTravel: Wchr, Priority: new PrioritySelectionOptions(true))))));
        Assert.Contains("OfferId", Failures(Command(Wheelchair("r_1", Wchr)) with { OfferId = null }));
        Assert.Contains(
            "Selections[0].Options.Baggage.Dimensions.LengthCm",
            Failures(Command(new AncillarySelectionLine(
                "r_1",
                Options: new SelectionOptionsInput(Baggage: new BaggageSelectionOptions(null, null, new SelectedDimensionsInput(0m, 1m, 1m), null))))));
        Assert.Empty(Failures(Command(Wheelchair("r_1", Wchr), Wheelchair("r_2", Wchs), V8Lab.Direct("d_1"))));
    }

    [Fact]
    public async Task P06_RefusedReferencesWriteNothing()
    {
        var (lab, context, references) = await V8Lab.WheelchairsAsync(1);
        var store = new SnapshotStore(lab.Shop.Catalog);
        var otherOffer = context with { SourceIdentity = context.SourceIdentity with { TrustedSourceReference = "OFFER-2" } };

        var tampered = await BusinessAssert.ThrowsAsync(
            InvalidSignature,
            422,
            () => SubmitAsync(lab, new LabShoppingSource([context]), store, Command(Wheelchair(references[0], Wchr), Wheelchair(TokenTamper.Unsigned(references[1]), Wchr))));
        await BusinessAssert.ThrowsAsync(
            NotSellable,
            409,
            () => SubmitAsync(lab, new LabShoppingSource([otherOffer]), store, Command(Wheelchair(references[0], Wchr))));
        var otherFamily = await BusinessAssert.ThrowsAsync(
            NotFound,
            404,
            () => SubmitAsync(lab, new LabShoppingSource([context]), store, Command(new AncillarySelectionLine(references[0], Options: new SelectionOptionsInput(Baggage: new BaggageSelectionOptions("BAG-1", 24m, null, null))))));
        lab.Clock.Now = (await lab.Codec.ReadAsync(references[0], SelectionIdKind.Reference)).Selection!.ExpiresAtUtc;
        await BusinessAssert.ThrowsAsync(
            Expired,
            410,
            () => SubmitAsync(lab, new LabShoppingSource([context]), store, Command(Wheelchair(references[0], Wchr))));

        Assert.StartsWith("selections[1]: ", tampered.Message);
        Assert.StartsWith("selections[0]: ", otherFamily.Message);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
        Assert.Equal(0, store.Saves);
    }

    [Fact]
    public void P08_UnknownMembersAreRefusedByTheBinder()
    {
        const string line = "{\"offerItemId\":\"r_1\",\"options\":{\"assistedTravel\":{\"assistanceSsrCode\":\"WCHR\"}}}";

        var bound = HostJson.Bind<PriceAncillaryOffersRequest>($"{{\"offerId\":\"OFFER-1\",\"selections\":[{line},{{\"offerItemId\":\"d_1\",\"quantity\":2}}]}}").Selections;

        Assert.Equal(("r_1", "WCHR", null), (bound[0].OfferItemId, bound[0].Options!.AssistedTravel!.AssistanceSsrCode, bound[0].Options!.Baggage));
        Assert.Equal(("d_1", 2, null), (bound[1].OfferItemId, bound[1].Quantity, bound[1].Options));
        Assert.Throws<JsonException>(() => HostJson.Bind<PriceAncillaryOffersRequest>($"{{\"offerId\":\"OFFER-1\",\"price\":1,\"selections\":[{line}]}}"));
        Assert.Throws<JsonException>(() => HostJson.Bind<PriceAncillaryOffersRequest>(
            "{\"offerId\":\"OFFER-1\",\"selections\":[{\"offerItemId\":\"r_1\",\"options\":{\"meal\":{},\"assistedTravel\":{\"assistanceSsrCode\":\"WCHR\"}}}]}"));
        Assert.Throws<JsonException>(() => HostJson.Bind<PriceAncillaryOffersRequest>(
            "{\"offerId\":\"OFFER-1\",\"selections\":[{\"offerItemId\":\"r_1\",\"options\":{\"assistedTravel\":{\"assistanceSsrCode\":\"WCHR\",\"weightKg\":3}}}]}"));
        Assert.Throws<JsonException>(() => HostJson.Bind<PriceAncillaryOffersRequest>(
            "{\"offerId\":\"OFFER-1\",\"selections\":[{\"reference\":\"r_1\",\"assistedTravel\":{\"assistanceSsrCode\":\"WCHR\"}}]}"));
    }

    [Fact]
    public async Task P10_APetLineIsRefusedLikeBeforeBecausePetsNeedConfirmation()
    {
        var lab = new V8Lab();
        var definition = lab.Shop.Definition("A13", specification: spec => spec with { Pet = spec.Pet! with { MinAnimalAgeWeeks = null } });
        var provision = lab.Shop.Provision(definition, disposition: CommercialDisposition.Paid);
        var pricing = lab.Shop.Price(definition, provision, ShoppingLab.Rate(50m));
        var context = V8Lab.Context();
        var reference = lab.Codec.Issue(new SignedSelection(
            SelectionIdKind.Reference,
            pricing.Rates.Single().Id,
            provision.CoverageScope,
            "T1",
            "P1",
            null,
            null,
            SelectionBindings.Offer(V8Lab.Offer, V8Lab.Version),
            SelectionBindings.Sales(context),
            ShoppingLab.Now.AddMinutes(15)));
        var cat = new PetSelectionOptions(PetAnimalType.Cat, null, null, 6m, new SelectedDimensionsInput(40m, 30m, 20m), ["ENTRY_RULES_ACK"], null);

        var refused = await BusinessAssert.ThrowsAsync(
            NotSellable,
            409,
            () => SubmitAsync(lab, new LabShoppingSource([context]), new SnapshotStore(lab.Shop.Catalog), Command(new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(Pet: cat)))));

        Assert.StartsWith("selections[0]: ", refused.Message);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task P13_APriorityLineStillNeedsOptInTrue()
    {
        var lab = new V8Lab();
        lab.Sellable("A23", amount: 0m);
        var context = V8Lab.Context();
        var reference = Assert.Single(await lab.ShopAsync(context)).OfferItemId!;

        await BusinessAssert.ThrowsAsync(
            InvalidOptions,
            422,
            () => SubmitAsync(
                lab,
                new LabShoppingSource([context]),
                new SnapshotStore(lab.Shop.Catalog),
                Command(new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(Priority: new PrioritySelectionOptions(false))))));

        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task P17_AFailedSaveCommitsNothing()
    {
        var (lab, context, references) = await V8Lab.WheelchairsAsync(1);
        var store = new SnapshotStore(lab.Shop.Catalog) { FailsOnSave = true };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => SubmitAsync(lab, new LabShoppingSource([context]), store, Command(Wheelchair(references[0], Wchr), Wheelchair(references[1], Wchs))));

        Assert.Empty(lab.Shop.Catalog.Snapshots);
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public async Task P18_TheSameRequestTwiceGivesNewIds()
    {
        var (lab, context, references) = await V8Lab.WheelchairsAsync(1);
        var command = Command(Wheelchair(references[0], Wchr), Wheelchair(references[1], Wchs));

        var first = await SubmitAsync(lab, new LabShoppingSource([context]), new SnapshotStore(lab.Shop.Catalog), command);
        var second = await SubmitAsync(lab, new LabShoppingSource([context]), new SnapshotStore(lab.Shop.Catalog), command);

        Assert.NotEqual(first.SelectionId, second.SelectionId);
        Assert.Equal(2, lab.Shop.Catalog.Snapshots.Count);
    }

    [Fact]
    public async Task P20_E01_E02_E04_CardsSubmitAndResolveWorkTogether()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Travellers(2);
        var source = new LabShoppingSource([context]);
        var answer = AncillaryOffersMapper.Map(V8Lab.Snapshot(await lab.ShopAsync(context)));
        var direct = answer.Baggages.Single().Products.Single().Units[0].OfferItemId!;
        var wheelchairs = answer.AssistedTravel.Single().Products.Single().Units.Take(2).ToList();

        var submitted = await SubmitAsync(
            lab,
            source,
            new SnapshotStore(lab.Shop.Catalog),
            Command(
            [
                V8Lab.Direct(direct),
                .. wheelchairs.Select(unit => Wheelchair(
                    unit.OfferItemId!,
                    new AssistedTravelSelectionOptions(unit.Options!.AssistedTravel!["assistanceSsrCode"].Choices![0].Value, null, null)))
            ]));
        var resolved = await lab.Resolver.ResolveAsync(submitted.SelectionId);

        Assert.Equal(3, resolved.Count);
        Assert.Single(lab.Shop.Catalog.Snapshots);
        Assert.Equal(1, source.Loads);
    }

    [Fact]
    public async Task DirectAndOptionLinesMakeOneSelectionId()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);
        AncillaryOfferRow Row(string variant) => rows.Single(row => row.Candidate.VariantCode == variant);

        var result = await lab.SubmitAsync(
            [V8Lab.Direct(Row("A05").OfferItemId!), V8Lab.Direct(Row("A01").OfferItemId!, 2), Wheelchair(Row("A15").OfferItemId!, Wchs)],
            context);

        var lines = Assert.Single(lab.Shop.Catalog.Snapshots, snapshot => snapshot.Id == result.SelectionId).Lines.ToList();
        Assert.Equal([1, 2, 3], lines.Select(line => line.Sequence));
        Assert.Equal([1, 2, 1], lines.Select(line => line.Quantity));
        Assert.Equal([null, null, "{\"assistanceSsrCode\":\"WCHS\"}"], lines.Select(line => line.SelectedJson));
        Assert.Equal(V8Lab.Offer, Assert.Single(lab.Shop.Catalog.Snapshots).OfferId);
    }

    [Fact]
    public async Task TheSameUnitTwiceIsRefused()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        var context = V8Lab.Context();
        var first = Assert.Single(await lab.ShopAsync(context)).OfferItemId!;
        lab.Clock.Now = lab.Clock.Now.AddMinutes(1);
        var second = Assert.Single(await lab.ShopAsync(context)).OfferItemId!;

        var refused = await BusinessAssert.ThrowsAsync(
            NotSellable,
            409,
            () => lab.SubmitAsync([V8Lab.Direct(first), V8Lab.Direct(second, 2)], context));

        Assert.NotEqual(first, second);
        Assert.StartsWith("selections[1]: ", refused.Message);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task TheQuantityIsChosenInsideTheSignedRange()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        var context = V8Lab.Context();
        var selectionId = Assert.Single(await lab.ShopAsync(context)).OfferItemId!;

        var outside = await BusinessAssert.ThrowsAsync(InvalidOptions, 422, () => lab.SubmitAsync([V8Lab.Direct(selectionId, 3)], context));
        var two = await lab.SubmitAsync([V8Lab.Direct(selectionId, 2)], context);
        var minimum = await lab.SubmitAsync([V8Lab.Direct(selectionId)], context);

        Assert.StartsWith("selections[0]: ", outside.Message);
        Assert.Equal(2, Assert.Single(await lab.Resolver.ResolveAsync(two.SelectionId)).Quantity);
        Assert.Equal(1, Assert.Single(await lab.Resolver.ResolveAsync(minimum.SelectionId)).Quantity);
    }

    [Fact]
    public void AQuantityIsNotTakenWithOptions()
    {
        var validator = new ApiPriceAncillaryOffersCommandValidator();

        var errors = validator.Validate(Command(new AncillarySelectionLine("r_1", 2, new SelectionOptionsInput(AssistedTravel: Wchr)), V8Lab.Direct("d_1", 0))).Errors;

        Assert.Contains(errors, error => error.PropertyName == "Selections[0].Quantity");
        Assert.Contains(errors, error => error.PropertyName == "Selections[1].Quantity");
    }
}
