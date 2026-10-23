using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.Api;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
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

    public ShoppingLab Shop { get; } = new();

    public FixedClock Clock { get; } = new() { Now = ShoppingLab.Now };

    public SelectionIdOptions Options { get; } = new(SigningKey, TimeSpan.FromMinutes(15));

    public SelectionIdCodec Codec => new(Options, Clock);

    public SelectionSigner Signer => new(Codec, Options, Clock);

    public AncillaryOfferListing Listing => new(Shop.Engine, Shop.Catalog, Shop.Catalog, Signer);

    public TypedSelectionService Typed => new(Codec, Shop.Catalog, Shop.Catalog, Shop.Catalog, Shop.Engine, Options, Clock);

    public SelectionResolver Resolver => new(Codec, Shop.Catalog, Shop.Catalog, Shop.Catalog, Shop.Catalog, Clock);

    public static AncillaryShoppingContext Context(
        IReadOnlyList<ShoppingFlight>? flights = null,
        IReadOnlyList<ShoppingPortion>? portions = null,
        IReadOnlyList<ShoppingTraveller>? travellers = null,
        long pointOfSaleId = ShoppingLab.Pos)
    {
        var context = Trip.Context(flights, portions, travellers, pointOfSaleId: pointOfSaleId);

        return context with { SourceIdentity = context.SourceIdentity with { ValidUntilUtc = ShoppingLab.Now.AddMinutes(30) } };
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
        => Listing.ListAsync(contexts, OfferFamilyFilters.Services);

    public static AncillaryOffersSnapshot Snapshot(IReadOnlyList<AncillaryOfferRow> rows) => new(Offer, ShoppingLab.Eur, rows);

    public Task<SelectionSnapshotResult> SubmitAsync(string reference, AncillaryProfile family, ISelectionOptions options, params AncillaryShoppingContext[] contexts)
    {
        var store = new SnapshotStore(Shop.Catalog);
        var service = new SubmitSelectionOptionsService(new LabVerifier(Typed, contexts), store, store, Clock);

        return service.SubmitAsync(new ApiSubmitSelectionOptionsCommand(reference, family, options));
    }
}
