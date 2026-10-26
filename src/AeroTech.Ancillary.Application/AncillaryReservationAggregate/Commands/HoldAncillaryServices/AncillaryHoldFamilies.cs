using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    internal static class AncillaryHoldFamilies
    {
        public static IReadOnlyList<HoldSelection> Selections(IHoldAncillaryServicesCommand command)
            => OptionFamily(AncillaryProfile.Baggage, command.Baggages, options => options.Baggage)
                .Concat(QuantityFamily(AncillaryProfile.Meal, command.Meals))
                .Concat(OptionFamily(AncillaryProfile.AssistedTravel, command.AssistedTravel, options => options.AssistedTravel))
                .Concat(OptionFamily(AncillaryProfile.AirportService, command.AirportServices, options => options.AirportService))
                .Concat(OptionFamily(AncillaryProfile.Priority, command.PriorityServices, options => options.Priority))
                .Concat(OptionFamily(AncillaryProfile.Connectivity, command.Connectivity, options => options.Connectivity))
                .Concat(QuantityFamily(AncillaryProfile.Upgrade, command.Upgrades))
                .ToList();

        public static IReadOnlyList<long> OrderServiceIds(IHoldAncillaryServicesCommand command)
            => OrderServiceIdsOf(command.Baggages)
                .Concat(OrderServiceIdsOf(command.Meals))
                .Concat(OrderServiceIdsOf(command.AssistedTravel))
                .Concat(OrderServiceIdsOf(command.AirportServices))
                .Concat(OrderServiceIdsOf(command.PriorityServices))
                .Concat(OrderServiceIdsOf(command.Connectivity))
                .Concat(OrderServiceIdsOf(command.Upgrades))
                .ToList();

        private static IEnumerable<HoldSelection> OptionFamily<TOptions>(
            AncillaryProfile family,
            IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<TOptions>>>? variants,
            Func<TOptions, ISelectionOptions> options)
            where TOptions : class
            => Family(family, variants, selected => selected.Quantity, selected => selected.Options is { } chosen ? options(chosen) : null);

        private static IEnumerable<HoldSelection> QuantityFamily(AncillaryProfile family, IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? variants)
            => Family(family, variants, selected => selected.Quantity, _ => null);

        private static IEnumerable<HoldSelection> Family<TSelected>(
            AncillaryProfile family,
            IReadOnlyList<AncillaryVariantHoldInput<TSelected>>? variants,
            Func<TSelected, int?> quantity,
            Func<TSelected, ISelectionOptions?> options)
            where TSelected : class
            => (variants ?? [])
                .SelectMany(variant => variant.Products
                    .SelectMany(product => product.Units
                        .Select(unit => new HoldSelection(
                            family,
                            variant.VariantCode,
                            product.ProvisionId,
                            unit.OrderServiceId,
                            unit.Unit?.TravellerId,
                            unit.Coverage?.FlightIds ?? [],
                            unit.Selected is { } selected ? quantity(selected) : null,
                            unit.Selected is { } chosen ? options(chosen) : null))));

        private static IEnumerable<long> OrderServiceIdsOf<TSelected>(IReadOnlyList<AncillaryVariantHoldInput<TSelected>>? variants)
            where TSelected : class
            => (variants ?? [])
                .SelectMany(variant => variant?.Products ?? [])
                .SelectMany(product => product?.Units ?? [])
                .Where(unit => unit is not null)
                .Select(unit => unit.OrderServiceId);
    }
}
