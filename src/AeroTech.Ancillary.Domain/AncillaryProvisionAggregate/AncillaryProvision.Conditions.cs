using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision
    {
        private readonly List<ProvisionPassengerType> _passengerTypes = new();
        private readonly List<ProvisionPointOfSale> _pointsOfSale = new();
        private readonly List<ProvisionCustomer> _customers = new();
        private readonly List<ProvisionCustomerType> _customerTypes = new();
        private readonly List<ProvisionOriginAirport> _originAirports = new();
        private readonly List<ProvisionDestinationAirport> _destinationAirports = new();
        private readonly List<ProvisionViaAirport> _viaAirports = new();
        private readonly List<ProvisionRoutePair> _routePairs = new();
        private readonly List<ProvisionMarketingAirline> _marketingAirlines = new();
        private readonly List<ProvisionOperatingAirline> _operatingAirlines = new();
        private readonly List<ProvisionFlightNumber> _flightNumbers = new();
        private readonly List<ProvisionFlight> _flights = new();
        private readonly List<ProvisionAircraft> _aircraft = new();
        private readonly List<ProvisionAirFare> _airFares = new();
        private readonly List<ProvisionAirFareType> _airFareTypes = new();
        private readonly List<ProvisionFareFamily> _fareFamilies = new();
        private readonly List<ProvisionFareBasis> _fareBases = new();
        private readonly List<ProvisionCabinClass> _cabinClasses = new();
        private readonly List<ProvisionRbd> _rbds = new();
        private readonly List<ProvisionTravelDate> _travelDates = new();
        private readonly List<ProvisionSeasonalPeriod> _seasonalPeriods = new();
        private readonly List<ProvisionBlackoutPeriod> _blackoutPeriods = new();
        private readonly List<ProvisionDayTimeRestriction> _dayTimeRestrictions = new();
        private readonly List<ProvisionSeatNumber> _seatNumbers = new();
        private readonly List<ProvisionSeatCharacteristic> _seatCharacteristics = new();

        public IReadOnlyCollection<ProvisionPassengerType> PassengerTypes => _passengerTypes.AsReadOnly();

        public IReadOnlyCollection<ProvisionPointOfSale> PointsOfSale => _pointsOfSale.AsReadOnly();

        public IReadOnlyCollection<ProvisionCustomer> Customers => _customers.AsReadOnly();

        public IReadOnlyCollection<ProvisionCustomerType> CustomerTypes => _customerTypes.AsReadOnly();

        public IReadOnlyCollection<ProvisionOriginAirport> OriginAirports => _originAirports.AsReadOnly();

        public IReadOnlyCollection<ProvisionDestinationAirport> DestinationAirports => _destinationAirports.AsReadOnly();

        public IReadOnlyCollection<ProvisionViaAirport> ViaAirports => _viaAirports.AsReadOnly();

        public IReadOnlyCollection<ProvisionRoutePair> RoutePairs => _routePairs.AsReadOnly();

        public IReadOnlyCollection<ProvisionMarketingAirline> MarketingAirlines => _marketingAirlines.AsReadOnly();

        public IReadOnlyCollection<ProvisionOperatingAirline> OperatingAirlines => _operatingAirlines.AsReadOnly();

        public IReadOnlyCollection<ProvisionFlightNumber> FlightNumbers => _flightNumbers.AsReadOnly();

        public IReadOnlyCollection<ProvisionFlight> Flights => _flights.AsReadOnly();

        public IReadOnlyCollection<ProvisionAircraft> Aircraft => _aircraft.AsReadOnly();

        public IReadOnlyCollection<ProvisionAirFare> AirFares => _airFares.AsReadOnly();

        public IReadOnlyCollection<ProvisionAirFareType> AirFareTypes => _airFareTypes.AsReadOnly();

        public IReadOnlyCollection<ProvisionFareFamily> FareFamilies => _fareFamilies.AsReadOnly();

        public IReadOnlyCollection<ProvisionFareBasis> FareBases => _fareBases.AsReadOnly();

        public IReadOnlyCollection<ProvisionCabinClass> CabinClasses => _cabinClasses.AsReadOnly();

        public IReadOnlyCollection<ProvisionRbd> Rbds => _rbds.AsReadOnly();

        public IReadOnlyCollection<ProvisionTravelDate> TravelDates => _travelDates.AsReadOnly();

        public IReadOnlyCollection<ProvisionSeasonalPeriod> SeasonalPeriods => _seasonalPeriods.AsReadOnly();

        public IReadOnlyCollection<ProvisionBlackoutPeriod> BlackoutPeriods => _blackoutPeriods.AsReadOnly();

        public IReadOnlyCollection<ProvisionDayTimeRestriction> DayTimeRestrictions => _dayTimeRestrictions.AsReadOnly();

        public IReadOnlyCollection<ProvisionSeatNumber> SeatNumbers => _seatNumbers.AsReadOnly();

        public IReadOnlyCollection<ProvisionSeatCharacteristic> SeatCharacteristics => _seatCharacteristics.AsReadOnly();

        public void ReplaceConditions(ProvisionConditionsArgs conditions, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var rows = Plan(conditions, idGenerator);

            EnsureSeatSelectors(Application.Type, rows.SeatNumbers.Count, rows.SeatCharacteristics.Count, rows.Aircraft.Count);
            Commit(rows);
        }

        public ProvisionPassengerType AddPassengerType(PassengerTypeCode passengerTypeCode, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionPassengerType(idGenerator.NewId(), Id, passengerTypeCode);

            EnsureAbsent(_passengerTypes, row, (left, right) => left.SameAs(right));
            _passengerTypes.Add(row);

            return row;
        }

        public void ChangePassengerType(long rowId, PassengerTypeCode passengerTypeCode)
        {
            EnsureDraft();

            var stored = Find(_passengerTypes, rowId);
            var row = new ProvisionPassengerType(rowId, Id, passengerTypeCode);

            EnsureAbsent(_passengerTypes.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(passengerTypeCode);
        }

        public void RemovePassengerType(long rowId)
        {
            EnsureDraft();

            var stored = Find(_passengerTypes, rowId);

            _passengerTypes.Remove(stored);
        }

        public ProvisionPointOfSale AddPointOfSale(long pointOfSaleId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionPointOfSale(idGenerator.NewId(), Id, pointOfSaleId);

            EnsureAbsent(_pointsOfSale, row, (left, right) => left.SameAs(right));
            _pointsOfSale.Add(row);

            return row;
        }

        public void ChangePointOfSale(long rowId, long pointOfSaleId)
        {
            EnsureDraft();

            var stored = Find(_pointsOfSale, rowId);
            var row = new ProvisionPointOfSale(rowId, Id, pointOfSaleId);

            EnsureAbsent(_pointsOfSale.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(pointOfSaleId);
        }

        public void RemovePointOfSale(long rowId)
        {
            EnsureDraft();

            var stored = Find(_pointsOfSale, rowId);

            _pointsOfSale.Remove(stored);
        }

        public ProvisionCustomer AddCustomer(long customerId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionCustomer(idGenerator.NewId(), Id, customerId);

            EnsureAbsent(_customers, row, (left, right) => left.SameAs(right));
            _customers.Add(row);

            return row;
        }

        public void ChangeCustomer(long rowId, long customerId)
        {
            EnsureDraft();

            var stored = Find(_customers, rowId);
            var row = new ProvisionCustomer(rowId, Id, customerId);

            EnsureAbsent(_customers.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(customerId);
        }

        public void RemoveCustomer(long rowId)
        {
            EnsureDraft();

            var stored = Find(_customers, rowId);

            _customers.Remove(stored);
        }

        public ProvisionCustomerType AddCustomerType(CustomerType customerType, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionCustomerType(idGenerator.NewId(), Id, customerType);

            EnsureAbsent(_customerTypes, row, (left, right) => left.SameAs(right));
            _customerTypes.Add(row);

            return row;
        }

        public void ChangeCustomerType(long rowId, CustomerType customerType)
        {
            EnsureDraft();

            var stored = Find(_customerTypes, rowId);
            var row = new ProvisionCustomerType(rowId, Id, customerType);

            EnsureAbsent(_customerTypes.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(customerType);
        }

        public void RemoveCustomerType(long rowId)
        {
            EnsureDraft();

            var stored = Find(_customerTypes, rowId);

            _customerTypes.Remove(stored);
        }

        public ProvisionOriginAirport AddOriginAirport(int airportId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionOriginAirport(idGenerator.NewId(), Id, airportId);

            EnsureAbsent(_originAirports, row, (left, right) => left.SameAs(right));
            _originAirports.Add(row);

            return row;
        }

        public void ChangeOriginAirport(long rowId, int airportId)
        {
            EnsureDraft();

            var stored = Find(_originAirports, rowId);
            var row = new ProvisionOriginAirport(rowId, Id, airportId);

            EnsureAbsent(_originAirports.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airportId);
        }

        public void RemoveOriginAirport(long rowId)
        {
            EnsureDraft();

            var stored = Find(_originAirports, rowId);

            _originAirports.Remove(stored);
        }

        public ProvisionDestinationAirport AddDestinationAirport(int airportId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionDestinationAirport(idGenerator.NewId(), Id, airportId);

            EnsureAbsent(_destinationAirports, row, (left, right) => left.SameAs(right));
            _destinationAirports.Add(row);

            return row;
        }

        public void ChangeDestinationAirport(long rowId, int airportId)
        {
            EnsureDraft();

            var stored = Find(_destinationAirports, rowId);
            var row = new ProvisionDestinationAirport(rowId, Id, airportId);

            EnsureAbsent(_destinationAirports.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airportId);
        }

        public void RemoveDestinationAirport(long rowId)
        {
            EnsureDraft();

            var stored = Find(_destinationAirports, rowId);

            _destinationAirports.Remove(stored);
        }

        public ProvisionViaAirport AddViaAirport(int airportId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionViaAirport(idGenerator.NewId(), Id, airportId);

            EnsureAbsent(_viaAirports, row, (left, right) => left.SameAs(right));
            _viaAirports.Add(row);

            return row;
        }

        public void ChangeViaAirport(long rowId, int airportId)
        {
            EnsureDraft();

            var stored = Find(_viaAirports, rowId);
            var row = new ProvisionViaAirport(rowId, Id, airportId);

            EnsureAbsent(_viaAirports.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airportId);
        }

        public void RemoveViaAirport(long rowId)
        {
            EnsureDraft();

            var stored = Find(_viaAirports, rowId);

            _viaAirports.Remove(stored);
        }

        public ProvisionRoutePair AddRoutePair(ProvisionRoutePairArgs args, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionRoutePair(idGenerator.NewId(), Id, args);

            EnsureAbsent(_routePairs, row, (left, right) => left.Overlaps(right));
            _routePairs.Add(row);

            return row;
        }

        public void ChangeRoutePair(long rowId, ProvisionRoutePairArgs args)
        {
            EnsureDraft();

            var stored = Find(_routePairs, rowId);
            var row = new ProvisionRoutePair(rowId, Id, args);

            EnsureAbsent(_routePairs.Where(other => other.Id != rowId), row, (left, right) => left.Overlaps(right));
            stored.Change(args);
        }

        public void RemoveRoutePair(long rowId)
        {
            EnsureDraft();

            var stored = Find(_routePairs, rowId);

            _routePairs.Remove(stored);
        }

        public ProvisionMarketingAirline AddMarketingAirline(int airlineId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionMarketingAirline(idGenerator.NewId(), Id, airlineId);

            EnsureAbsent(_marketingAirlines, row, (left, right) => left.SameAs(right));
            _marketingAirlines.Add(row);

            return row;
        }

        public void ChangeMarketingAirline(long rowId, int airlineId)
        {
            EnsureDraft();

            var stored = Find(_marketingAirlines, rowId);
            var row = new ProvisionMarketingAirline(rowId, Id, airlineId);

            EnsureAbsent(_marketingAirlines.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airlineId);
        }

        public void RemoveMarketingAirline(long rowId)
        {
            EnsureDraft();

            var stored = Find(_marketingAirlines, rowId);

            _marketingAirlines.Remove(stored);
        }

        public ProvisionOperatingAirline AddOperatingAirline(int airlineId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionOperatingAirline(idGenerator.NewId(), Id, airlineId);

            EnsureAbsent(_operatingAirlines, row, (left, right) => left.SameAs(right));
            _operatingAirlines.Add(row);

            return row;
        }

        public void ChangeOperatingAirline(long rowId, int airlineId)
        {
            EnsureDraft();

            var stored = Find(_operatingAirlines, rowId);
            var row = new ProvisionOperatingAirline(rowId, Id, airlineId);

            EnsureAbsent(_operatingAirlines.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airlineId);
        }

        public void RemoveOperatingAirline(long rowId)
        {
            EnsureDraft();

            var stored = Find(_operatingAirlines, rowId);

            _operatingAirlines.Remove(stored);
        }

        public ProvisionFlightNumber AddFlightNumber(string flightNumber, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionFlightNumber(idGenerator.NewId(), Id, flightNumber);

            EnsureAbsent(_flightNumbers, row, (left, right) => left.SameAs(right));
            _flightNumbers.Add(row);

            return row;
        }

        public void ChangeFlightNumber(long rowId, string flightNumber)
        {
            EnsureDraft();

            var stored = Find(_flightNumbers, rowId);
            var row = new ProvisionFlightNumber(rowId, Id, flightNumber);

            EnsureAbsent(_flightNumbers.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(flightNumber);
        }

        public void RemoveFlightNumber(long rowId)
        {
            EnsureDraft();

            var stored = Find(_flightNumbers, rowId);

            _flightNumbers.Remove(stored);
        }

        public ProvisionFlight AddFlight(long flightId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionFlight(idGenerator.NewId(), Id, flightId);

            EnsureAbsent(_flights, row, (left, right) => left.SameAs(right));
            _flights.Add(row);

            return row;
        }

        public void ChangeFlight(long rowId, long flightId)
        {
            EnsureDraft();

            var stored = Find(_flights, rowId);
            var row = new ProvisionFlight(rowId, Id, flightId);

            EnsureAbsent(_flights.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(flightId);
        }

        public void RemoveFlight(long rowId)
        {
            EnsureDraft();

            var stored = Find(_flights, rowId);

            _flights.Remove(stored);
        }

        public ProvisionAircraft AddAircraft(int aircraftId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionAircraft(idGenerator.NewId(), Id, aircraftId);

            EnsureAbsent(_aircraft, row, (left, right) => left.SameAs(right));
            EnsureSeatSelectors(Application.Type, _seatNumbers.Count, _seatCharacteristics.Count, _aircraft.Count + 1);
            _aircraft.Add(row);

            return row;
        }

        public void ChangeAircraft(long rowId, int aircraftId)
        {
            EnsureDraft();

            var stored = Find(_aircraft, rowId);
            var row = new ProvisionAircraft(rowId, Id, aircraftId);

            EnsureAbsent(_aircraft.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(aircraftId);
        }

        public void RemoveAircraft(long rowId)
        {
            EnsureDraft();

            var stored = Find(_aircraft, rowId);

            EnsureSeatSelectors(Application.Type, _seatNumbers.Count, _seatCharacteristics.Count, _aircraft.Count - 1);
            _aircraft.Remove(stored);
        }

        public ProvisionAirFare AddAirFare(long airFareId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionAirFare(idGenerator.NewId(), Id, airFareId);

            EnsureAbsent(_airFares, row, (left, right) => left.SameAs(right));
            _airFares.Add(row);

            return row;
        }

        public void ChangeAirFare(long rowId, long airFareId)
        {
            EnsureDraft();

            var stored = Find(_airFares, rowId);
            var row = new ProvisionAirFare(rowId, Id, airFareId);

            EnsureAbsent(_airFares.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airFareId);
        }

        public void RemoveAirFare(long rowId)
        {
            EnsureDraft();

            var stored = Find(_airFares, rowId);

            _airFares.Remove(stored);
        }

        public ProvisionAirFareType AddAirFareType(AirFareType airFareType, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionAirFareType(idGenerator.NewId(), Id, airFareType);

            EnsureAbsent(_airFareTypes, row, (left, right) => left.SameAs(right));
            _airFareTypes.Add(row);

            return row;
        }

        public void ChangeAirFareType(long rowId, AirFareType airFareType)
        {
            EnsureDraft();

            var stored = Find(_airFareTypes, rowId);
            var row = new ProvisionAirFareType(rowId, Id, airFareType);

            EnsureAbsent(_airFareTypes.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(airFareType);
        }

        public void RemoveAirFareType(long rowId)
        {
            EnsureDraft();

            var stored = Find(_airFareTypes, rowId);

            _airFareTypes.Remove(stored);
        }

        public ProvisionFareFamily AddFareFamily(long fareFamilyId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionFareFamily(idGenerator.NewId(), Id, fareFamilyId);

            EnsureAbsent(_fareFamilies, row, (left, right) => left.SameAs(right));
            _fareFamilies.Add(row);

            return row;
        }

        public void ChangeFareFamily(long rowId, long fareFamilyId)
        {
            EnsureDraft();

            var stored = Find(_fareFamilies, rowId);
            var row = new ProvisionFareFamily(rowId, Id, fareFamilyId);

            EnsureAbsent(_fareFamilies.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(fareFamilyId);
        }

        public void RemoveFareFamily(long rowId)
        {
            EnsureDraft();

            var stored = Find(_fareFamilies, rowId);

            _fareFamilies.Remove(stored);
        }

        public ProvisionFareBasis AddFareBasis(string fareBasisCode, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionFareBasis(idGenerator.NewId(), Id, fareBasisCode);

            EnsureAbsent(_fareBases, row, (left, right) => left.SameAs(right));
            _fareBases.Add(row);

            return row;
        }

        public void ChangeFareBasis(long rowId, string fareBasisCode)
        {
            EnsureDraft();

            var stored = Find(_fareBases, rowId);
            var row = new ProvisionFareBasis(rowId, Id, fareBasisCode);

            EnsureAbsent(_fareBases.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(fareBasisCode);
        }

        public void RemoveFareBasis(long rowId)
        {
            EnsureDraft();

            var stored = Find(_fareBases, rowId);

            _fareBases.Remove(stored);
        }

        public ProvisionCabinClass AddCabinClass(int cabinClassId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionCabinClass(idGenerator.NewId(), Id, cabinClassId);

            EnsureAbsent(_cabinClasses, row, (left, right) => left.SameAs(right));
            _cabinClasses.Add(row);

            return row;
        }

        public void ChangeCabinClass(long rowId, int cabinClassId)
        {
            EnsureDraft();

            var stored = Find(_cabinClasses, rowId);
            var row = new ProvisionCabinClass(rowId, Id, cabinClassId);

            EnsureAbsent(_cabinClasses.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(cabinClassId);
        }

        public void RemoveCabinClass(long rowId)
        {
            EnsureDraft();

            var stored = Find(_cabinClasses, rowId);

            _cabinClasses.Remove(stored);
        }

        public ProvisionRbd AddRbd(long rbdId, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionRbd(idGenerator.NewId(), Id, rbdId);

            EnsureAbsent(_rbds, row, (left, right) => left.SameAs(right));
            _rbds.Add(row);

            return row;
        }

        public void ChangeRbd(long rowId, long rbdId)
        {
            EnsureDraft();

            var stored = Find(_rbds, rowId);
            var row = new ProvisionRbd(rowId, Id, rbdId);

            EnsureAbsent(_rbds.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(rbdId);
        }

        public void RemoveRbd(long rowId)
        {
            EnsureDraft();

            var stored = Find(_rbds, rowId);

            _rbds.Remove(stored);
        }

        public ProvisionTravelDate AddTravelDate(DateOnly travelDate, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionTravelDate(idGenerator.NewId(), Id, travelDate);

            EnsureAbsent(_travelDates, row, (left, right) => left.SameAs(right));
            _travelDates.Add(row);

            return row;
        }

        public void ChangeTravelDate(long rowId, DateOnly travelDate)
        {
            EnsureDraft();

            var stored = Find(_travelDates, rowId);
            var row = new ProvisionTravelDate(rowId, Id, travelDate);

            EnsureAbsent(_travelDates.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(travelDate);
        }

        public void RemoveTravelDate(long rowId)
        {
            EnsureDraft();

            var stored = Find(_travelDates, rowId);

            _travelDates.Remove(stored);
        }

        public ProvisionSeasonalPeriod AddSeasonalPeriod(ProvisionDatePeriodArgs args, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionSeasonalPeriod(idGenerator.NewId(), Id, args);

            EnsureAbsent(_seasonalPeriods, row, (left, right) => left.SameAs(right));
            _seasonalPeriods.Add(row);

            return row;
        }

        public void ChangeSeasonalPeriod(long rowId, ProvisionDatePeriodArgs args)
        {
            EnsureDraft();

            var stored = Find(_seasonalPeriods, rowId);
            var row = new ProvisionSeasonalPeriod(rowId, Id, args);

            EnsureAbsent(_seasonalPeriods.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(args);
        }

        public void RemoveSeasonalPeriod(long rowId)
        {
            EnsureDraft();

            var stored = Find(_seasonalPeriods, rowId);

            _seasonalPeriods.Remove(stored);
        }

        public ProvisionBlackoutPeriod AddBlackoutPeriod(ProvisionDatePeriodArgs args, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionBlackoutPeriod(idGenerator.NewId(), Id, args);

            EnsureAbsent(_blackoutPeriods, row, (left, right) => left.SameAs(right));
            _blackoutPeriods.Add(row);

            return row;
        }

        public void ChangeBlackoutPeriod(long rowId, ProvisionDatePeriodArgs args)
        {
            EnsureDraft();

            var stored = Find(_blackoutPeriods, rowId);
            var row = new ProvisionBlackoutPeriod(rowId, Id, args);

            EnsureAbsent(_blackoutPeriods.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(args);
        }

        public void RemoveBlackoutPeriod(long rowId)
        {
            EnsureDraft();

            var stored = Find(_blackoutPeriods, rowId);

            _blackoutPeriods.Remove(stored);
        }

        public ProvisionDayTimeRestriction AddDayTimeRestriction(ProvisionDayTimeRestrictionArgs args, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionDayTimeRestriction(idGenerator.NewId(), Id, args);

            EnsureAbsent(_dayTimeRestrictions, row, (left, right) => left.SameAs(right));
            _dayTimeRestrictions.Add(row);

            return row;
        }

        public void ChangeDayTimeRestriction(long rowId, ProvisionDayTimeRestrictionArgs args)
        {
            EnsureDraft();

            var stored = Find(_dayTimeRestrictions, rowId);
            var row = new ProvisionDayTimeRestriction(rowId, Id, args);

            EnsureAbsent(_dayTimeRestrictions.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(args);
        }

        public void RemoveDayTimeRestriction(long rowId)
        {
            EnsureDraft();

            var stored = Find(_dayTimeRestrictions, rowId);

            _dayTimeRestrictions.Remove(stored);
        }

        public ProvisionSeatNumber AddSeatNumber(string seatNumber, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionSeatNumber(idGenerator.NewId(), Id, seatNumber);

            EnsureAbsent(_seatNumbers, row, (left, right) => left.SameAs(right));
            EnsureSeatSelectors(Application.Type, _seatNumbers.Count + 1, _seatCharacteristics.Count, _aircraft.Count);
            _seatNumbers.Add(row);

            return row;
        }

        public void ChangeSeatNumber(long rowId, string seatNumber)
        {
            EnsureDraft();

            var stored = Find(_seatNumbers, rowId);
            var row = new ProvisionSeatNumber(rowId, Id, seatNumber);

            EnsureAbsent(_seatNumbers.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(seatNumber);
        }

        public void RemoveSeatNumber(long rowId)
        {
            EnsureDraft();

            var stored = Find(_seatNumbers, rowId);

            EnsureSeatSelectors(Application.Type, _seatNumbers.Count - 1, _seatCharacteristics.Count, _aircraft.Count);
            _seatNumbers.Remove(stored);
        }

        public ProvisionSeatCharacteristic AddSeatCharacteristic(string characteristicCode, IIdGenerator idGenerator)
        {
            EnsureDraft();

            var row = new ProvisionSeatCharacteristic(idGenerator.NewId(), Id, characteristicCode);

            EnsureAbsent(_seatCharacteristics, row, (left, right) => left.SameAs(right));
            EnsureSeatSelectors(Application.Type, _seatNumbers.Count, _seatCharacteristics.Count + 1, _aircraft.Count);
            _seatCharacteristics.Add(row);

            return row;
        }

        public void ChangeSeatCharacteristic(long rowId, string characteristicCode)
        {
            EnsureDraft();

            var stored = Find(_seatCharacteristics, rowId);
            var row = new ProvisionSeatCharacteristic(rowId, Id, characteristicCode);

            EnsureAbsent(_seatCharacteristics.Where(other => other.Id != rowId), row, (left, right) => left.SameAs(right));
            stored.Change(characteristicCode);
        }

        public void RemoveSeatCharacteristic(long rowId)
        {
            EnsureDraft();

            var stored = Find(_seatCharacteristics, rowId);

            EnsureSeatSelectors(Application.Type, _seatNumbers.Count, _seatCharacteristics.Count - 1, _aircraft.Count);
            _seatCharacteristics.Remove(stored);
        }

        private ConditionRows Plan(ProvisionConditionsArgs conditions, IIdGenerator idGenerator)
            => new()
            {
                PassengerTypes = Merge(
                    _passengerTypes,
                    conditions.PassengerTypeCodes.Select(value => new ProvisionPassengerType(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(PassengerTypes)),
                PointsOfSale = Merge(
                    _pointsOfSale,
                    conditions.PointOfSaleIds.Select(value => new ProvisionPointOfSale(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(PointsOfSale)),
                Customers = Merge(
                    _customers,
                    conditions.CustomerIds.Select(value => new ProvisionCustomer(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(Customers)),
                CustomerTypes = Merge(
                    _customerTypes,
                    conditions.CustomerTypes.Select(value => new ProvisionCustomerType(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(CustomerTypes)),
                OriginAirports = Merge(
                    _originAirports,
                    conditions.OriginAirportIds.Select(value => new ProvisionOriginAirport(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(OriginAirports)),
                DestinationAirports = Merge(
                    _destinationAirports,
                    conditions.DestinationAirportIds.Select(value => new ProvisionDestinationAirport(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(DestinationAirports)),
                ViaAirports = Merge(
                    _viaAirports,
                    conditions.ViaAirportIds.Select(value => new ProvisionViaAirport(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(ViaAirports)),
                RoutePairs = Merge(
                    _routePairs,
                    conditions.RoutePairs.Select(value => new ProvisionRoutePair(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.Overlaps(right),
                    nameof(RoutePairs)),
                MarketingAirlines = Merge(
                    _marketingAirlines,
                    conditions.MarketingAirlineIds.Select(value => new ProvisionMarketingAirline(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(MarketingAirlines)),
                OperatingAirlines = Merge(
                    _operatingAirlines,
                    conditions.OperatingAirlineIds.Select(value => new ProvisionOperatingAirline(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(OperatingAirlines)),
                FlightNumbers = Merge(
                    _flightNumbers,
                    conditions.FlightNumbers.Select(value => new ProvisionFlightNumber(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(FlightNumbers)),
                Flights = Merge(
                    _flights,
                    conditions.FlightIds.Select(value => new ProvisionFlight(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(Flights)),
                Aircraft = Merge(
                    _aircraft,
                    conditions.AircraftIds.Select(value => new ProvisionAircraft(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(Aircraft)),
                AirFares = Merge(
                    _airFares,
                    conditions.AirFareIds.Select(value => new ProvisionAirFare(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(AirFares)),
                AirFareTypes = Merge(
                    _airFareTypes,
                    conditions.AirFareTypes.Select(value => new ProvisionAirFareType(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(AirFareTypes)),
                FareFamilies = Merge(
                    _fareFamilies,
                    conditions.FareFamilyIds.Select(value => new ProvisionFareFamily(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(FareFamilies)),
                FareBases = Merge(
                    _fareBases,
                    conditions.FareBasisCodes.Select(value => new ProvisionFareBasis(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(FareBases)),
                CabinClasses = Merge(
                    _cabinClasses,
                    conditions.CabinClassIds.Select(value => new ProvisionCabinClass(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(CabinClasses)),
                Rbds = Merge(
                    _rbds,
                    conditions.RbdIds.Select(value => new ProvisionRbd(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(Rbds)),
                TravelDates = Merge(
                    _travelDates,
                    conditions.TravelDates.Select(value => new ProvisionTravelDate(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(TravelDates)),
                SeasonalPeriods = Merge(
                    _seasonalPeriods,
                    conditions.SeasonalPeriods.Select(value => new ProvisionSeasonalPeriod(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(SeasonalPeriods)),
                BlackoutPeriods = Merge(
                    _blackoutPeriods,
                    conditions.BlackoutPeriods.Select(value => new ProvisionBlackoutPeriod(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(BlackoutPeriods)),
                DayTimeRestrictions = Merge(
                    _dayTimeRestrictions,
                    conditions.DayTimeRestrictions.Select(value => new ProvisionDayTimeRestriction(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(DayTimeRestrictions)),
                SeatNumbers = Merge(
                    _seatNumbers,
                    conditions.SeatNumbers.Select(value => new ProvisionSeatNumber(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(SeatNumbers)),
                SeatCharacteristics = Merge(
                    _seatCharacteristics,
                    conditions.SeatCharacteristicCodes.Select(value => new ProvisionSeatCharacteristic(idGenerator.NewId(), Id, value)).ToList(),
                    (left, right) => left.SameAs(right),
                    (left, right) => left.SameAs(right),
                    nameof(SeatCharacteristics))
            };

        private void Commit(ConditionRows rows)
        {
            Replace(_passengerTypes, rows.PassengerTypes);
            Replace(_pointsOfSale, rows.PointsOfSale);
            Replace(_customers, rows.Customers);
            Replace(_customerTypes, rows.CustomerTypes);
            Replace(_originAirports, rows.OriginAirports);
            Replace(_destinationAirports, rows.DestinationAirports);
            Replace(_viaAirports, rows.ViaAirports);
            Replace(_routePairs, rows.RoutePairs);
            Replace(_marketingAirlines, rows.MarketingAirlines);
            Replace(_operatingAirlines, rows.OperatingAirlines);
            Replace(_flightNumbers, rows.FlightNumbers);
            Replace(_flights, rows.Flights);
            Replace(_aircraft, rows.Aircraft);
            Replace(_airFares, rows.AirFares);
            Replace(_airFareTypes, rows.AirFareTypes);
            Replace(_fareFamilies, rows.FareFamilies);
            Replace(_fareBases, rows.FareBases);
            Replace(_cabinClasses, rows.CabinClasses);
            Replace(_rbds, rows.Rbds);
            Replace(_travelDates, rows.TravelDates);
            Replace(_seasonalPeriods, rows.SeasonalPeriods);
            Replace(_blackoutPeriods, rows.BlackoutPeriods);
            Replace(_dayTimeRestrictions, rows.DayTimeRestrictions);
            Replace(_seatNumbers, rows.SeatNumbers);
            Replace(_seatCharacteristics, rows.SeatCharacteristics);
        }

        private static List<TRow> Merge<TRow>(
            List<TRow> stored,
            List<TRow> candidates,
            Func<TRow, TRow, bool> same,
            Func<TRow, TRow, bool> conflicts,
            string field)
            where TRow : class
        {
            for (var index = 1; index < candidates.Count; index++)
            {
                var candidate = candidates[index];

                if (candidates.Take(index).Any(previous => conflicts(previous, candidate)))
                    throw ExceptionFactory.ProvisionIsInvalid(field);
            }

            return candidates
                .Select(candidate => stored.FirstOrDefault(row => same(row, candidate)) ?? candidate)
                .ToList();
        }

        private static void Replace<TRow>(List<TRow> stored, List<TRow> rows)
        {
            stored.Clear();
            stored.AddRange(rows);
        }

        private static TRow Find<TRow>(List<TRow> rows, long rowId)
            where TRow : Entity<long>
            => rows.FirstOrDefault(row => row.Id == rowId) ?? throw ExceptionFactory.ProvisionConditionNotFound();

        private static void EnsureAbsent<TRow>(IEnumerable<TRow> rows, TRow candidate, Func<TRow, TRow, bool> conflicts)
        {
            if (rows.Any(row => conflicts(row, candidate)))
                throw ExceptionFactory.ProvisionConditionAlreadyExists();
        }

        private static void EnsureSeatSelectors(ProvisionApplicationType type, int seatNumbers, int seatCharacteristics, int aircraft)
        {
            Require(
                type == ProvisionApplicationType.Seat ? seatNumbers + seatCharacteristics > 0 : seatNumbers + seatCharacteristics == 0,
                nameof(SeatNumbers));
            Require(seatNumbers == 0 || aircraft > 0, nameof(Aircraft));
        }

        private sealed class ConditionRows
        {
            public List<ProvisionPassengerType> PassengerTypes { get; init; } = new();

            public List<ProvisionPointOfSale> PointsOfSale { get; init; } = new();

            public List<ProvisionCustomer> Customers { get; init; } = new();

            public List<ProvisionCustomerType> CustomerTypes { get; init; } = new();

            public List<ProvisionOriginAirport> OriginAirports { get; init; } = new();

            public List<ProvisionDestinationAirport> DestinationAirports { get; init; } = new();

            public List<ProvisionViaAirport> ViaAirports { get; init; } = new();

            public List<ProvisionRoutePair> RoutePairs { get; init; } = new();

            public List<ProvisionMarketingAirline> MarketingAirlines { get; init; } = new();

            public List<ProvisionOperatingAirline> OperatingAirlines { get; init; } = new();

            public List<ProvisionFlightNumber> FlightNumbers { get; init; } = new();

            public List<ProvisionFlight> Flights { get; init; } = new();

            public List<ProvisionAircraft> Aircraft { get; init; } = new();

            public List<ProvisionAirFare> AirFares { get; init; } = new();

            public List<ProvisionAirFareType> AirFareTypes { get; init; } = new();

            public List<ProvisionFareFamily> FareFamilies { get; init; } = new();

            public List<ProvisionFareBasis> FareBases { get; init; } = new();

            public List<ProvisionCabinClass> CabinClasses { get; init; } = new();

            public List<ProvisionRbd> Rbds { get; init; } = new();

            public List<ProvisionTravelDate> TravelDates { get; init; } = new();

            public List<ProvisionSeasonalPeriod> SeasonalPeriods { get; init; } = new();

            public List<ProvisionBlackoutPeriod> BlackoutPeriods { get; init; } = new();

            public List<ProvisionDayTimeRestriction> DayTimeRestrictions { get; init; } = new();

            public List<ProvisionSeatNumber> SeatNumbers { get; init; } = new();

            public List<ProvisionSeatCharacteristic> SeatCharacteristics { get; init; } = new();
        }
    }
}
