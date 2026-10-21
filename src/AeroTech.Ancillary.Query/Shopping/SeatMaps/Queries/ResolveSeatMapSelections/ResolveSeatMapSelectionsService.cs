using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections
{
    public sealed class ResolveSeatMapSelectionsService : IResolveSeatMapSelectionsService
    {
        private readonly IActiveAncillaryPricingReader _pricings;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryDefinitionReader _definitions;

        public ResolveSeatMapSelectionsService(
            IActiveAncillaryPricingReader pricings,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryDefinitionReader definitions)
        {
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
        }

        public async Task<SeatMapResolutionSnapshot> ExecuteAsync(IReadOnlyList<string>? ids, CancellationToken cancellationToken = default)
        {
            var selections = WellFormed(ids);
            var pricings = (await _pricings.FindActivePricingsAsync(selections.Select(seat => seat.Selection.PricingId).Distinct().ToList(), cancellationToken))
                .ToDictionary(pricing => pricing.Id);
            var provisions = (await _provisions.FindActiveProvisionsAsync(pricings.Values.Select(pricing => pricing.AncillaryProvisionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(provision => provision.Id);
            var definitions = (await _definitions.FindActiveDefinitionsAsync(provisions.Values.Select(provision => provision.ServiceDefinitionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(definition => definition.Id);

            return new SeatMapResolutionSnapshot(selections
                .Select(seat => pricings.GetValueOrDefault(seat.Selection.PricingId) is { } pricing
                                && provisions.GetValueOrDefault(pricing.AncillaryProvisionId) is { } provision
                                && definitions.GetValueOrDefault(provision.ServiceDefinitionId) is { Profile: AncillaryProfile.Seat } definition
                    ? new ResolvedSeatSnapshot(seat.Id, seat.Selection, definition, provision, pricing)
                    : throw ExceptionFactory.OfferItemWasNotFound($"seat id {seat.Id} names no active seat pricing"))
                .ToList());
        }

        private static IReadOnlyList<(string Id, SeatMapSelection Selection)> WellFormed(IReadOnlyList<string>? ids)
        {
            if (ids is not { Count: > 0 and <= EvaluateBatchService.MaximumLines })
                throw ExceptionFactory.ShoppingContextIsInvalid($"ids must hold between 1 and {EvaluateBatchService.MaximumLines} seat ids");

            if (ids.GroupBy(id => id, StringComparer.Ordinal).Any(group => group.Skip(1).Any()))
                throw ExceptionFactory.ShoppingContextIsInvalid("a seat id must appear once");

            var selections = ids
                .Select(id => (Id: id, Selection: SeatMapSelectionId.Decode(id) ?? throw ExceptionFactory.ShoppingContextIsInvalid($"{id} is not a seat map id")))
                .ToList();

            if (selections.GroupBy(seat => (seat.Selection.FlightId, seat.Selection.SeatNumber)).Any(group => group.Skip(1).Any()))
                throw ExceptionFactory.ShoppingContextIsInvalid("a seat of a flight must be selected once");

            return selections.GroupBy(seat => (seat.Selection.FlightId, seat.Selection.TravellerRef)).Any(group => group.Skip(1).Any())
                ? throw ExceptionFactory.ShoppingContextIsInvalid("a traveller must select one seat per flight")
                : selections;
        }
    }
}
