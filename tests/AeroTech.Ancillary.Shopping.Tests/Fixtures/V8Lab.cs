using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifySelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Resolution;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class V8Lab
{
    public const string Offer = "OFFER-1";
    public const string Version = "v1";

    private const string SigningKey = "v8-closure-fixture-signing-key-0123456789";

    private static readonly ShoppingFlight F1 = Trip.Flight("F1", 101);
    private static readonly ShoppingFlight F2 = Trip.Flight("F2", 102);

    public ShoppingLab Shop { get; } = new();

    public FixedClock Clock { get; } = new() { Now = ShoppingLab.Now };

    public SelectionIdOptions Options { get; } = new(SigningKey, TimeSpan.FromMinutes(15));

    public SelectionIdCodec Codec => new(Options, Clock);

    public SelectionSigner Signer => new(Codec, Options, Clock);

    public AncillaryOfferListing Listing => new(Shop.Engine, Shop.Catalog, Shop.Catalog, Signer);

    public SelectionProofService Proofs => new(Codec, Shop.Catalog, Shop.Catalog, Shop.Catalog, Shop.Engine, Options, Clock);

    public SelectionResolver Resolver => new(Shop.Catalog, Shop.Catalog, Shop.Catalog, Shop.Catalog, Clock);

    public static AncillaryShoppingContext Context(
        IReadOnlyList<ShoppingFlight>? flights = null,
        IReadOnlyList<ShoppingPortion>? portions = null,
        IReadOnlyList<ShoppingTraveller>? travellers = null,
        long pointOfSaleId = ShoppingLab.Pos)
    {
        var context = Trip.Context(flights, portions, travellers, pointOfSaleId: pointOfSaleId);

        return context with { SourceIdentity = context.SourceIdentity with { ValidUntilUtc = ShoppingLab.Now.AddMinutes(30) } };
    }

    public static AncillaryShoppingContext Travellers(int count)
        => Context([F1, F2], [Trip.Portion("P1", 1, F1, F2)], Enumerable.Range(1, count).Select(index => Trip.Adult($"T{index}")).ToList());

    public static async Task<(V8Lab Lab, AncillaryShoppingContext Context, IReadOnlyList<string> References)> WheelchairsAsync(int travellers)
    {
        var lab = new V8Lab();
        lab.Sellable("A15", amount: 0m);
        var context = Travellers(travellers);
        var rows = await lab.ShopAsync(context);

        return (lab, context, rows.Select(row => row.OfferItemId!).ToList());
    }

    public (AncillaryServiceDefinition Definition, AncillaryProvision Provision, AncillaryPricing Pricing) Sellable(
        string code,
        decimal amount = 100m,
        Func<ServiceSpecificationInput, ServiceSpecificationInput>? specification = null,
        ServiceCoverageScope? scope = null,
        int? maxQuantity = null,
        PassengerTypeCode? passengerType = null,
        ProvisionRulesArgs? rules = null,
        long pos = ShoppingLab.Pos)
    {
        var definition = Shop.Definition(code, specification: specification, confirmation: ConfirmationRequirement.Immediate);
        var provision = Shop.Provision(definition, scope: scope, disposition: CommercialDisposition.Paid, maxQuantity: maxQuantity, rules: rules, pos: pos);
        var pricing = Shop.Price(definition, provision, ShoppingLab.Rate(amount, passengerType: passengerType));

        return (definition, provision, pricing);
    }

    public Task<IReadOnlyList<AncillaryOfferRow>> ShopAsync(params AncillaryShoppingContext[] contexts)
        => Listing.ListAsync(contexts, OfferFamilyFilters.Services, null);

    public static AncillaryOffersSnapshot Snapshot(IReadOnlyList<AncillaryOfferRow> rows) => new(Offer, ShoppingLab.Eur, rows);

    public SelectionVerifier Verifier(LabShoppingSource source)
    {
        var scopes = new LabSalesScopes();

        return new SelectionVerifier(scopes, scopes, scopes, source, Proofs);
    }

    public GetPricedAncillaryOffersService Selected(LabShoppingSource source) => new(Resolver, new GetAncillaryOffersService(source, Listing));

    public PriceAncillaryOffersService Submissions(LabShoppingSource source, SnapshotStore store) => new(Verifier(source), store, store, Clock);

    public Task<SelectionSnapshotResult> SubmitAsync(IReadOnlyList<AncillarySelectionLine> lines, params AncillaryShoppingContext[] contexts)
        => Submissions(new LabShoppingSource(contexts), new SnapshotStore(Shop.Catalog)).SubmitAsync(new ApiPriceAncillaryOffersCommand(Offer, lines));

    public Task<SelectionSnapshotResult> SubmitAsync(string reference, AncillaryProfile family, ISelectionOptions options, params AncillaryShoppingContext[] contexts)
        => SubmitAsync([Line(reference, family, options)], contexts);

    public static AncillarySelectionLine Direct(string offerItemId, int? quantity = null) => new(offerItemId, quantity);

    public static AncillarySelectionLine Line(string reference, AncillaryProfile family, ISelectionOptions options)
        => family switch
        {
            AncillaryProfile.Baggage => new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(Baggage: (BaggageSelectionOptions)options)),
            AncillaryProfile.Pet => new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(Pet: (PetSelectionOptions)options)),
            AncillaryProfile.AssistedTravel => new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(AssistedTravel: (AssistedTravelSelectionOptions)options)),
            AncillaryProfile.AirportService => new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(AirportService: (AirportServiceSelectionOptions)options)),
            AncillaryProfile.Priority => new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(Priority: (PrioritySelectionOptions)options)),
            AncillaryProfile.Connectivity => new AncillarySelectionLine(reference, Options: new SelectionOptionsInput(Connectivity: (ConnectivitySelectionOptions)options)),
            _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
        };
}
