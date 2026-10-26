using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V9InlineOptionsTests
{
    private static readonly string[] VariantsWithOptions = ["A03", "A04", "A06", "A13", "A14", "A15", "A16", "A18", "A20", "A22", "A23", "A24"];

    private static ShoppingTraveller Infant(string reference, string adultRef)
        => new()
        {
            TravellerRef = reference,
            PassengerTypeCode = PassengerTypeCode.INF,
            DateOfBirth = new DateOnly(2026, 3, 1),
            AssociatedAdultRef = adultRef,
            StableTravellerIdentity = $"PAX-{reference}"
        };

    private static async Task<AncillaryOffersDto> OffersAsync(V8Lab lab, AncillaryShoppingContext context)
        => AncillaryOffersMapper.Map(V8Lab.Snapshot(await lab.ShopAsync(context)));

    private static IEnumerable<string> MemberNames(Type type) => type.GetProperties().Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name));

    [Fact]
    public async Task OptionUnitsCarryTheirFormUnderTheMemberOfTheirFamily()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A15", amount: 0m);

        var answer = await OffersAsync(lab, V8Lab.Context());
        var wheelchair = answer.AssistedTravel.Single().Products.Single().Units.Single();
        var bag = answer.Baggages.Single().Products.Single().Units.Single();

        Assert.Equal(["WCHC", "WCHR", "WCHS"], wheelchair.Options!.AssistedTravel!["assistanceSsrCode"].Choices!.Select(choice => choice.Value));
        Assert.Contains(
            "\"options\":{\"assistedTravel\":{\"assistanceSsrCode\":{\"control\":\"select\",\"required\":true,\"choices\":[",
            JsonSerializer.Serialize(wheelchair, HostJson.Options));
        Assert.Null(bag.Options);
        Assert.DoesNotContain("\"options\"", JsonSerializer.Serialize(bag, HostJson.Options));
    }

    [Fact]
    public async Task DimensionsDescribeTheirParts()
    {
        var lab = new V8Lab();
        lab.Sellable("A06", amount: 30m);

        var unit = (await OffersAsync(lab, V8Lab.Context())).Baggages.Single().Products.Single().Units.Single();
        var dimensions = unit.Options!.Baggage!["dimensions"];

        Assert.Equal((SelectionControls.Dimensions, true), (dimensions.Control, dimensions.Required));
        Assert.Equal(["lengthCm", "widthCm", "heightCm"], dimensions.Fields!.Keys);
        Assert.All(dimensions.Fields.Values, part => Assert.Equal((SelectionControls.Number, true, "cm", (decimal?)0m), (part.Control, part.Required, part.Unit, part.ExclusiveMin)));
        Assert.Equal(MemberNames(typeof(SelectedDimensionsInput)), dimensions.Fields.Keys);
    }

    [Fact]
    public void TheUnitAndTheSubmitLineUseTheSameNames()
    {
        Assert.Equal(
            typeof(SelectionOptionsInput).GetProperties().Select(property => property.Name),
            typeof(SelectionOptionsDto<object>).GetProperties().Select(property => property.Name));
        Assert.Equal([nameof(AncillarySelectionLine.OfferItemId), nameof(AncillarySelectionLine.Quantity), nameof(AncillarySelectionLine.Options)], typeof(AncillarySelectionLine).GetProperties().Select(property => property.Name));
        Assert.Contains(nameof(AncillaryUnitOfferDto.OfferItemId), typeof(AncillaryUnitOfferDto).GetProperties().Select(property => property.Name));
        Assert.Contains(nameof(AncillaryUnitOfferDto.Options), typeof(AncillaryUnitOfferDto).GetProperties().Select(property => property.Name));
    }

    [Fact]
    public void EveryFieldOfAFormIsAMemberOfItsSubmitOptions()
    {
        var lab = new V8Lab();
        var travellers = new[] { Trip.Adult("T1"), Infant("I1", "T1") };
        var forms = VariantsWithOptions.Select(code =>
        {
            var definition = lab.Shop.Definition(code);

            return (Code: code, Definition: definition, Fields: TypedSelectionForm.Fields(definition, lab.Shop.Provision(definition), travellers, "T1"));
        }).ToList();

        Assert.Empty(forms.Where(form => form.Fields.Count == 0).Select(form => form.Code));
        Assert.All(forms, form =>
        {
            var options = typeof(SelectionOptionsInput).GetProperty($"{form.Definition.Profile}")!.PropertyType;

            Assert.All(form.Fields, field => Assert.Contains(field.Name, MemberNames(options)));
        });
    }

    [Fact]
    public async Task EachUnitKeepsItsOwnChoices()
    {
        var lab = new V8Lab();
        lab.Sellable("A18", amount: 0m);
        var context = V8Lab.Context(travellers: [Trip.Adult("T1"), Infant("I1", "T1"), Trip.Adult("T2"), Infant("I2", "T2")]);

        var units = (await OffersAsync(lab, context)).AssistedTravel.Single().Products.Single().Units
            .Where(unit => unit.Coverage.TravellerRefs.Single() is "T1" or "T2");

        Assert.Equal(
            [["I1"], ["I2"]],
            units.Select(unit => unit.Options!.AssistedTravel!["infantRef"].Choices!.Select(choice => choice.Value).ToArray()).ToArray());
    }

    [Fact]
    public async Task BackofficeUnitsCarryTheSameForm()
    {
        var lab = new V8Lab();
        lab.Sellable("A15", amount: 0m);
        var snapshot = V8Lab.Snapshot(await lab.ShopAsync(V8Lab.Context()));

        var backoffice = BackofficeAncillaryOffersMapper.Map(snapshot).AssistedTravel.Single().Products.Single().Units.Single();
        var agency = AncillaryOffersMapper.Map(snapshot).AssistedTravel.Single().Products.Single().Units.Single();

        Assert.Equal(JsonSerializer.Serialize(agency.Options, HostJson.Options), JsonSerializer.Serialize(backoffice.Options, HostJson.Options));
    }
}
