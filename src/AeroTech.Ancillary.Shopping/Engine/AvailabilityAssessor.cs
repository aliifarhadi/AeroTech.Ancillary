using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal static class AvailabilityAssessor
    {
        public static async Task<AvailabilityAssessment> AssessAsync(
            AncillaryProvision provision,
            InventoryConfiguration? configuration,
            EvaluationUnit unit,
            InventoryReferenceProbe references,
            CancellationToken cancellationToken)
        {
            var reasons = new List<string>();
            var mustCheck = provision.Availability.MustCheckAvailability;

            if (mustCheck)
                reasons.Add(ShoppingReasonCodes.AvailabilityMustBeChecked);

            if (configuration is null)
                return Assessment(null, InventoryCapacityReadState.NotConfigured, AvailabilityCheckState.NotChecked, mustCheck, null, reasons, ShoppingReasonCodes.InventoryNotConfigured);

            var policy = configuration.Policy;
            var unsupported = policy.LocalPattern is LocalInventoryPattern.DailyCount or LocalInventoryPattern.RoomNight or LocalInventoryPattern.AssignedAsset;

            if (policy.Status == InventoryRecordStatus.Draft)
            {
                return unsupported
                    ? Assessment(policy.Authority, InventoryCapacityReadState.UnsupportedPattern, AvailabilityCheckState.NotChecked, true, null, reasons, ShoppingReasonCodes.InventoryPatternNotSupported)
                    : Assessment(policy.Authority, InventoryCapacityReadState.NotConfigured, AvailabilityCheckState.NotChecked, true, null, reasons, ShoppingReasonCodes.InventoryNotConfigured);
            }

            if (policy.Status == InventoryRecordStatus.Suspended)
                return Assessment(policy.Authority, InventoryCapacityReadState.ClosedForSale, AvailabilityCheckState.Unavailable, true, null, reasons, ShoppingReasonCodes.InventoryClosedForSale);

            switch (policy.Authority)
            {
                case InventoryAuthority.Unlimited:
                    return Assessment(policy.Authority, InventoryCapacityReadState.Unlimited, AvailabilityCheckState.NotChecked, mustCheck, null, reasons);
                case InventoryAuthority.Supplier:
                    return Assessment(
                        policy.Authority,
                        InventoryCapacityReadState.DelegatedCheckRequired,
                        AvailabilityCheckState.NotChecked,
                        true,
                        null,
                        reasons,
                        ShoppingReasonCodes.InventoryDelegatedCheckRequired);
                case InventoryAuthority.FlightFlow:
                    var delegation = await references.FlightFlowAsync(policy.ProviderKey!, cancellationToken);

                    return Assessment(
                        policy.Authority,
                        InventoryCapacityReadState.DelegatedCheckRequired,
                        Checked(delegation, reasons),
                        true,
                        null,
                        reasons,
                        ShoppingReasonCodes.InventoryDelegatedCheckRequired);
                default:
                    return unsupported
                        ? Assessment(policy.Authority, InventoryCapacityReadState.UnsupportedPattern, AvailabilityCheckState.NotChecked, true, null, reasons, ShoppingReasonCodes.InventoryPatternNotSupported)
                        : policy.LocalPattern == LocalInventoryPattern.AirportSlot
                            ? await SlotAsync(policy, configuration, unit, references, reasons, cancellationToken)
                            : await FlightAsync(policy, configuration, unit, references, reasons, cancellationToken);
            }
        }

        private static async Task<AvailabilityAssessment> FlightAsync(
            AncillaryInventoryPolicy policy,
            InventoryConfiguration configuration,
            EvaluationUnit unit,
            InventoryReferenceProbe references,
            List<string> reasons,
            CancellationToken cancellationToken)
        {
            var quantity = Math.Max(unit.Selection?.Quantity ?? 1, 1);
            var needed = new List<(InventoryResourceKind Kind, long ResourceId)>();

            if (policy.CountConsumption is { } count)
                needed.Add((InventoryResourceKind.FlightCount, count.ResourceId));

            if (policy.WeightConsumption is { } weight)
                needed.Add((InventoryResourceKind.FlightWeight, weight.WeightResourceId));

            var preview = new ConsumptionPreview(
                policy.CountConsumption?.RequiredUnits(quantity),
                policy.CountConsumption?.CountUnit,
                policy.WeightConsumption is { ConsumptionMode: FlightWeightConsumptionMode.FixedKgPerAcceptedUnit } fixedWeight ? fixedWeight.RequiredKg(quantity, null) : null,
                null,
                null);
            var state = InventoryCapacityReadState.ConfiguredNotGuaranteed;

            foreach (var flight in unit.Flights)
            {
                foreach (var (kind, resourceId) in needed)
                {
                    var source = configuration.FlightSources.FirstOrDefault(row => row.Kind == kind && row.FlightId == flight.FlightId && row.ResourceId == resourceId);

                    if (source is null || source.Status == InventoryRecordStatus.Draft)
                        state = state == InventoryCapacityReadState.ClosedForSale ? state : InventoryCapacityReadState.NotConfigured;
                    else if (source.ClosedForSale || source.Status == InventoryRecordStatus.Suspended)
                        state = InventoryCapacityReadState.ClosedForSale;
                }
            }

            var checkedState = AvailabilityCheckState.NotChecked;

            foreach (var (kind, resourceId) in needed)
            {
                var answer = Checked(await references.ResourceAsync(kind, resourceId, cancellationToken), reasons);

                if (answer != AvailabilityCheckState.NotChecked)
                    checkedState = answer;
            }

            return state switch
            {
                InventoryCapacityReadState.ClosedForSale
                    => Assessment(policy.Authority, state, AvailabilityCheckState.Unavailable, true, preview, reasons, ShoppingReasonCodes.InventoryClosedForSale),
                InventoryCapacityReadState.NotConfigured
                    => Assessment(policy.Authority, state, checkedState, true, preview, reasons, ShoppingReasonCodes.InventorySourceNotConfigured),
                _ => Assessment(policy.Authority, state, checkedState, true, preview, reasons)
            };
        }

        private static async Task<AvailabilityAssessment> SlotAsync(
            AncillaryInventoryPolicy policy,
            InventoryConfiguration configuration,
            EvaluationUnit unit,
            InventoryReferenceProbe references,
            List<string> reasons,
            CancellationToken cancellationToken)
        {
            var consumption = policy.SlotConsumption!;
            var guests = Math.Max(unit.Selection?.GuestCount ?? 0, 0);
            var preview = new ConsumptionPreview(null, null, null, consumption.OccupancyMinutes, consumption.PeoplePerAcceptedUnit * (1 + guests));
            var checkedState = Checked(await references.FacilityAsync(consumption.FacilityId, cancellationToken), reasons);

            if (unit.Selection?.TimeWithOffset is not { } time)
                return Assessment(policy.Authority, InventoryCapacityReadState.Unknown, checkedState, true, preview, reasons, ShoppingReasonCodes.AppointmentRequired);

            var slot = configuration.SlotSources.FirstOrDefault(row => row.StartUtc <= time && time < row.EndUtc);

            if (slot is null || slot.Status == InventoryRecordStatus.Draft)
                return Assessment(policy.Authority, InventoryCapacityReadState.NotConfigured, checkedState, true, preview, reasons, ShoppingReasonCodes.InventorySourceNotConfigured);

            return slot.ClosedForSale || slot.Status == InventoryRecordStatus.Suspended
                ? Assessment(
                    policy.Authority,
                    InventoryCapacityReadState.ClosedForSale,
                    AvailabilityCheckState.Unavailable,
                    true,
                    preview,
                    reasons,
                    ShoppingReasonCodes.InventoryClosedForSale)
                : Assessment(policy.Authority, InventoryCapacityReadState.ConfiguredNotGuaranteed, checkedState, true, preview, reasons);
        }

        private static AvailabilityCheckState Checked(InventoryReferenceCheck answer, List<string> reasons)
        {
            switch (answer)
            {
                case InventoryReferenceCheck.SourceUnavailable:
                    reasons.Add(ShoppingReasonCodes.BlockedExternalReference);

                    return AvailabilityCheckState.SourceUnavailable;
                case InventoryReferenceCheck.NotFound:
                    reasons.Add(ShoppingReasonCodes.InventoryReferenceNotFound);

                    return AvailabilityCheckState.Unknown;
                default:
                    return AvailabilityCheckState.NotChecked;
            }
        }

        private static AvailabilityAssessment Assessment(
            InventoryAuthority? authority,
            InventoryCapacityReadState configState,
            AvailabilityCheckState checkedState,
            bool requiresCheckAtReserve,
            ConsumptionPreview? preview,
            List<string> reasons,
            params string[] extraReasons)
            => new(
                authority,
                configState,
                checkedState,
                false,
                requiresCheckAtReserve,
                null,
                null,
                preview,
                reasons.Concat(extraReasons).Distinct(StringComparer.Ordinal).ToList());
    }
}
