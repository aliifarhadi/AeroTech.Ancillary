using System.Globalization;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class Phase1Commands
{
    public const int CurrencyId = 978;

    public static readonly DateTimeOffset AsOf = Instant("2026-10-01T14:00:00+00:00");

    public static DateTimeOffset Instant(string value) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);

    public static BackofficeRegisterServiceSubCodeCommand SubCodeS(int airlineId)
        => new(airlineId, "0CC", null, null, null, null, null, null);

    public static BackofficeRegisterServiceSubCodeCommand SubCodeG(int airlineId)
        => new(airlineId, "XBG", "C", "BG", null, null, null, "EXTRA BAG");

    public static BackofficeDefineAncillaryProductCommand ProductX(int airlineId)
        => new(
            airlineId,
            "XBAG1",
            AncillaryProductType.ExtraBaggage,
            "First extra bag 23kg",
            null,
            AncillarySalesScope.TravellerBound,
            new ProductQuantity(AncillaryQuantityUnit.Piece, 1, 1),
            new ProductDocument(AncillaryDocumentType.EmdAssociated, "0CC"),
            new ProductCodes("C"),
            new ProductTerms(false, null, null, null, null),
            AncillaryInventoryControl.Unlimited,
            new ProductBaggage(1, 23m, AncillaryWeightUnit.Kg),
            null);

    public static BackofficeDefineAncillaryProductCommand ProductG(int airlineId)
        => ProductX(airlineId) with
        {
            ProductRef = "XBAGG",
            Name = "Extra bag 23kg",
            Quantity = new ProductQuantity(AncillaryQuantityUnit.Piece, 1, 2),
            Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "XBG")
        };

    public static BackofficeChangeAncillaryProductCommand ChangeTo(long productId, BackofficeDefineAncillaryProductCommand content)
        => new(
            productId,
            content.Name,
            content.Description,
            content.SalesScope,
            content.Quantity,
            content.Document,
            content.Codes,
            content.Terms,
            content.InventoryControl,
            content.Baggage,
            content.Lounge);

    public static PriceRuleLine Ancillary(decimal amount, string? name = null)
        => new(AncillaryPriceLineCategory.Ancillary, null, name, amount);

    public static PriceRuleLine Tax(string? code, decimal amount, string? name = null)
        => new(AncillaryPriceLineCategory.Tax, code, name, amount);

    public static BackofficeDefineAncillaryPriceRuleCommand RuleR(int airlineId)
        => new(
            airlineId,
            "XBAG1",
            1,
            CurrencyId,
            [Ancillary(35.00m), Tax("VAT", 3.50m, "Value added tax")],
            null,
            null,
            null,
            null,
            new PriceRuleConditionsInput(null, null, null));

    public static BackofficeDefineAncillaryPriceRuleCommand RuleRG(int airlineId)
        => RuleR(airlineId) with { ProductRef = "XBAGG", Lines = [Ancillary(30.00m)] };

    public static BackofficeChangeAncillaryPriceRuleCommand ChangeTo(long priceRuleId, BackofficeDefineAncillaryPriceRuleCommand content)
        => new(
            priceRuleId,
            content.Priority,
            content.CurrencyId,
            content.Lines,
            content.SalesFrom,
            content.SalesTo,
            content.TravelFrom,
            content.TravelTo,
            content.Conditions);

    public static ServiceGetAncillaryQuoteQuery Golden(int airlineId, params QuoteSelection[] selections)
        => new(
            CurrencyId,
            AsOf,
            null,
            [new QuoteTraveller("T1", "ADT", ["F1"])],
            [new QuoteBound("B1", [new QuoteFlight("F1", 100, null, 1, 2, Instant("2026-10-10T08:00:00+03:30"), airlineId, airlineId, null, null, null)])],
            selections.Length == 0 ? null : selections,
            null);

    public static QuoteSelection Selection(string productRef, int productVersion, long priceRuleId, int quantity = 1)
        => new(productRef, productVersion, priceRuleId, "T1", "B1", null, quantity);
}
