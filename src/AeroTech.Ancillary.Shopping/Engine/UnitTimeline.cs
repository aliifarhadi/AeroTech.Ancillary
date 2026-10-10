using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed class UnitTimeline
    {
        public UnitTimeline(AncillaryServiceDefinition definition, EvaluationUnit unit)
        {
            Occurrences = Resolve(definition, unit);

            if (Occurrences is null)
                return;

            var traveller = unit.Traveller;

            if (traveller.DateOfBirth is { } dateOfBirth)
                Ages = Occurrences.Select(occurrence => ServiceOccurrence.CompletedYears(dateOfBirth, occurrence.LocalDate)).ToList();
            else if (traveller.VerifiedAgeAtTravel is { } age && Occurrences.All(occurrence => occurrence.LocalDate == traveller.AgeEvidenceAsOfDate))
                Ages = Occurrences.Select(_ => age).ToList();
        }

        public IReadOnlyList<ServiceOccurrence>? Occurrences { get; }

        public string? OccurrenceReason { get; private set; }

        public string? OccurrenceField { get; private set; }

        public bool OccurrencePendingSelection { get; private set; }

        public IReadOnlyList<int>? Ages { get; }

        private IReadOnlyList<ServiceOccurrence>? Resolve(AncillaryServiceDefinition definition, EvaluationUnit unit)
        {
            switch (definition.ServiceDateBasis)
            {
                case ServiceDateBasis.FlightDeparture:
                    var occurrences = new List<ServiceOccurrence>();

                    foreach (var flight in unit.Flights)
                    {
                        if (!ServiceOccurrence.TryResolve(flight.DepartureAt, flight.OriginIanaTimeZoneId, out var occurrence))
                            return Unresolved(ShoppingReasonCodes.TimeZoneNotVerified, $"Flights[{flight.FlightRef}].OriginIanaTimeZoneId", false);

                        occurrences.Add(occurrence);
                    }

                    return occurrences;
                case ServiceDateBasis.ServiceStart:
                    if (unit.Selection?.TimeWithOffset is not { } time)
                        return Unresolved(ShoppingReasonCodes.AppointmentRequired, "Selection.TimeWithOffset", true);

                    return ServiceOccurrence.TryResolve(time, definition.AirportService?.IanaTimeZone, out var service)
                        ? [service]
                        : Unresolved(ShoppingReasonCodes.TimeZoneNotVerified, "Specification.IanaTimeZone", false);
                default:
                    return Unresolved(ShoppingReasonCodes.ServiceDateBasisNotSupported, nameof(AncillaryServiceDefinition.ServiceDateBasis), false);
            }
        }

        private IReadOnlyList<ServiceOccurrence>? Unresolved(string reason, string field, bool pendingSelection)
        {
            OccurrenceReason = reason;
            OccurrenceField = field;
            OccurrencePendingSelection = pendingSelection;

            return null;
        }
    }
}
