using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class InMemoryCatalog :
    IActiveAncillaryDefinitionReader,
    IActiveAncillaryProvisionReader,
    IActiveAncillaryPricingReader,
    IInventoryConfigurationReader,
    ICurrencyReference,
    IAirlineReference,
    IRateOfExchangeReference,
    IInventoryResourceReference,
    IAirportFacilityReference,
    ICountingFamilyReference,
    ISelectionSnapshotReader
{
    public List<AncillaryServiceDefinition> Definitions { get; } = [];

    public List<AncillaryProvision> Provisions { get; } = [];

    public List<AncillaryPricing> Pricings { get; } = [];

    public List<InventoryConfiguration> Inventory { get; } = [];

    public List<SelectionSnapshot> Snapshots { get; } = [];

    public Dictionary<int, int> CurrencyScales { get; } = new() { [47] = 2, [155] = 2, [75] = 0, [82] = 3 };

    public List<PublishedRateOfExchange> RatesOfExchange { get; } = [];

    public Dictionary<int, string> CurrencyCodes { get; } = new() { [47] = "EUR", [155] = "USD", [75] = "JPY", [82] = "KWD", [70] = "IRR" };

    public Dictionary<int, string> AirlineCodes { get; } = [];

    public InventoryReferenceCheck ResourceAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

    public InventoryReferenceCheck FacilityAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

    public int? FacilityAirportId { get; set; }

    public InventoryReferenceCheck FamilyAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

    public bool Unfiltered { get; set; }

    public int Reads { get; private set; }

    public Task<IReadOnlyList<AncillaryServiceDefinition>> ListActiveAsync(int ownerAirlineId, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryServiceDefinition>>(
            Definitions.Where(definition => Unfiltered || (definition.OwnerAirlineId == ownerAirlineId && definition.Status == ServiceDefinitionStatus.Active)).ToList());
    }

    public Task<IReadOnlyList<AncillaryProvision>> ListActiveAsync(
        IReadOnlyCollection<long> serviceDefinitionIds,
        long pointOfSaleId,
        CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryProvision>>(
            Provisions
                .Where(provision => Unfiltered
                                    || (serviceDefinitionIds.Contains(provision.ServiceDefinitionId)
                                        && provision.Status == ProvisionStatus.Active
                                        && provision.SalesRestrictions is { PointsOfSale.Count: 1 } sales
                                        && sales.PointsOfSale.Single().PointOfSaleId == pointOfSaleId))
                .ToList());
    }

    public Task<IReadOnlyList<AncillaryPricing>> ListActiveAsync(IReadOnlyCollection<long> ancillaryProvisionIds, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryPricing>>(
            Pricings.Where(pricing => Unfiltered || (ancillaryProvisionIds.Contains(pricing.AncillaryProvisionId) && pricing.Status == PricingStatus.Active)).ToList());
    }

    public Task<IReadOnlyList<AncillaryServiceDefinition>> FindActiveDefinitionsAsync(IReadOnlyCollection<long> definitionIds, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryServiceDefinition>>(
            Definitions.Where(definition => definitionIds.Contains(definition.Id) && definition.Status == ServiceDefinitionStatus.Active).ToList());
    }

    public Task<IReadOnlyList<AncillaryPricing>> FindActivePricingsAsync(IReadOnlyCollection<long> pricingIds, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryPricing>>(Pricings.Where(pricing => pricingIds.Contains(pricing.Id) && pricing.Status == PricingStatus.Active).ToList());
    }

    public Task<IReadOnlyList<AncillaryPricing>> FindActiveByRateIdsAsync(IReadOnlyCollection<long> pricingRateIds, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryPricing>>(
            Pricings.Where(pricing => pricing.Status == PricingStatus.Active && pricing.Rates.Any(rate => pricingRateIds.Contains(rate.Id))).ToList());
    }

    public Task<SelectionSnapshot?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult(Snapshots.SingleOrDefault(snapshot => snapshot.Id == id));
    }

    public Task<IReadOnlyList<AncillaryProvision>> FindActiveProvisionsAsync(IReadOnlyCollection<long> provisionIds, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<AncillaryProvision>>(Provisions.Where(provision => provisionIds.Contains(provision.Id) && provision.Status == ProvisionStatus.Active).ToList());
    }

    public Task<IReadOnlyList<InventoryConfiguration>> ListAsync(
        int ownerAirlineId,
        IReadOnlyCollection<string> serviceDefinitionRefs,
        IReadOnlyCollection<long> flightIds,
        DateTimeOffset evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyList<InventoryConfiguration>>(
            Inventory.Where(configuration => configuration.Policy.OwnerAirlineId == ownerAirlineId && serviceDefinitionRefs.Contains(configuration.Policy.ServiceDefinitionRef)).ToList());
    }

    public Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default)
    {
        Reads++;

        return Task.FromResult<IReadOnlyDictionary<int, int>>(
            CurrencyScales.Where(scale => currencyIds.Contains(scale.Key)).ToDictionary(scale => scale.Key, scale => scale.Value));
    }

    public Task<IReadOnlyDictionary<int, string>> FindCodesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<int, string>>(CurrencyCodes.Where(code => currencyIds.Contains(code.Key)).ToDictionary(code => code.Key, code => code.Value));

    public Task<IReadOnlyDictionary<int, string>> FindIataCodesAsync(IReadOnlyCollection<int> airlineIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<int, string>>(AirlineCodes.Where(code => airlineIds.Contains(code.Key)).ToDictionary(code => code.Key, code => code.Value));

    public Task<IReadOnlyList<PublishedRateOfExchange>> FindRatesAsync(int toCurrencyId, DateTimeOffset at, CancellationToken cancellationToken = default)
    {
        Reads++;

        var decimals = CurrencyScales.GetValueOrDefault(toCurrencyId, 2);

        return Task.FromResult<IReadOnlyList<PublishedRateOfExchange>>(
            RatesOfExchange
                .Where(rate => rate.ToCurrencyId == toCurrencyId)
                .Append(new PublishedRateOfExchange(toCurrencyId.ToString(), toCurrencyId, toCurrencyId, 1m, decimals, 1d))
                .ToList());
    }

    Task<InventoryReferenceCheck> IInventoryResourceReference.CheckAsync(int ownerAirlineId, InventoryResourceKind kind, long resourceId, CancellationToken cancellationToken)
        => Task.FromResult(ResourceAnswer);

    Task<AirportFacilityCheck> IAirportFacilityReference.CheckAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken)
        => Task.FromResult(new AirportFacilityCheck(FacilityAnswer, FacilityAirportId, null));

    Task<InventoryReferenceCheck> ICountingFamilyReference.CheckAsync(int ownerAirlineId, string countingFamilyCode, CancellationToken cancellationToken)
        => Task.FromResult(FamilyAnswer);
}
