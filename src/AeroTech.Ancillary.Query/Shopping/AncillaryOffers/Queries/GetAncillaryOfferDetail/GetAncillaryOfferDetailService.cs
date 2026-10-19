using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Reading;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail
{
    public sealed class GetAncillaryOfferDetailService : IGetAncillaryOfferDetailService
    {
        private readonly IGetAncillaryOffersService _offers;
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryPricingReader _pricings;

        public GetAncillaryOfferDetailService(
            IGetAncillaryOffersService offers,
            IActiveAncillaryDefinitionReader definitions,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryPricingReader pricings)
        {
            _offers = offers;
            _definitions = definitions;
            _provisions = provisions;
            _pricings = pricings;
        }

        public Task<AncillaryOfferDetailSnapshot> ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ExecuteAsync(query, scope, ShoppingFilter.None, cancellationToken);

        public async Task<AncillaryOfferDetailSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
        {
            var (shopping, item) = await _offers.FindItemAsync(query, scope, family, cancellationToken);
            var candidate = item.Candidate;
            var definition = (await _definitions.ListActiveAsync(scope.OwnerAirlineId, cancellationToken))
                             .SingleOrDefault(row => row.Id == candidate.DefinitionVersionId)
                             ?? throw ExceptionFactory.OfferItemWasNotFound();
            var provision = (await _provisions.ListActiveAsync([definition.Id], item.PointOfSaleId, cancellationToken))
                            .SingleOrDefault(row => row.Id == candidate.ProvisionId)
                            ?? throw ExceptionFactory.OfferItemWasNotFound();
            var pricing = candidate.Price.PricingRevisionId is { } revision
                ? (await _pricings.ListActiveAsync([provision.Id], cancellationToken)).SingleOrDefault(row => row.Id == revision)
                  ?? throw ExceptionFactory.OfferItemWasNotFound()
                : null;

            return new AncillaryOfferDetailSnapshot(
                shopping,
                item,
                definition.SpecificationArgs(),
                item.Context.TravellerBaggageFacts
                    .Where(fact => candidate.Travellers.Contains(fact.TravellerRef) && candidate.FlightRefs.Contains(fact.FlightRef))
                    .ToList(),
                provision.BaggageApplication is { } baggage
                    ? new OfferBaggageRule(
                        baggage.FreePieces,
                        baggage.FirstExcessPiece,
                        baggage.LastExcessPiece,
                        baggage.Weight,
                        baggage.WeightUnit,
                        baggage.PurchaseApplication,
                        baggage.ChargeKind,
                        baggage.AllowanceConcept)
                    : null,
                provision.SeatApplication is { } seats
                    ? new OfferSeatRule(
                        seats.SeatNumbers.Select(seat => seat.SeatNumber).Order(StringComparer.Ordinal).ToList(),
                        seats.SeatCharacteristics.Select(characteristic => characteristic.CharacteristicCode).Order(StringComparer.Ordinal).ToList())
                    : null,
                new OfferRestrictions(
                    provision.PassengerEligibility?.PassengerTypes.Select(type => type.PassengerTypeCode).Order().ToList() ?? [],
                    provision.SalesRestrictions?.SalesEffectiveFrom,
                    provision.SalesRestrictions?.SalesDiscontinueAt,
                    definition.SalesEffectiveFrom,
                    definition.SalesDiscontinueOn,
                    provision.TravelDate?.PermittedPeriods.OrderBy(period => period.StartDate).Select(period => new OfferDatePeriod(period.StartDate, period.EndDate)).ToList() ?? [],
                    provision.TravelDate?.BlackoutPeriods.OrderBy(period => period.StartDate).Select(period => new OfferDatePeriod(period.StartDate, period.EndDate)).ToList() ?? [],
                    provision.AdvancePurchase is { } advance
                        ? new OfferAdvancePurchase(advance.MinimumPeriod, advance.MaximumPeriod, advance.Unit, advance.SameTimeAsTicketed)
                        : null,
                    provision.PurchaseStage),
                new OfferTerms(
                    provision.Outcome.Disposition,
                    provision.Settlement.ReissueRefund,
                    provision.Settlement.FormOfRefund,
                    provision.Settlement.Commissionable,
                    provision.Settlement.InterlineSettlement,
                    definition.Booking.ConfirmationRequirement),
                pricing?.Rates
                    .Where(rate => rate.CurrencyId == shopping.CurrencyId)
                    .OrderBy(rate => rate.PassengerTypeCode)
                    .ThenBy(rate => rate.AgeFromInclusive)
                    .Select(rate => new OfferPublishedRate(
                        rate.PassengerTypeCode,
                        rate.AgeFromInclusive,
                        rate.AgeToExclusive,
                        rate.CurrencyId,
                        rate.BaseAmount,
                        rate.UnitTotal.Amount,
                        rate.IsUnitTotalComplete,
                        rate.Id == candidate.Price.PricingRateId))
                    .ToList() ?? [],
                OfferOptionCatalog.Build(definition, provision, candidate, item.Context),
                new OfferCommercialTerms(
                    provision.PurchaseStage,
                    provision.Outcome.Disposition,
                    provision.Settlement.ReissueRefund,
                    provision.Settlement.FormOfRefund,
                    definition.Booking.ConfirmationRequirement,
                    definition.Booking.Method,
                    definition.DocumentRouting!.Value,
                    definition.Document.Type,
                    definition.Document.Rfic,
                    definition.Document.Rfisc,
                    provision.CancellationRules
                        .OrderBy(rule => rule.RuleRef, StringComparer.Ordinal)
                        .Select(rule => new OfferCancellationRule(
                            rule.RuleRef,
                            rule.ReasonKind,
                            rule.BeforeStartMinutesFrom,
                            rule.BeforeStartMinutesTo,
                            rule.PenaltyKind,
                            rule.PenaltyFixedAmount,
                            rule.PenaltyPercentage,
                            rule.PenaltyCurrencyId,
                            rule.PercentageBase,
                            rule.WaiverPermitted,
                            rule.SourceReference))
                        .ToList()));
        }
    }
}
