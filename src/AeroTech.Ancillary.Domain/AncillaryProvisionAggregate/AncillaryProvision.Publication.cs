using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision
    {
        private static readonly IReadOnlyDictionary<PricingUnit, AncillaryQuantityUnit> QuantityUnits =
            new Dictionary<PricingUnit, AncillaryQuantityUnit>
            {
                [PricingUnit.PerPassenger] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerRoom] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerItem] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerVehicle] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerSeat] = AncillaryQuantityUnit.Each,
                [PricingUnit.PerPiece] = AncillaryQuantityUnit.Piece,
                [PricingUnit.PerKilogram] = AncillaryQuantityUnit.Kilogram
            };

        private static readonly TimeUnit[] ActivatableAdvancePurchaseUnits =
        [
            TimeUnit.Minutes,
            TimeUnit.Hours,
            TimeUnit.Days,
            TimeUnit.Months
        ];

        private bool IsFlightBound
            => ApplicationType != ProvisionApplicationType.Standard
               || FlightApplication is not null
               || FareApplication is not null
               || AdvancePurchase is { SameTimeAsTicketed: true }
               || Geography is { OriginAirports.Count: > 0 }
                   or { DestinationAirports.Count: > 0 }
                   or { ViaAirports.Count: > 0 }
                   or { RoutePairs.Count: > 0 };

        private bool NeedsServiceOccurrence
            => IsFlightBound
               || TravelDate is not null
               || DayTimeApplication is not null
               || AdvancePurchase is not null
               || PassengerEligibility is { AgeBands.Count: > 0 };

        public void EnsurePublishable(AncillaryServiceDefinition definition)
        {
            Require(definition.Id == ServiceDefinitionId, nameof(ServiceDefinitionId));

            var pricingUnit = definition.PricingUnit ?? throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();

            if (QuantityUnits[pricingUnit] != Quantity.Unit)
                throw ExceptionFactory.ProvisionQuantityUnitIncompatible(pricingUnit, Quantity.Unit);

            if (NeedsServiceOccurrence && definition.ServiceDateBasis is null)
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisNotAssigned();

            if (IsFlightBound && definition.ServiceDateBasis != ServiceDateBasis.FlightDeparture)
                throw ExceptionFactory.ProvisionRuleContextNotSupported(definition.ServiceDateBasis);

            if (AdvancePurchase is not null && !ActivatableAdvancePurchaseUnits.Contains(AdvancePurchase.Unit))
                throw ExceptionFactory.ProvisionAdvancePurchaseUnitNotSupported(AdvancePurchase.Unit);

            if (TravelDate is { IsUnsatisfiable: true })
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(TravelDate));

            if (DayTimeApplication is { IsUnsatisfiable: true })
                throw ExceptionFactory.ProvisionRuleUnreachable(nameof(DayTimeApplication));
        }

        private void EnsureDescriptorsAreStated()
        {
            if (PurchaseStage == PurchaseStage.LegacyUnspecified)
                throw ExceptionFactory.ProvisionPurchaseStageRequired();

            if (BaggageApplication is { ChargeKind: null })
                throw ExceptionFactory.ProvisionBaggageChargeKindRequired();
        }
    }
}
