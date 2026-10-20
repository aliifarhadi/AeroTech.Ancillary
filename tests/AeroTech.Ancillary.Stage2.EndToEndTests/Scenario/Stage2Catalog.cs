using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public static class Stage2Catalog
{
    public const int Irr = 70;
    public const decimal BagPrice = 5_000_000m;
    public const decimal BikePrice = 3_000_000m;
    public const decimal WifiPrice = 900_000m;
    public const decimal SeatPrice = 1_500_000m;
    public const decimal ExitRowPrice = 2_000_000m;
    public const decimal EuroBagPrice = 40m;
    public const decimal EuroToRial = 125_000m;
    public const string ProviderKey = "Ancillary";

    public static ShoppingLab Standard(bool exitRow = false, bool wifiTicketFee = false, bool euroBag = false, bool orderScopedWifi = false)
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        var bag = lab.Definition("A01");

        if (euroBag)
        {
            lab.Catalog.RatesOfExchange.Add(new PublishedRateOfExchange("ROE-1", Eur, Irr, EuroToRial, 0, 1d));
            lab.Price(bag, lab.Provision(bag), Rate(EuroBagPrice));
        }
        else
        {
            lab.Price(bag, lab.Provision(bag), Rate(BagPrice, Irr));
        }

        var bike = lab.Definition("A06");
        var wifi = lab.Definition("A24");

        lab.Price(bike, lab.Provision(bike), Rate(BikePrice, Irr));
        lab.Price(
            wifi,
            orderScopedWifi ? lab.Provision(wifi, scope: ServiceCoverageScope.Order, stage: PurchaseStage.Both) : lab.Provision(wifi),
            wifiTicketFee ? Rate(WifiPrice, Irr, components: [Fee("TKT", 100_000m, FeeApplicationUnit.Ticket, Irr)]) : Rate(WifiPrice, Irr));

        if (exitRow)
        {
            var exit = lab.Definition("A08");

            lab.Price(exit, lab.Provision(exit, stage: PurchaseStage.Both), Rate(ExitRowPrice, Irr));
        }
        else
        {
            var seat = lab.Definition("A07");

            lab.Price(seat, lab.Provision(seat), Rate(SeatPrice, Irr));
        }

        return lab;
    }

    public static void ChangeEuroRate(ShoppingLab lab, decimal rate)
    {
        lab.Catalog.RatesOfExchange.Clear();
        lab.Catalog.RatesOfExchange.Add(new PublishedRateOfExchange("ROE-2", Eur, Irr, rate, 0, 1d));
    }
}
