using System.Globalization;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail
{
    public static class OfferOptionCatalog
    {
        public static IReadOnlyList<OfferOptionGroup> Build(
            AncillaryServiceDefinition definition,
            AncillaryProvision provision,
            CanonicalAncillaryOfferCandidate candidate,
            AncillaryShoppingContext context)
            => definition.VariantCode switch
            {
                AncillaryVariant.ExtraWeightPackage =>
                [
                    Known("packageProductRef", new OfferOptionChoice(definition.ServiceDefinitionRef, Kilograms(definition.Baggage!.PackageWeightKg)))
                ],
                AncillaryVariant.SpecialEquipment when definition.Baggage!.EquipmentKind is { } equipment =>
                [
                    Known("equipmentKind", new OfferOptionChoice(equipment, equipment))
                ],
                AncillaryVariant.ExtraSeat when definition.Seat!.ExtraSeatPurpose is { } purpose =>
                [
                    Known("purpose", new OfferOptionChoice(purpose.ToString(), purpose.ToString()))
                ],
                AncillaryVariant.CabinUpgrade => Upgrade(definition.Upgrade!),
                AncillaryVariant.FreeSpecialMeal when definition.Meal!.MealCode is { } mealCode =>
                [
                    Known("mealCode", new OfferOptionChoice(mealCode, mealCode))
                ],
                AncillaryVariant.PaidPreorderMeal => definition.Meal!.MenuItemRef is { } menuItem
                    ? [Known("menuItemRef", new OfferOptionChoice(menuItem, menuItem))]
                    : [new OfferOptionGroup("menuItemRef", OptionSourceState.Unknown, [])],
                AncillaryVariant.PetInCabin or AncillaryVariant.PetInHold => Pet(definition.Pet!),
                AncillaryVariant.Wheelchair => Codes("assistanceSsrCode", definition.AssistedTravel!.Wheelchair!.AllowedSsrCodes.Select(code => code.Code)),
                AncillaryVariant.DisabilityAssistance => Codes("assistanceSsrCode", definition.AssistedTravel!.DisabilityAssistance!.AllowedSsrCodes.Select(code => code.Code)),
                AncillaryVariant.MedicalEquipment =>
                [
                    Known("equipmentCode", new OfferOptionChoice(definition.AssistedTravel!.MedicalEquipment!.MedicalServiceCode, definition.AssistedTravel.MedicalEquipment.MedicalServiceCode))
                ],
                AncillaryVariant.Bassinet =>
                [
                    Travellers("infantRef", context.Travellers.Where(traveller => traveller.PassengerTypeCode == PassengerTypeCode.INF)),
                    Travellers("guardianRef", context.Travellers.Where(traveller => traveller.PassengerTypeCode == PassengerTypeCode.ADT))
                ],
                AncillaryVariant.UnaccompaniedMinor =>
                [
                    Travellers("childRef", context.Travellers.Where(traveller => candidate.Travellers.Contains(traveller.TravellerRef, StringComparer.Ordinal)))
                ],
                AncillaryVariant.Lounge or AncillaryVariant.FastTrack or AncillaryVariant.CipMeetAssist => Airport(definition.AirportService!, provision),
                AncillaryVariant.OnboardWifi =>
                [
                    Known("planCode", new OfferOptionChoice(definition.ServiceDefinitionRef, $"{definition.CommercialName} ({definition.Connectivity!.PlanKind})"))
                ],
                _ => []
            };

        public static OptionSourceState StateOf(IReadOnlyList<OfferOptionGroup> groups)
            => groups.Any(group => group.SourceState == OptionSourceState.Unknown)
                ? OptionSourceState.Unknown
                : groups.Any(group => group.SourceState == OptionSourceState.Known)
                    ? OptionSourceState.Known
                    : OptionSourceState.NotApplicable;

        private static IReadOnlyList<OfferOptionGroup> Upgrade(UpgradeSpecification upgrade)
        {
            var target = upgrade.ToCabinId.ToString(CultureInfo.InvariantCulture);
            var groups = new List<OfferOptionGroup> { Known("targetCabinId", new OfferOptionChoice(target, target)) };

            if (upgrade.AllowedUpgradeKind != UpgradeKind.FixedAncillary)
                groups.Add(new OfferOptionGroup("quoteRef", OptionSourceState.Unknown, []));

            return groups;
        }

        private static IReadOnlyList<OfferOptionGroup> Pet(PetSpecification pet)
        {
            var groups = new List<OfferOptionGroup>
            {
                new("animalType", OptionSourceState.Known, pet.AllowedAnimalTypes
                    .Select(animal => animal.AnimalType)
                    .Distinct()
                    .Order()
                    .Select(animal => new OfferOptionChoice(animal.ToString(), animal.ToString()))
                    .ToList())
            };
            var otherCodes = pet.AllowedAnimalTypes.Where(animal => animal.AnimalType == PetAnimalType.RegisteredOther).Select(animal => animal.OtherCode).ToList();

            if (otherCodes.Count > 0)
                groups.Add(Codes("otherAnimalCode", otherCodes)[0]);

            if (pet.AllowedHoldAnimalSizeBrackets.Count > 0)
                groups.Add(Codes("sizeBracket", pet.AllowedHoldAnimalSizeBrackets.Select(bracket => bracket.Code))[0]);

            return groups;
        }

        private static IReadOnlyList<OfferOptionGroup> Airport(AirportServiceSpecification airport, AncillaryProvision provision)
        {
            var airportId = airport.AirportId.ToString(CultureInfo.InvariantCulture);
            var groups = new List<OfferOptionGroup> { Known("airportId", new OfferOptionChoice(airportId, airportId)) };

            if ((provision.AirportServiceRule?.FacilityId ?? airport.FacilityId) is { } facility)
            {
                var facilityRef = facility.ToString(CultureInfo.InvariantCulture);

                groups.Add(Known("facilityRef", new OfferOptionChoice(facilityRef, facilityRef)));
            }

            return groups;
        }

        private static IReadOnlyList<OfferOptionGroup> Codes(string fieldName, IEnumerable<string> codes)
            => [new OfferOptionGroup(fieldName, OptionSourceState.Known, codes.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(code => new OfferOptionChoice(code, code)).ToList())];

        private static OfferOptionGroup Travellers(string fieldName, IEnumerable<ShoppingTraveller> travellers)
            => new(fieldName, OptionSourceState.Known, travellers.Select(traveller => new OfferOptionChoice(traveller.TravellerRef, traveller.TravellerRef)).ToList());

        private static OfferOptionGroup Known(string fieldName, OfferOptionChoice choice) => new(fieldName, OptionSourceState.Known, [choice]);

        private static string Kilograms(decimal? weight) => $"{weight?.ToString("0.##", CultureInfo.InvariantCulture)} kg";
    }
}
