using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryProvisionAggregate
{
    public sealed class AncillaryProvisionQueryDbSynchronizer : IAncillaryProvisionQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public AncillaryProvisionQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(AncillaryProvisionReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var provision = await _dbContext.AncillaryProvisions
                .FirstOrDefaultAsync(row => row.Id == snapshot.ProvisionId, cancellationToken);

            if (provision is null)
            {
                provision = new AncillaryProvisionReadModel { Id = snapshot.ProvisionId };
                _dbContext.AncillaryProvisions.Add(provision);
            }

            provision.ServiceDefinitionId = snapshot.ServiceDefinitionId;
            provision.Sequence = snapshot.Sequence;
            provision.Status = snapshot.Status;
            provision.SalesEffectiveFrom = snapshot.SalesEffectiveFrom;
            provision.SalesDiscontinueAt = snapshot.SalesDiscontinueAt;
            provision.CoverageScope = snapshot.CoverageScope;
            provision.PassengerTypeCodes = snapshot.Criteria.PassengerTypeCodes.ToList();
            provision.PointOfSaleIds = snapshot.Criteria.PointOfSaleIds.ToList();
            provision.CustomerIds = snapshot.Criteria.CustomerIds.ToList();
            provision.CustomerTypes = snapshot.Criteria.CustomerTypes.ToList();
            provision.OriginAirportIds = snapshot.Criteria.OriginAirportIds.ToList();
            provision.DestinationAirportIds = snapshot.Criteria.DestinationAirportIds.ToList();
            provision.ViaAirportIds = snapshot.Criteria.ViaAirportIds.ToList();
            provision.TravelFrom = snapshot.Criteria.TravelFrom;
            provision.TravelTo = snapshot.Criteria.TravelTo;
            provision.DaysOfWeek = snapshot.Criteria.DaysOfWeek.ToList();
            provision.TimeFrom = snapshot.Criteria.TimeFrom;
            provision.TimeTo = snapshot.Criteria.TimeTo;
            provision.MarketingAirlineIds = snapshot.Criteria.MarketingAirlineIds.ToList();
            provision.OperatingAirlineIds = snapshot.Criteria.OperatingAirlineIds.ToList();
            provision.FlightNumbers = snapshot.Criteria.FlightNumbers.ToList();
            provision.FlightIds = snapshot.Criteria.FlightIds.ToList();
            provision.AircraftIds = snapshot.Criteria.AircraftIds.ToList();
            provision.AirFareIds = snapshot.Criteria.AirFareIds.ToList();
            provision.AirFareTypes = snapshot.Criteria.AirFareTypes.ToList();
            provision.FareFamilyIds = snapshot.Criteria.FareFamilyIds.ToList();
            provision.FareBasisCodes = snapshot.Criteria.FareBasisCodes.ToList();
            provision.CabinClassIds = snapshot.Criteria.CabinClassIds.ToList();
            provision.RbdIds = snapshot.Criteria.RbdIds.ToList();
            provision.AdvancePurchasePeriod = snapshot.Criteria.AdvancePurchasePeriod;
            provision.AdvancePurchaseUnit = snapshot.Criteria.AdvancePurchaseUnit;
            provision.QuantityUnit = snapshot.QuantityUnit;
            provision.MinQuantity = snapshot.MinQuantity;
            provision.MaxQuantity = snapshot.MaxQuantity;
            provision.ApplicationType = snapshot.Application.Type;
            provision.BaggageFreePieces = snapshot.Application.BaggageFreePieces;
            provision.BaggageFirstExcessPiece = snapshot.Application.BaggageFirstExcessPiece;
            provision.BaggageLastExcessPiece = snapshot.Application.BaggageLastExcessPiece;
            provision.BaggageWeight = snapshot.Application.BaggageWeight;
            provision.BaggageWeightUnit = snapshot.Application.BaggageWeightUnit;
            provision.BaggageTravelApplication = snapshot.Application.BaggageTravelApplication;
            provision.BaggagePurchaseApplication = snapshot.Application.BaggagePurchaseApplication;
            provision.BaggageRuleDeference = snapshot.Application.BaggageRuleDeference;
            provision.SeatNumbers = snapshot.Application.SeatNumbers.ToList();
            provision.SeatCharacteristicCodes = snapshot.Application.SeatCharacteristicCodes.ToList();
            provision.Disposition = snapshot.Disposition;
            provision.DocumentRequired = snapshot.DocumentRequired;
            provision.BookingRequired = snapshot.BookingRequired;
            provision.FeeCurrencyId = snapshot.FeeCurrencyId;
            provision.FeeApplicationUnit = snapshot.FeeApplicationUnit;
            provision.ReissueRefund = snapshot.ReissueRefund;
            provision.FormOfRefund = snapshot.FormOfRefund;
            provision.Commissionable = snapshot.Commissionable;
            provision.InterlineSettlement = snapshot.InterlineSettlement;
            provision.MustCheckAvailability = snapshot.MustCheckAvailability;
            provision.FulfillmentProviderKey = snapshot.FulfillmentProviderKey;
            provision.CreatedAt = snapshot.CreatedAt;
            provision.ActivatedAt = snapshot.ActivatedAt;
            provision.SuspendedAt = snapshot.SuspendedAt;
            provision.RetiredAt = snapshot.RetiredAt;
            provision.LastUpdateTime = _clock.GetDateTime();

            await ProjectRoutePairsAsync(snapshot, cancellationToken);
            await ProjectPriceLinesAsync(snapshot, cancellationToken);
        }

        private async Task ProjectRoutePairsAsync(AncillaryProvisionReadModelSnapshot snapshot, CancellationToken cancellationToken)
        {
            var storedPairs = await _dbContext.AncillaryProvisionRoutePairs
                .Where(pair => pair.AncillaryProvisionId == snapshot.ProvisionId)
                .ToListAsync(cancellationToken);

            _dbContext.AncillaryProvisionRoutePairs.RemoveRange(
                storedPairs.Where(pair => snapshot.RoutePairs.All(snapshotPair => snapshotPair.RoutePairId != pair.Id)));

            foreach (var snapshotPair in snapshot.RoutePairs)
            {
                var pair = storedPairs.FirstOrDefault(stored => stored.Id == snapshotPair.RoutePairId);

                if (pair is null)
                {
                    pair = new AncillaryProvisionRoutePairReadModel { Id = snapshotPair.RoutePairId };
                    _dbContext.AncillaryProvisionRoutePairs.Add(pair);
                }

                pair.AncillaryProvisionId = snapshot.ProvisionId;
                pair.OriginAirportId = snapshotPair.OriginAirportId;
                pair.DestinationAirportId = snapshotPair.DestinationAirportId;
                pair.Direction = snapshotPair.Direction;
            }
        }

        private async Task ProjectPriceLinesAsync(AncillaryProvisionReadModelSnapshot snapshot, CancellationToken cancellationToken)
        {
            var storedLines = await _dbContext.AncillaryProvisionPriceLines
                .Where(line => line.AncillaryProvisionId == snapshot.ProvisionId)
                .ToListAsync(cancellationToken);

            _dbContext.AncillaryProvisionPriceLines.RemoveRange(
                storedLines.Where(line => snapshot.PriceLines.All(snapshotLine => snapshotLine.PriceLineId != line.Id)));

            foreach (var snapshotLine in snapshot.PriceLines)
            {
                var line = storedLines.FirstOrDefault(stored => stored.Id == snapshotLine.PriceLineId);

                if (line is null)
                {
                    line = new AncillaryProvisionPriceLineReadModel { Id = snapshotLine.PriceLineId };
                    _dbContext.AncillaryProvisionPriceLines.Add(line);
                }

                line.AncillaryProvisionId = snapshot.ProvisionId;
                line.Category = snapshotLine.Category;
                line.Code = snapshotLine.Code;
                line.Name = snapshotLine.Name;
                line.CountryId = snapshotLine.CountryId;
                line.StationAirportId = snapshotLine.StationAirportId;
                line.UnitAmount = snapshotLine.UnitAmount;
            }
        }
    }
}
