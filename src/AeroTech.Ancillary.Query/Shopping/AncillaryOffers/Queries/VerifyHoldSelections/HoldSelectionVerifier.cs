using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyHoldSelections
{
    public sealed class HoldSelectionVerifier : IHoldSelectionVerifier
    {
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryDefinitionReader _definitions;

        public HoldSelectionVerifier(IActiveAncillaryProvisionReader provisions, IActiveAncillaryDefinitionReader definitions)
        {
            _provisions = provisions;
            _definitions = definitions;
        }

        public async Task<IReadOnlyList<VerifiedHoldSelection>> VerifyAsync(HoldSelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var provisions = (await _provisions.FindActiveProvisionsAsync(
                    verification.Selections.Select(selection => selection.ProvisionId).Distinct().ToList(),
                    cancellationToken))
                .ToDictionary(provision => provision.Id);
            var definitions = (await _definitions.FindActiveDefinitionsAsync(
                    provisions.Values.Select(provision => provision.ServiceDefinitionId).Distinct().ToList(),
                    cancellationToken))
                .ToDictionary(definition => definition.Id);

            return verification.Selections.Select(selection => Verified(selection, provisions, definitions)).ToList();
        }

        private static VerifiedHoldSelection Verified(
            HoldSelection selection,
            IReadOnlyDictionary<long, AncillaryProvision> provisions,
            IReadOnlyDictionary<long, AncillaryServiceDefinition> definitions)
        {
            var provision = provisions.GetValueOrDefault(selection.ProvisionId)
                            ?? throw ExceptionFactory.AncillaryHoldNotReservable($"provision {selection.ProvisionId} is not active");
            var definition = definitions.GetValueOrDefault(provision.ServiceDefinitionId)
                             ?? throw ExceptionFactory.AncillaryHoldNotReservable($"service {provision.ServiceDefinitionId} of provision {provision.Id} is not active");
            var options = selection.Options;

            if (definition.Profile != selection.Family || definition.VariantCode != selection.VariantCode)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(HoldSelection.VariantCode));

            AncillaryHoldChecks.EnsureHoldable(definition, provision);
            HoldSelectionFields.Check(TypedSelectionForm.Fields(definition, provision, [], string.Empty), options);

            if (definition.VariantCode == AncillaryVariant.PriorityBoardingCheckin && options is not PrioritySelectionOptions { OptIn: true })
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(PrioritySelectionOptions.OptIn));

            var chosen = TypedSelectionForm.WithFixedChoices(options is null ? new AncillarySelection() : TypedSelectionMapper.ToSelection(options), definition, provision);

            return new VerifiedHoldSelection(definition.Id, provision.CoverageScope, QuantityOf(selection, provision), SelectionOf(definition.Profile!.Value, chosen));
        }

        private static int QuantityOf(HoldSelection selection, AncillaryProvision provision)
            => selection.Quantity
               ?? (provision.Quantity.MinQuantity == provision.Quantity.MaxQuantity
                   ? provision.Quantity.MinQuantity
                   : throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(HoldSelection.Quantity)));

        private static AncillaryReservationSelectionArgs? SelectionOf(AncillaryProfile family, AncillarySelection chosen)
            => family switch
            {
                AncillaryProfile.Baggage when chosen is { BagRef: null, MeasuredWeightKg: null, Dimensions: null, WeightKg: null, EquipmentKind: null } => null,
                AncillaryProfile.Baggage => new AncillaryReservationSelectionArgs(Baggage: new BaggageSelectionArgs(
                    chosen.BagRef,
                    chosen.MeasuredWeightKg,
                    chosen.Dimensions is { } size ? new SelectedDimensionsArgs(size.LengthCm, size.WidthCm, size.HeightCm) : null,
                    chosen.WeightKg,
                    chosen.EquipmentKind)),
                AncillaryProfile.Meal when chosen is { MealCode: null, MenuItemRef: null } => null,
                AncillaryProfile.Meal => new AncillaryReservationSelectionArgs(Meal: new MealSelectionArgs(chosen.MealCode, chosen.MenuItemRef)),
                AncillaryProfile.AssistedTravel => new AncillaryReservationSelectionArgs(AssistedTravel: new AssistedTravelSelectionArgs(chosen.AssistanceSsrCode)),
                AncillaryProfile.AirportService => new AncillaryReservationSelectionArgs(AirportService: new AirportServiceSelectionArgs(
                    chosen.TimeWithOffset,
                    chosen.GuestCount,
                    chosen.AirportId,
                    chosen.FacilityRef)),
                AncillaryProfile.Connectivity when chosen.DeviceCount is null => null,
                AncillaryProfile.Connectivity => new AncillaryReservationSelectionArgs(Connectivity: new ConnectivitySelectionArgs(chosen.DeviceCount)),
                AncillaryProfile.Upgrade when chosen.TargetCabinId is { } cabin => new AncillaryReservationSelectionArgs(Upgrade: new UpgradeSelectionArgs(cabin)),
                _ => null
            };
    }
}
