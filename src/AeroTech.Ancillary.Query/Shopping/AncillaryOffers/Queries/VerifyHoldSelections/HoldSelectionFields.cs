using System.Text.Json;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Options;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyHoldSelections
{
    public static class HoldSelectionFields
    {
        public static void Check(IReadOnlyList<SelectionFieldDescriptor> fields, ISelectionOptions? options)
        {
            var values = Values(options);

            if (values.Keys.FirstOrDefault(name => fields.All(field => field.Name != name)) is { } unexpected)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(unexpected);

            if (fields.FirstOrDefault(field => !Fits(field, values.GetValueOrDefault(field.Name))) is { } refused)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(refused.Name);
        }

        private static bool Fits(SelectionFieldDescriptor field, object? value)
            => value switch
            {
                null => !field.Required,
                string text => field.Choices is null || field.Choices.Any(choice => choice.Value == text),
                decimal number => InRange(field, number),
                int number => InRange(field, number),
                SelectedDimensionsInput size => field.Fields is null || Fits(field.Fields, size),
                _ => true
            };

        private static bool Fits(IReadOnlyList<SelectionFieldDescriptor> parts, SelectedDimensionsInput size)
            => new[] { Value(nameof(size.LengthCm), size.LengthCm), Value(nameof(size.WidthCm), size.WidthCm), Value(nameof(size.HeightCm), size.HeightCm) }
                .All(part => parts.FirstOrDefault(descriptor => descriptor.Name == part.Name) is not { } descriptor || InRange(descriptor, (decimal)part.Value!));

        private static bool InRange(SelectionFieldDescriptor field, decimal value)
            => (field.Min is null || value >= field.Min)
               && (field.ExclusiveMin is null || value > field.ExclusiveMin)
               && (field.Max is null || value <= field.Max);

        private static IReadOnlyDictionary<string, object> Values(ISelectionOptions? options)
        {
            (string Name, object? Value)[] provided = options switch
            {
                BaggageSelectionOptions baggage =>
                [
                    Value(nameof(baggage.BagRef), baggage.BagRef),
                    Value(nameof(baggage.MeasuredWeightKg), baggage.MeasuredWeightKg),
                    Value(nameof(baggage.Dimensions), baggage.Dimensions),
                    Value(nameof(baggage.WeightKg), baggage.WeightKg)
                ],
                AssistedTravelHoldSelectionOptions assistedTravel => [Value(nameof(assistedTravel.AssistanceSsrCode), assistedTravel.AssistanceSsrCode)],
                AirportServiceSelectionOptions airportService =>
                [
                    Value(nameof(airportService.TimeWithOffset), airportService.TimeWithOffset),
                    Value(nameof(airportService.GuestCount), airportService.GuestCount)
                ],
                PrioritySelectionOptions priority => [Value(nameof(priority.OptIn), priority.OptIn)],
                ConnectivitySelectionOptions connectivity => [Value(nameof(connectivity.DeviceCount), connectivity.DeviceCount)],
                _ => []
            };

            return provided
                .Where(entry => entry.Value is not null)
                .ToDictionary(entry => entry.Name, entry => entry.Value!, StringComparer.Ordinal);
        }

        private static (string Name, object? Value) Value(string member, object? value) => (JsonNamingPolicy.CamelCase.ConvertName(member), value);
    }
}
