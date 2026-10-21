using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2InlineOptionsTests
{
    private const string Field = "assistanceSsrCode";

    private sealed record Shown(string Surface, string OfferItemRef, string VariantCode, string Mode, bool RequiresDetails, IReadOnlyList<string> ReasonCodes, IReadOnlyList<ShownGroup> Groups);

    private sealed record ShownGroup(string FieldName, string SourceState, IReadOnlyList<string> Values, int TotalChoices, bool InlineChoicesIncluded);

    private sealed class CountingCatalog(InMemoryCatalog catalog) : IActiveAncillaryDefinitionReader, IActiveAncillaryProvisionReader, IActiveAncillaryPricingReader
    {
        public int DefinitionReads { get; private set; }

        public int ProvisionReads { get; private set; }

        public int PricingReads { get; private set; }

        public Task<IReadOnlyList<AncillaryServiceDefinition>> ListActiveAsync(int ownerAirlineId, CancellationToken cancellationToken = default)
        {
            DefinitionReads++;

            return catalog.ListActiveAsync(ownerAirlineId, cancellationToken);
        }

        public Task<IReadOnlyList<AncillaryProvision>> ListActiveAsync(IReadOnlyCollection<long> serviceDefinitionIds, long pointOfSaleId, CancellationToken cancellationToken = default)
        {
            ProvisionReads++;

            return catalog.ListActiveAsync(serviceDefinitionIds, pointOfSaleId, cancellationToken);
        }

        public Task<IReadOnlyList<AncillaryPricing>> ListActiveAsync(IReadOnlyCollection<long> ancillaryProvisionIds, CancellationToken cancellationToken = default)
        {
            PricingReads++;

            return catalog.ListActiveAsync(ancillaryProvisionIds, cancellationToken);
        }

        public Task<IReadOnlyList<AncillaryServiceDefinition>> FindActiveDefinitionsAsync(IReadOnlyCollection<long> definitionIds, CancellationToken cancellationToken = default)
        {
            DefinitionReads++;

            return catalog.FindActiveDefinitionsAsync(definitionIds, cancellationToken);
        }

        public Task<IReadOnlyList<AncillaryPricing>> FindActivePricingsAsync(IReadOnlyCollection<long> pricingIds, CancellationToken cancellationToken = default)
        {
            PricingReads++;

            return catalog.FindActivePricingsAsync(pricingIds, cancellationToken);
        }

        public Task<IReadOnlyList<AncillaryProvision>> FindActiveProvisionsAsync(IReadOnlyCollection<long> provisionIds, CancellationToken cancellationToken = default)
        {
            ProvisionReads++;

            return catalog.FindActiveProvisionsAsync(provisionIds, cancellationToken);
        }
    }

    private static ShoppingLab Lab(params string[] codes)
    {
        var lab = new ShoppingLab();

        foreach (var code in codes)
            lab.Sellable(code, 30m);

        return lab;
    }

    private static ApiOfferReferences References(AncillaryShoppingSnapshot snapshot)
    {
        var flights = snapshot.Offer.Bounds.SelectMany(bound => bound.Flights).ToList();

        return new ApiOfferReferences(
            snapshot.Offer,
            flights
                .SelectMany(flight => flight.Legs.SelectMany(leg => new[] { leg.OriginAirportId, leg.DestinationAirportId }))
                .Distinct()
                .ToDictionary(id => id, id => $"P{id:00}"),
            flights.SelectMany(flight => new[] { flight.MarketingAirlineId, flight.OperatingAirlineId }).Distinct().ToDictionary(id => id, id => $"L{id}"),
            new Dictionary<int, string> { [Eur] = "EUR" },
            new Dictionary<long, BaggageSpecificationArgs>());
    }

    private static IReadOnlyList<Shown> Surfaces(AncillaryShoppingSnapshot snapshot)
    {
        var backoffice = BackofficeAncillaryOffersMapper.Map(snapshot);
        var otaPanel = OtaPanelAncillaryOffersMapper.Map(snapshot);
        var api = ApiAncillaryOffersMapper.Map(snapshot, References(snapshot));

        return backoffice.Offers.Concat(backoffice.BlockedOffers)
            .Select(offer => new Shown(
                "backoffice",
                offer.OfferItemRef,
                offer.Service.VariantCode,
                offer.Interaction.Mode,
                offer.Interaction.RequiresDetails,
                offer.Interaction.ReasonCodes,
                offer.OptionGroups.Select(group => new ShownGroup(group.FieldName, group.SourceState, group.Choices.Select(choice => choice.Value).ToList(), group.TotalChoices, group.InlineChoicesIncluded)).ToList()))
            .Concat(otaPanel.TravellerGroups.SelectMany(group => group.FlightGroups).SelectMany(group => group.Services).Concat(otaPanel.OrderScopedServices)
                .Select(service => new Shown(
                    "otapanel",
                    service.OfferItemRef,
                    service.VariantCode,
                    service.Interaction.Mode,
                    service.Interaction.RequiresDetails,
                    service.Interaction.ReasonCodes,
                    service.OptionGroups.Select(group => new ShownGroup(group.FieldName, group.SourceState, group.Choices.Select(choice => choice.Value).ToList(), group.TotalChoices, group.InlineChoicesIncluded)).ToList())))
            .Concat(api.Included.Bags.Select(bag => (bag.Key, bag.Value.Ancillary)).Concat(api.Included.OtherServices.Select(other => (other.Key, other.Value.Ancillary)))
                .Select(entry => new Shown(
                    "api",
                    entry.Key,
                    entry.Ancillary.Service.VariantCode,
                    entry.Ancillary.Interaction.Mode,
                    entry.Ancillary.Interaction.RequiresDetails,
                    entry.Ancillary.Interaction.ReasonCodes,
                    entry.Ancillary.OptionGroups.Select(group => new ShownGroup(group.FieldName, group.SourceState, group.Choices.Select(choice => choice.Value).ToList(), group.TotalChoices, group.InlineChoicesIncluded)).ToList())))
            .ToList();
    }

    private static AncillaryShoppingSnapshot WithGroups(AncillaryShoppingSnapshot snapshot, AncillaryOfferItem item, IReadOnlyList<OfferOptionGroup> groups)
        => snapshot with
        {
            PointsOfSale = snapshot.PointsOfSale
                .Where(pointOfSale => pointOfSale.PointOfSaleId == item.PointOfSaleId)
                .Select(pointOfSale => pointOfSale with { Items = [item with { OptionGroups = groups }] })
                .ToList()
        };

    [Fact]
    public async Task B3_a_quantity_bag_a_wifi_plan_and_a_disability_assistance_ssr_are_inline_with_their_authored_choices_on_all_three_surfaces()
    {
        var shown = Surfaces(await SnapshotAsync(Lab("A01", "A24", "A16")));

        Assert.All(new[] { "A01", "A24", "A16" }, code => Assert.Equal(["api", "backoffice", "otapanel"], shown.Where(service => service.VariantCode == code).Select(service => service.Surface).Distinct().Order(StringComparer.Ordinal)));
        Assert.All(shown.Where(service => service.VariantCode == "A01"), service => Assert.Equal(("Inline", false, 0, 0), (service.Mode, service.RequiresDetails, service.ReasonCodes.Count, service.Groups.Count)));
        Assert.All(shown.Where(service => service.VariantCode == "A24"), service =>
        {
            var plan = Assert.Single(service.Groups);

            Assert.Equal(("Inline", false), (service.Mode, service.RequiresDetails));
            Assert.Equal(("planCode", "Known", 1, true), (plan.FieldName, plan.SourceState, plan.TotalChoices, plan.InlineChoicesIncluded));
            Assert.Equal(["WIFI_FULL_FLIGHT"], plan.Values);
        });
        Assert.All(shown.Where(service => service.VariantCode == "A16"), service =>
        {
            var ssr = Assert.Single(service.Groups);

            Assert.Equal(("Inline", false), (service.Mode, service.RequiresDetails));
            Assert.Equal((Field, "Known", 3, true), (ssr.FieldName, ssr.SourceState, ssr.TotalChoices, ssr.InlineChoicesIncluded));
            Assert.Equal(["BLND", "DEAF", "DPNA"], ssr.Values);
        });
    }

    [Theory]
    [InlineData("Known", 0, true, "DetailsOnDemand", "NoOptionAvailable")]
    [InlineData("Known", 1, true, "Inline", null)]
    [InlineData("Known", 12, true, "Inline", null)]
    [InlineData("Known", 13, false, "DetailsOnDemand", "OptionsDeferred")]
    [InlineData("Unknown", 0, false, "DetailsOnDemand", "OptionSourceUnknown")]
    public async Task B3_Q8_twelve_choices_stay_inline_thirteen_are_deferred_and_an_unknown_source_is_never_known_empty(
        string sourceState,
        int count,
        bool included,
        string mode,
        string? reason)
    {
        var snapshot = await SnapshotAsync(Lab("A16"));
        var item = snapshot.Items.First(candidate => candidate.Candidate.VariantCode == "A16");
        var choices = Enumerable.Range(1, count).Select(index => new OfferOptionChoice($"SSR{index:00}", $"SSR{index:00}")).ToList();
        var groups = new[] { new OfferOptionGroup(Field, Enum.Parse<OptionSourceState>(sourceState), choices) };

        var shown = Surfaces(WithGroups(snapshot, item, groups));

        Assert.Equal(["api", "backoffice", "otapanel"], shown.Select(service => service.Surface).Order(StringComparer.Ordinal));
        Assert.All(shown, service =>
        {
            var group = Assert.Single(service.Groups);

            Assert.Equal(item.OfferItemRef, service.OfferItemRef);
            Assert.Equal((Field, sourceState, count, included), (group.FieldName, group.SourceState, group.TotalChoices, group.InlineChoicesIncluded));
            Assert.Equal(included ? choices.Select(choice => choice.Value) : [], group.Values);
            Assert.Equal((mode, mode == "DetailsOnDemand"), (service.Mode, service.RequiresDetails));
            Assert.Equal(reason is null ? Array.Empty<string>() : [reason], service.ReasonCodes);
        });
    }

    [Fact]
    public async Task B3_a_quote_upgrade_names_the_quote_and_its_unknown_source_and_a_seat_in_the_generic_list_is_never_inline()
    {
        var shown = Surfaces(await SnapshotAsync(Lab("A10", "A07")));

        Assert.All(shown.Where(service => service.VariantCode == "A10"), service =>
        {
            Assert.Equal(("DetailsOnDemand", true), (service.Mode, service.RequiresDetails));
            Assert.Equal(["QuoteRequired", "OptionSourceUnknown"], service.ReasonCodes);
            Assert.Contains(service.Groups, group => group is { FieldName: "quoteRef", SourceState: "Unknown", Values.Count: 0, TotalChoices: 0, InlineChoicesIncluded: false });
        });
        Assert.Contains(shown, service => service.VariantCode == "A10");
        Assert.All(shown.Where(service => service.VariantCode == "A07"), service => Assert.Equal("DetailsOnDemand", service.Mode));
        Assert.Contains(shown, service => service.VariantCode == "A07");
    }

    [Fact]
    public async Task B3_twenty_list_items_read_the_catalog_once_need_no_details_call_and_one_click_reads_details_once()
    {
        var lab = Lab("A01", "A05", "A06", "A11", "A16", "A23", "A24");
        var list = new CountingCatalog(lab.Catalog);
        var details = new CountingCatalog(lab.Catalog);
        var offers = new GetAncillaryOffersService(await SourceAsync(), lab.Engine, list, list);
        var detailService = new GetAncillaryOfferDetailService(offers, details, details, details);

        var snapshot = await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Sale);
        var shown = Surfaces(snapshot);

        Assert.InRange(snapshot.Items.Count(), 20, int.MaxValue);
        Assert.Equal((1, 1, 0), (list.DefinitionReads, list.ProvisionReads, list.PricingReads));
        Assert.Equal((0, 0, 0), (details.DefinitionReads, details.ProvisionReads, details.PricingReads));
        Assert.All(shown.Where(service => service.Mode == "Inline"), service => Assert.All(service.Groups, group => Assert.True(group.InlineChoicesIncluded)));

        var clicked = snapshot.Items.First(item => item.Candidate.VariantCode == "A06");
        var detail = await detailService.ExecuteAsync(new OfferQuery(OfferWire.OfferId, clicked.OfferItemRef), Sale);

        Assert.Contains(shown, service => service.OfferItemRef == clicked.OfferItemRef && service.Mode == "DetailsOnDemand");
        Assert.Equal(clicked.OfferItemRef, detail.Item.OfferItemRef);
        Assert.Equal((1, 1, 1), (details.DefinitionReads, details.ProvisionReads, details.PricingReads));
    }
}
