using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class Phase2Commands
{
    public static BackofficeRegisterServiceSubCodeCommand SubCodeL(int airlineId)
        => new(airlineId, "0BX", null, null, null, null, null, null);

    public static BackofficeRegisterServiceSubCodeCommand SubCodeGL(int airlineId)
        => new(airlineId, "XLG", "E", "LG", null, null, null, "LOUNGE");

    public static BackofficeDefineAncillaryProductCommand ProductL(int airlineId)
        => new(
            airlineId,
            "LNGTHR",
            AncillaryProductType.LoungeAccess,
            "Lounge access",
            null,
            AncillarySalesScope.TravellerSegment,
            new ProductQuantity(AncillaryQuantityUnit.Each, 1, 1),
            new ProductDocument(AncillaryDocumentType.EmdStandalone, "0BX"),
            new ProductCodes("F"),
            new ProductTerms(false, null, null, null, null),
            AncillaryInventoryControl.Unlimited,
            null,
            new ProductLounge([1]));

    public static BackofficeDefineAncillaryProductCommand ProductLG(int airlineId)
        => ProductL(airlineId) with
        {
            ProductRef = "LNGXLG",
            Quantity = new ProductQuantity(AncillaryQuantityUnit.Each, 1, 2),
            Document = new ProductDocument(AncillaryDocumentType.EmdStandalone, "XLG"),
            Lounge = new ProductLounge([1, 5])
        };

    public static BackofficeDefineAncillaryPriceRuleCommand RuleRL(int airlineId)
        => Phase1Commands.RuleR(airlineId) with { ProductRef = "LNGTHR", Lines = [Phase1Commands.Ancillary(20.00m)] };

    public static BackofficeDefineAncillaryPriceRuleCommand RuleRLG(int airlineId)
        => Phase1Commands.RuleR(airlineId) with { ProductRef = "LNGXLG", Lines = [Phase1Commands.Ancillary(15.00m)] };

    public static ServiceGetAncillaryQuoteQuery Golden(int airlineId, params QuoteSelection[] selections)
        => new(
            Phase1Commands.CurrencyId,
            Phase1Commands.AsOf,
            null,
            [new QuoteTraveller("T1", "ADT", ["F1", "F2"])],
            [
                new QuoteBound("B1", [Flight("F1", 100, 1, 2, "2026-10-10T08:00:00+03:30", airlineId)]),
                new QuoteBound("B2", [Flight("F2", 101, 2, 1, "2026-10-15T08:00:00+03:30", airlineId)])
            ],
            selections.Length == 0 ? null : selections,
            null);

    public static QuoteSelection LoungeSelection(string productRef, int productVersion, long priceRuleId, string flightRef = "F1", string? boundRef = null, int quantity = 1)
        => new(productRef, productVersion, priceRuleId, "T1", boundRef, flightRef, quantity);

    private static QuoteFlight Flight(string reference, long flightId, int originAirportId, int destinationAirportId, string departure, int airlineId)
        => new(reference, flightId, null, originAirportId, destinationAirportId, Phase1Commands.Instant(departure), airlineId, airlineId, null, null, null);
}
