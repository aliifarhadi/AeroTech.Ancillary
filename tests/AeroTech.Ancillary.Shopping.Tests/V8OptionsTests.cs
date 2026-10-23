using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V8OptionsTests
{
    private const int InvalidOptions = 16821;

    private static async Task<(V8Lab Lab, AncillaryShoppingContext Context, string Reference)> ReferenceAsync(string code, decimal amount = 0m)
    {
        var lab = new V8Lab();
        lab.Sellable(code, amount: amount);
        var context = V8Lab.Context();
        var row = Assert.Single(await lab.ShopAsync(context));

        return (lab, context, row.Reference!);
    }

    private static async Task<SelectionOptionsForm> FormAsync(V8Lab lab, AncillaryShoppingContext context, string reference, AncillaryProfile family)
        => await lab.Typed.FormAsync(await lab.Typed.ReadReferenceAsync(reference), family, [context]);

    private static PetSelectionOptions Pet(decimal weight)
        => new(PetAnimalType.Cat, null, null, weight, new SelectedDimensionsInput(40m, 30m, 20m), ["ENTRY_RULES_ACK"], null);

    [Fact]
    public async Task O01_WheelchairFormListsTheAuthoredSsrCodes()
    {
        var (lab, context, reference) = await ReferenceAsync("A15");

        var field = Assert.Single((await FormAsync(lab, context, reference, AncillaryProfile.AssistedTravel)).Fields);

        Assert.Equal(("assistanceSsrCode", SelectionControls.Select, true), (field.Name, field.Control, field.Required));
        Assert.Equal(["WCHC", "WCHR", "WCHS"], field.Choices!.Select(choice => choice.Value));
    }

    [Fact]
    public async Task O02_AllowedSsrCodeStoresOneSnapshot()
    {
        var (lab, context, reference) = await ReferenceAsync("A15");

        var result = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, new AssistedTravelSelectionOptions("WCHS", null, null), context);

        var snapshot = Assert.Single(lab.Shop.Catalog.Snapshots);
        Assert.Equal(result.SelectionId, snapshot.Id);
        Assert.Matches("^s_[A-Za-z0-9_-]{22}$", result.SelectionId);
        Assert.Equal("{\"assistanceSsrCode\":\"WCHS\"}", snapshot.SelectedJson);
        Assert.Equal(1, snapshot.Quantity);
    }

    [Fact]
    public async Task O03_InventedSsrCodeIsRefusedWithoutSnapshot()
    {
        var (lab, context, reference) = await ReferenceAsync("A15");

        await BusinessAssert.ThrowsAsync(
            InvalidOptions,
            422,
            () => lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, new AssistedTravelSelectionOptions("XXXX", null, null), context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    private static (V8Lab Lab, AncillaryShoppingContext Context, string Reference) SignedPet()
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
            V8Lab.Offer,
            SelectionBindings.Offer(V8Lab.Offer, V8Lab.Version),
            SelectionBindings.Sales(context),
            ShoppingLab.Now.AddMinutes(15)));

        return (lab, context, reference);
    }

    [Fact]
    public async Task O04_PetNeedsSupplierConfirmationSoItGetsNoReference()
    {
        var (lab, context, reference) = SignedPet();

        Assert.Empty(await lab.ShopAsync(context));
        await BusinessAssert.ThrowsAsync(16820, 409, () => lab.SubmitAsync(reference, AncillaryProfile.Pet, Pet(6m), context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task O05_PetAboveTheMaximumIsRefusedWithoutSnapshot()
    {
        var (lab, context, reference) = SignedPet();

        await BusinessAssert.ThrowsAsync(InvalidOptions, 422, () => lab.SubmitAsync(reference, AncillaryProfile.Pet, Pet(9m), context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task O06_EquipmentDimensionsAreAskedOnlyWhenTheProductLimitsThem()
    {
        var (lab, context, reference) = await ReferenceAsync("A06", amount: 30m);
        var unlimited = new V8Lab();
        unlimited.Sellable("A06", amount: 30m, specification: spec => spec with
        {
            Baggage = spec.Baggage! with { MaxSize = null, MaxLinearSumCm = null, MaxKgPerPiece = null }
        });

        var fields = (await FormAsync(lab, context, reference, AncillaryProfile.Baggage)).Fields;
        var direct = Assert.Single(await unlimited.ShopAsync(V8Lab.Context()));

        Assert.Equal([("dimensions", true), ("weightKg", true)], fields.Select(field => (field.Name, field.Required)));
        Assert.StartsWith("d_", direct.SelectionId);
        Assert.Null(direct.Reference);
    }

    [Fact]
    public async Task O07_FixedMenuWithQuantityHasSignedOptionsAndNoSnapshot()
    {
        var lab = new V8Lab();
        lab.Sellable("A12", amount: 12m, maxQuantity: 2);

        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context()));

        Assert.Equal([1, 2], row.QuantityOptions!.Select(option => option.Quantity));
        Assert.All(row.QuantityOptions!, option => Assert.StartsWith("d_", option.SelectionId));
        Assert.Null(row.Reference);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task O08_MenuIsFixedByTheDefinitionSoItIsNeverAskedAgain()
    {
        var lab = new V8Lab();
        var (definition, provision, _) = lab.Sellable("A12", amount: 12m);

        Assert.Empty(TypedSelectionForm.Fields(definition, provision, [], string.Empty));
        Assert.Null(Assert.Single(await lab.ShopAsync(V8Lab.Context())).Reference);
    }

    [Fact]
    public async Task O09_OptInMissingOrFalseMakesNoSelection()
    {
        var (lab, context, reference) = await ReferenceAsync("A23");

        await BusinessAssert.ThrowsAsync(InvalidOptions, 422, () => lab.SubmitAsync(reference, AncillaryProfile.Priority, new PrioritySelectionOptions(null), context));
        await BusinessAssert.ThrowsAsync(InvalidOptions, 422, () => lab.SubmitAsync(reference, AncillaryProfile.Priority, new PrioritySelectionOptions(false), context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task O10_OptInTrueStoresASnapshot()
    {
        var (lab, context, reference) = await ReferenceAsync("A23");

        var result = await lab.SubmitAsync(reference, AncillaryProfile.Priority, new PrioritySelectionOptions(true), context);

        Assert.Equal("{\"optIn\":true}", Assert.Single(lab.Shop.Catalog.Snapshots, snapshot => snapshot.Id == result.SelectionId).SelectedJson);
    }

    [Fact]
    public async Task O11_MedicalEquipmentHasNoFormAndNoReference()
    {
        var lab = new V8Lab();
        var definition = lab.Shop.Definition("A17");
        var provision = lab.Shop.Provision(definition, disposition: CommercialDisposition.Paid);
        lab.Shop.Price(definition, provision, ShoppingLab.Rate(80m));

        Assert.Empty(TypedSelectionForm.Fields(definition, provision, [], string.Empty));
        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task ExclusiveMinimumsFollowTheEngineRules()
    {
        var (lab, context, reference) = await ReferenceAsync("A03", 50m);
        var overweight = Assert.Single((await FormAsync(lab, context, reference, AncillaryProfile.Baggage)).Fields, field => field.Name == "measuredWeightKg");
        var equipment = new V8Lab();
        var (bike, bikeRule, _) = equipment.Sellable("A06", amount: 30m);
        var pets = new V8Lab();
        var pet = pets.Shop.Definition("A13");
        var petRule = pets.Shop.Provision(pet, disposition: CommercialDisposition.Paid);

        Assert.Equal((23m, (decimal?)null, 32m), (overweight.ExclusiveMin, overweight.Min, overweight.Max));
        Assert.Equal(0m, Assert.Single(TypedSelectionForm.Fields(bike, bikeRule, [], string.Empty), field => field.Name == "weightKg").ExclusiveMin);
        Assert.Equal(0m, Assert.Single(TypedSelectionForm.Fields(pet, petRule, [], string.Empty), field => field.Name == "combinedWeightKg").ExclusiveMin);
        await BusinessAssert.ThrowsAsync(
            InvalidOptions,
            422,
            () => lab.SubmitAsync(reference, AncillaryProfile.Baggage, new BaggageSelectionOptions("BAG-1", 23m, null, null), context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
        await lab.SubmitAsync(reference, AncillaryProfile.Baggage, new BaggageSelectionOptions("BAG-1", 24m, null, null), context);
        Assert.Single(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task O12_SamePostTwiceGivesTwoValidIds()
    {
        var (lab, context, reference) = await ReferenceAsync("A15");
        var options = new AssistedTravelSelectionOptions("WCHR", null, null);

        var first = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, options, context);
        var second = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, options, context);

        Assert.NotEqual(first.SelectionId, second.SelectionId);
        Assert.Equal(2, (await lab.Resolver.ResolveAsync([first.SelectionId, second.SelectionId])).Count);
    }
}
