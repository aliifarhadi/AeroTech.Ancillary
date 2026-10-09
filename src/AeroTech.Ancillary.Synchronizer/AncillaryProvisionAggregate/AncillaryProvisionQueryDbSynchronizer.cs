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
            provision.CoverageScope = snapshot.CoverageScope;
            provision.PurchaseStage = snapshot.PurchaseStage;
            provision.QuantityUnit = snapshot.QuantityUnit;
            provision.MinQuantity = snapshot.MinQuantity;
            provision.MaxQuantity = snapshot.MaxQuantity;
            provision.ApplicationType = snapshot.ApplicationType;
            provision.Disposition = snapshot.Disposition;
            provision.DocumentRequired = snapshot.DocumentRequired;
            provision.BookingRequired = snapshot.BookingRequired;
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
            provision.SalesEffectiveFrom = snapshot.SalesRestrictions?.SalesEffectiveFrom;
            provision.SalesDiscontinueAt = snapshot.SalesRestrictions?.SalesDiscontinueAt;
            provision.AdvancePurchasePeriod = snapshot.AdvancePurchase?.MinimumPeriod;
            provision.AdvancePurchaseUnit = snapshot.AdvancePurchase?.Unit;
            provision.AdvancePurchaseSameTimeAsTicketed = snapshot.AdvancePurchase?.SameTimeAsTicketed ?? false;
            provision.AdvancePurchaseMaximumPeriod = snapshot.AdvancePurchase?.MaximumPeriod;
            provision.BaggageFreePieces = snapshot.BaggageApplication?.FreePieces;
            provision.BaggageFirstExcessPiece = snapshot.BaggageApplication?.FirstExcessPiece;
            provision.BaggageLastExcessPiece = snapshot.BaggageApplication?.LastExcessPiece;
            provision.BaggageWeight = snapshot.BaggageApplication?.Weight;
            provision.BaggageWeightUnit = snapshot.BaggageApplication?.WeightUnit;
            provision.BaggageTravelApplication = snapshot.BaggageApplication?.TravelApplication;
            provision.BaggagePurchaseApplication = snapshot.BaggageApplication?.PurchaseApplication;
            provision.BaggageRuleDeference = snapshot.BaggageApplication?.RuleDeference;
            provision.BaggageChargeKind = snapshot.BaggageApplication?.ChargeKind;
            provision.BaggageAllowanceConcept = snapshot.BaggageApplication?.AllowanceConcept;
            provision.PassengerEligibilityRuleId = snapshot.PassengerEligibility?.RuleId;
            provision.SalesRestrictionsRuleId = snapshot.SalesRestrictions?.RuleId;
            provision.GeographyRuleId = snapshot.Geography?.RuleId;
            provision.FlightApplicationRuleId = snapshot.FlightApplication?.RuleId;
            provision.FareApplicationRuleId = snapshot.FareApplication?.RuleId;
            provision.TravelDateRuleId = snapshot.TravelDate?.RuleId;
            provision.DayTimeApplicationRuleId = snapshot.DayTimeApplication?.RuleId;
            provision.AdvancePurchaseRuleId = snapshot.AdvancePurchase?.RuleId;
            provision.BaggageApplicationRuleId = snapshot.BaggageApplication?.RuleId;
            provision.SeatApplicationRuleId = snapshot.SeatApplication?.RuleId;

            ProjectRows(
                _dbContext.AncillaryProvisionPassengerTypes,
                await _dbContext.AncillaryProvisionPassengerTypes.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.PassengerEligibility?.PassengerTypes ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionPassengerTypeReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.PassengerTypeCode = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionEligibleAgeBands,
                await _dbContext.AncillaryProvisionEligibleAgeBands.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.PassengerEligibility?.AgeBands ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionEligibleAgeBandReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) =>
                {
                    row.AgeFromInclusive = item.AgeFromInclusive;
                    row.AgeToExclusive = item.AgeToExclusive;
                });
            ProjectRows(
                _dbContext.AncillaryProvisionPointsOfSale,
                await _dbContext.AncillaryProvisionPointsOfSale.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.SalesRestrictions?.PointsOfSale ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionPointOfSaleReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.PointOfSaleId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionCustomers,
                await _dbContext.AncillaryProvisionCustomers.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.SalesRestrictions?.Customers ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionCustomerReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.CustomerId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionCustomerTypes,
                await _dbContext.AncillaryProvisionCustomerTypes.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.SalesRestrictions?.CustomerTypes ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionCustomerTypeReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.CustomerType = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionOriginAirports,
                await _dbContext.AncillaryProvisionOriginAirports.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.Geography?.OriginAirports ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionOriginAirportReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirportId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionDestinationAirports,
                await _dbContext.AncillaryProvisionDestinationAirports.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.Geography?.DestinationAirports ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionDestinationAirportReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirportId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionViaAirports,
                await _dbContext.AncillaryProvisionViaAirports.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.Geography?.ViaAirports ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionViaAirportReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirportId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionCoverageCountries,
                await _dbContext.AncillaryProvisionCoverageCountries.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.Geography?.CoverageCountries ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionCoverageCountryReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.CountryId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionRoutePairs,
                await _dbContext.AncillaryProvisionRoutePairs.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.Geography?.RoutePairs ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionRoutePairReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) =>
                {
                    row.OriginAirportId = item.OriginAirportId;
                    row.DestinationAirportId = item.DestinationAirportId;
                    row.Direction = item.Direction;
                });
            ProjectRows(
                _dbContext.AncillaryProvisionServiceLocations,
                await _dbContext.AncillaryProvisionServiceLocations.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.Geography?.ServiceLocations ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionServiceLocationReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) =>
                {
                    row.LocationType = item.LocationType;
                    row.LocationId = item.LocationId;
                });
            ProjectRows(
                _dbContext.AncillaryProvisionMarketingAirlines,
                await _dbContext.AncillaryProvisionMarketingAirlines.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FlightApplication?.MarketingAirlines ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionMarketingAirlineReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirlineId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionOperatingAirlines,
                await _dbContext.AncillaryProvisionOperatingAirlines.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FlightApplication?.OperatingAirlines ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionOperatingAirlineReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirlineId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionFlightNumbers,
                await _dbContext.AncillaryProvisionFlightNumbers.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FlightApplication?.FlightNumbers ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionFlightNumberReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.FlightNumber = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionFlights,
                await _dbContext.AncillaryProvisionFlights.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FlightApplication?.Flights ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionFlightReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.FlightId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionAircraft,
                await _dbContext.AncillaryProvisionAircraft.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FlightApplication?.Aircraft ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionAircraftReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AircraftId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionAirFares,
                await _dbContext.AncillaryProvisionAirFares.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FareApplication?.AirFares ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionAirFareReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirFareId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionAirFareTypes,
                await _dbContext.AncillaryProvisionAirFareTypes.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FareApplication?.AirFareTypes ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionAirFareTypeReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.AirFareType = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionFareFamilies,
                await _dbContext.AncillaryProvisionFareFamilies.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FareApplication?.FareFamilies ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionFareFamilyReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.FareFamilyId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionFareBases,
                await _dbContext.AncillaryProvisionFareBases.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FareApplication?.FareBases ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionFareBasisReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.FareBasisCode = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionCabinClasses,
                await _dbContext.AncillaryProvisionCabinClasses.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FareApplication?.CabinClasses ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionCabinClassReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.CabinClassId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionRbds,
                await _dbContext.AncillaryProvisionRbds.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.FareApplication?.Rbds ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionRbdReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.RbdId = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionPermittedTravelPeriods,
                await _dbContext.AncillaryProvisionPermittedTravelPeriods.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.TravelDate?.PermittedPeriods ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionPermittedTravelPeriodReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) =>
                {
                    row.StartDate = item.StartDate;
                    row.EndDate = item.EndDate;
                });
            ProjectRows(
                _dbContext.AncillaryProvisionBlackoutPeriods,
                await _dbContext.AncillaryProvisionBlackoutPeriods.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.TravelDate?.BlackoutPeriods ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionBlackoutPeriodReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) =>
                {
                    row.StartDate = item.StartDate;
                    row.EndDate = item.EndDate;
                });
            ProjectRows(
                _dbContext.AncillaryProvisionDayTimeWindows,
                await _dbContext.AncillaryProvisionDayTimeWindows.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.DayTimeApplication?.Windows ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionDayTimeWindowReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) =>
                {
                    row.DaysOfWeekMask = item.DaysOfWeekMask;
                    row.StartLocalTime = item.StartLocalTime;
                    row.EndLocalTime = item.EndLocalTime;
                    row.Effect = item.Effect;
                });
            ProjectRows(
                _dbContext.AncillaryProvisionSeatNumbers,
                await _dbContext.AncillaryProvisionSeatNumbers.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.SeatApplication?.SeatNumbers ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionSeatNumberReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.SeatNumber = item.Value);
            ProjectRows(
                _dbContext.AncillaryProvisionSeatCharacteristics,
                await _dbContext.AncillaryProvisionSeatCharacteristics.Where(row => row.AncillaryProvisionId == snapshot.ProvisionId).ToListAsync(cancellationToken),
                snapshot.SeatApplication?.SeatCharacteristics ?? [],
                row => row.Id,
                item => item.RowId,
                item => new AncillaryProvisionSeatCharacteristicReadModel { Id = item.RowId, AncillaryProvisionId = snapshot.ProvisionId },
                (row, item) => row.CharacteristicCode = item.Value);
        }

        private static void ProjectRows<TRow, TSnapshot>(
            DbSet<TRow> set,
            List<TRow> stored,
            IReadOnlyList<TSnapshot> snapshots,
            Func<TRow, long> storedId,
            Func<TSnapshot, long> snapshotId,
            Func<TSnapshot, TRow> create,
            Action<TRow, TSnapshot> apply)
            where TRow : class
        {
            var storedById = stored.ToDictionary(storedId);
            var snapshotIds = snapshots.Select(snapshotId).ToHashSet();

            set.RemoveRange(stored.Where(row => !snapshotIds.Contains(storedId(row))));

            foreach (var snapshot in snapshots)
            {
                if (!storedById.TryGetValue(snapshotId(snapshot), out var row))
                {
                    row = create(snapshot);
                    set.Add(row);
                }

                apply(row, snapshot);
            }
        }
    }
}
