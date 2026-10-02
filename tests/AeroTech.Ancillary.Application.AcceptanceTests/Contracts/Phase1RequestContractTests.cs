using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Contracts;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1RequestContractTests(TestDatabase database)
{
    private const string DefineProduct = """
        {
          "ownerAirlineId": 10,
          "productRef": "XBAG1",
          "type": "ExtraBaggage",
          "name": "First extra bag 23kg",
          "description": null,
          "salesScope": "TravellerBound",
          "quantity": { "unit": "Piece", "min": 1, "max": 1 },
          "document": { "type": "EmdAssociated", "rfisc": "0CC" },
          "codes": { "serviceTypeCode": "C" },
          "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null },
          "inventoryControl": "Unlimited",
          "baggage": { "pieces": 1, "weight": 23, "weightUnit": "Kg" }
        }
        """;

    private const string ChangeProduct = """
        {
          "name": "First extra bag 23kg",
          "description": null,
          "salesScope": "TravellerBound",
          "quantity": { "unit": "Piece", "min": 1, "max": 1 },
          "document": { "type": "EmdAssociated", "rfisc": "0CC" },
          "codes": { "serviceTypeCode": "C" },
          "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null },
          "inventoryControl": "Unlimited",
          "baggage": { "pieces": 1, "weight": 23, "weightUnit": "Kg" }
        }
        """;

    private const string DefinePriceRule = """
        {
          "ownerAirlineId": 10,
          "productRef": "XBAG1",
          "priority": 1,
          "currencyId": 978,
          "lines": [
            { "category": "Ancillary", "code": null, "name": null, "amount": 35.00 },
            { "category": "Tax", "code": "VAT", "name": "Value added tax", "amount": 3.50 }
          ],
          "salesFrom": null,
          "salesTo": null,
          "travelFrom": null,
          "travelTo": null,
          "conditions": { "passengerTypes": null, "originAirportIds": null, "destinationAirportIds": null }
        }
        """;

    private readonly AncillaryHarness _harness = new(database);

    [Theory]
    [InlineData("inventoryControl", "Quota")]
    [InlineData("inventoryControl", "SeatMap")]
    [InlineData("document.type", "None")]
    [InlineData("salesScope", "Traveller")]
    [InlineData("salesScope", "Order")]
    [InlineData("quantity.unit", "Kilogram")]
    [InlineData("type", "PetInCabin")]
    [InlineData("type", "Seat")]
    public void P1_C06_ValueOfALaterPhaseIsMalformedOnDefine(string member, string value)
    {
        var command = ApiJson.Deserialize<BackofficeDefineAncillaryProductCommand>(DefineProduct);

        Assert.Equal(Phase1Commands.ProductX(10), command);
        ApiJson.Malformed<BackofficeDefineAncillaryProductCommand>(ApiJson.With(DefineProduct, member, value));
    }

    [Theory]
    [InlineData("inventoryControl", "Quota")]
    [InlineData("inventoryControl", "SeatMap")]
    [InlineData("document.type", "None")]
    [InlineData("salesScope", "Traveller")]
    [InlineData("salesScope", "Order")]
    [InlineData("quantity.unit", "Kilogram")]
    public void P1_C06_ValueOfALaterPhaseIsMalformedOnChange(string member, string value)
    {
        var command = ApiJson.Deserialize<BackofficeChangeAncillaryProductCommand>(ChangeProduct);

        Assert.Equal(Phase1Commands.ChangeTo(0, Phase1Commands.ProductX(10)), command);
        ApiJson.Malformed<BackofficeChangeAncillaryProductCommand>(ApiJson.With(ChangeProduct, member, value));
    }

    [Theory]
    [InlineData("inventoryControl", 2)]
    [InlineData("inventoryControl", 3)]
    [InlineData("document.type", 1)]
    [InlineData("salesScope", 3)]
    [InlineData("salesScope", 4)]
    [InlineData("quantity.unit", 2)]
    [InlineData("type", 5)]
    [InlineData("type", 3)]
    public void P1_C06_NumericValueOfALaterPhaseIsRejectedByTheValidator(string member, int value)
    {
        var command = ApiJson.Deserialize<BackofficeDefineAncillaryProductCommand>(ApiJson.With(DefineProduct, member, value));

        ValidationAssert.Rejects(new BackofficeDefineAncillaryProductCommandValidator(), command);
    }

    [Fact]
    public void P1_C06_FeeLineOfALaterPhaseIsMalformed()
        => ApiJson.Malformed<BackofficeDefineAncillaryPriceRuleCommand>(DefinePriceRule.Replace("\"Tax\"", "\"Fee\""));

    [Fact]
    public async Task P1_C08_CopiedCodesSentInARequestAreIgnored()
    {
        var airline = _harness.AirlineId;
        var json = ApiJson.With(DefineProduct, "ownerAirlineId", airline);

        json = ApiJson.With(json, "document.rfic", "E");
        json = ApiJson.With(json, "codes.groupCode", "LG");
        json = ApiJson.With(json, "codes.subGroupCode", "XX");
        json = ApiJson.With(json, "codes.description1Code", "ZZ");
        json = ApiJson.With(json, "codes.description2Code", "ZZ");

        await _harness.RegisterAsync(Phase1Commands.SubCodeS(airline));

        var defined = await _harness.DefineAsync(ApiJson.Deserialize<BackofficeDefineAncillaryProductCommand>(json));
        var product = await _harness.GetProductAsync(defined.Id);

        Assert.Equal(("0CC", "C"), (product.Document.Rfisc, product.Document.Rfic));
        Assert.Equal(("C", "BG", null, "B1", null), (product.Codes.ServiceTypeCode, product.Codes.GroupCode, product.Codes.SubGroupCode, product.Codes.Description1Code, product.Codes.Description2Code));
    }

    [Fact]
    public void P1_R03_FeeLineIsMalformed()
    {
        var command = ApiJson.Deserialize<BackofficeDefineAncillaryPriceRuleCommand>(DefinePriceRule);

        Assert.Equal([AncillaryPriceLineCategory.Ancillary, AncillaryPriceLineCategory.Tax], command.Lines.Select(line => line.Category));
        ApiJson.Malformed<BackofficeDefineAncillaryPriceRuleCommand>(DefinePriceRule.Replace("\"Tax\"", "\"Fee\""));
    }
}
