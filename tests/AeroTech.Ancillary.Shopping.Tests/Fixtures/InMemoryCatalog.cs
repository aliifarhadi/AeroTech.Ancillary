using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
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
    IInventoryResourceReference,
    IAirportFacilityReference,
    ICountingFamilyReference,
    IFlightFlowDelegationReference
{
    public List<AncillaryServiceDefinition> Definitions { get; } = [];

    public List<AncillaryProvision> Provisions { get; } = [];

    public List<AncillaryPricing> Pricings { get; } = [];

    public List<InventoryConfiguration> Inventory { get; } = [];

    public Dictionary<int, int> CurrencyScales { get; } = new() { [47] = 2, [155] = 2, [75] = 0, [82] = 3 };

    public InventoryReferenceCheck ResourceAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

    public InventoryReferenceCheck FacilityAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

    public InventoryReferenceCheck FamilyAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

    public InventoryReferenceCheck FlightFlowAnswer { get; set; } = InventoryReferenceCheck.SourceUnavailable;

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

    Task<InventoryReferenceCheck> IInventoryResourceReference.CheckAsync(int ownerAirlineId, InventoryResourceKind kind, long resourceId, CancellationToken cancellationToken)
        => Task.FromResult(ResourceAnswer);

    Task<AirportFacilityCheck> IAirportFacilityReference.CheckAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken)
        => Task.FromResult(new AirportFacilityCheck(FacilityAnswer, null, null));

    Task<InventoryReferenceCheck> ICountingFamilyReference.CheckAsync(int ownerAirlineId, string countingFamilyCode, CancellationToken cancellationToken)
        => Task.FromResult(FamilyAnswer);

    Task<InventoryReferenceCheck> IFlightFlowDelegationReference.CheckAsync(int ownerAirlineId, string providerKey, CancellationToken cancellationToken)
        => Task.FromResult(FlightFlowAnswer);
}
