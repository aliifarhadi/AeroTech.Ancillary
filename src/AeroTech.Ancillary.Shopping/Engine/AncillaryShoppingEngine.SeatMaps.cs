using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    public sealed partial class AncillaryShoppingEngine : ISeatMapPricingEngine
    {
        private static readonly string[] NotThisSeatCodes =
        [
            ShoppingReasonCodes.SeatNumberNotAllowed,
            ShoppingReasonCodes.SeatCharacteristicNotAllowed,
            ShoppingReasonCodes.CabinNotAllowed
        ];

        public async Task<SeatMapPricingResult> PriceSeatsAsync(
            AncillaryShoppingContext context,
            ShoppingFilter family,
            IReadOnlyList<SeatMapSeat> seats,
            CancellationToken cancellationToken = default)
        {
            ShoppingContextValidator.Validate(context, family);

            var catalog = await LoadAsync(context, definition => Selected(definition, family), cancellationToken);
            var priced = new List<SeatPricing>();

            foreach (var traveller in context.Travellers)
            {
                var units = CoveragePlanner.Units(context, traveller, ServiceCoverageScope.Sector).ToDictionary(unit => unit.CoverageRef, StringComparer.Ordinal);

                foreach (var seat in seats)
                {
                    if (units.TryGetValue(seat.FlightRef, out var unit))
                        priced.Add(await PriceSeatAsync(catalog, unit, seat, cancellationToken));
                }
            }

            return new SeatMapPricingResult(
                priced,
                priced
                    .Select(seat => seat.Candidate?.Price.RateOfExchange)
                    .OfType<PublishedRateOfExchange>()
                    .DistinctBy(rate => rate.RateOfExchangeTableId, StringComparer.Ordinal)
                    .OrderBy(rate => rate.FromCurrencyId)
                    .ToList());
        }

        private async Task<SeatPricing> PriceSeatAsync(ShoppingCatalog catalog, EvaluationUnit unit, SeatMapSeat seat, CancellationToken cancellationToken)
        {
            var travellerRef = unit.TravellerRef;

            if (seat.Occupied)
                return new SeatPricing(travellerRef, seat.FlightRef, seat.SeatNumber, SeatAvailabilityStatus.Occupied, null);

            var flight = unit.Flights[0];

            if ((unit.FareOf(flight)?.CabinClassId ?? flight.CabinClassId) is not { } bookedCabin || seat.CabinClassId != bookedCabin)
                return new SeatPricing(travellerRef, seat.FlightRef, seat.SeatNumber, SeatAvailabilityStatus.Blocked, null);

            var chosen = unit with { Selection = new AncillarySelection { SeatNumber = seat.SeatNumber } };
            var categories = new List<SeatCategory>();

            foreach (var definition in catalog.Definitions)
            {
                SeatCategory? excluded = null;
                SeatCategory? decided = null;

                foreach (var provision in catalog.ProvisionsOf(definition.Id).Where(provision => provision.CoverageScope == ServiceCoverageScope.Sector))
                {
                    var evaluation = new ProvisionEvaluation(definition, provision, chosen);
                    var outcome = evaluation.Run();

                    if (outcome.RejectionCodes.Any(code => NotThisSeatCodes.Contains(code, StringComparer.Ordinal)))
                        continue;

                    if (outcome.Verdict == RuleVerdict.NoMatch)
                    {
                        excluded ??= new SeatCategory(definition, provision.Sequence, null);

                        continue;
                    }

                    var candidate = (await BuildAsync(catalog, definition, provision, chosen, evaluation, cancellationToken)).Candidate;

                    decided = new SeatCategory(
                        definition,
                        provision.Sequence,
                        candidate.OfferReadiness is OfferReadiness.Selectable or OfferReadiness.NeedsSelection ? candidate : null);

                    break;
                }

                if ((decided ?? excluded) is { } found)
                    categories.Add(found);
            }

            var category = categories
                .OrderBy(row => row.Definition.Seat?.SeatPurpose == SeatPurpose.Preferred ? 0 : 1)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.Definition.ServiceDefinitionRef, StringComparer.Ordinal)
                .FirstOrDefault();

            return category?.Offered is { Price.PricingRevisionId: not null } offered
                ? new SeatPricing(travellerRef, seat.FlightRef, seat.SeatNumber, SeatAvailabilityStatus.Available, offered)
                : new SeatPricing(travellerRef, seat.FlightRef, seat.SeatNumber, SeatAvailabilityStatus.Blocked, null);
        }

        private sealed record SeatCategory(AncillaryServiceDefinition Definition, int Sequence, CanonicalAncillaryOfferCandidate? Offered);
    }
}
