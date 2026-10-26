using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Options
{
    public static class TypedSelectionForm
    {
        private const string Kilograms = "kg";
        private const string Centimetres = "cm";

        public static IReadOnlyList<SelectionFieldDescriptor> Fields(
            AncillaryServiceDefinition definition,
            AncillaryProvision provision,
            IReadOnlyList<ShoppingTraveller> travellers,
            string travellerRef)
            => definition.VariantCode switch
            {
                AncillaryVariant.Overweight =>
                [
                    new SelectionFieldDescriptor("bagRef", SelectionControls.Text, true),
                    new SelectionFieldDescriptor(
                        "measuredWeightKg",
                        SelectionControls.Number,
                        true,
                        Unit: Kilograms,
                        ExclusiveMin: definition.Baggage!.WeightFromExclusiveKg ?? 0m,
                        Max: definition.Baggage.WeightToInclusiveKg)
                ],
                AncillaryVariant.Oversize => [Dimensions("dimensions")],
                AncillaryVariant.SpecialEquipment => SpecialEquipment(definition.Baggage!),
                AncillaryVariant.PetInCabin or AncillaryVariant.PetInHold => Pet(definition.Pet!, provision),
                AncillaryVariant.Wheelchair => [Select("assistanceSsrCode", definition.AssistedTravel!.Wheelchair!.AllowedSsrCodes.Select(code => code.Code))],
                AncillaryVariant.DisabilityAssistance => [Select("assistanceSsrCode", definition.AssistedTravel!.DisabilityAssistance!.AllowedSsrCodes.Select(code => code.Code))],
                AncillaryVariant.Bassinet =>
                [
                    Select("infantRef", travellers
                        .Where(traveller => traveller.PassengerTypeCode == PassengerTypeCode.INF && traveller.AssociatedAdultRef == travellerRef)
                        .Select(traveller => traveller.TravellerRef)),
                    Select("guardianRef", travellers.Where(traveller => traveller.TravellerRef == travellerRef).Select(traveller => traveller.TravellerRef))
                ],
                AncillaryVariant.Lounge or AncillaryVariant.FastTrack or AncillaryVariant.CipMeetAssist => AirportService(definition.AirportService!, provision),
                AncillaryVariant.PriorityBoardingCheckin => [new SelectionFieldDescriptor("optIn", SelectionControls.Checkbox, true)],
                AncillaryVariant.OnboardWifi when definition.Connectivity!.MaxDevices is { } devices =>
                [
                    new SelectionFieldDescriptor("deviceCount", SelectionControls.Number, false, Min: 1, Max: devices)
                ],
                _ => []
            };

        public static AncillarySelection WithFixedChoices(AncillarySelection selection, AncillaryServiceDefinition definition, AncillaryProvision provision)
            => definition.VariantCode switch
            {
                AncillaryVariant.SpecialEquipment => selection with { EquipmentKind = definition.Baggage!.EquipmentKind },
                AncillaryVariant.Lounge or AncillaryVariant.FastTrack or AncillaryVariant.CipMeetAssist => selection with
                {
                    AirportId = definition.AirportService!.AirportId,
                    FacilityRef = provision.AirportServiceRule?.FacilityId ?? definition.AirportService.FacilityId
                },
                AncillaryVariant.OnboardWifi => selection with { PlanCode = definition.ServiceDefinitionRef },
                _ => selection
            };

        private static IReadOnlyList<SelectionFieldDescriptor> SpecialEquipment(BaggageSpecification baggage)
        {
            var fields = new List<SelectionFieldDescriptor>();

            if (baggage.MaxSize is not null || baggage.MaxLinearSumCm is not null)
                fields.Add(Dimensions("dimensions"));

            if (baggage.MaxKgPerPiece is { } maximum)
                fields.Add(new SelectionFieldDescriptor("weightKg", SelectionControls.Number, true, Unit: Kilograms, ExclusiveMin: 0m, Max: maximum));

            return fields;
        }

        private static IReadOnlyList<SelectionFieldDescriptor> Pet(PetSpecification pet, AncillaryProvision provision)
        {
            var rule = provision.PetRule;
            var fields = new List<SelectionFieldDescriptor> { Select("animalType", pet.AllowedAnimalTypes.Select(animal => animal.AnimalType.ToString())) };
            var otherCodes = pet.AllowedAnimalTypes.Where(animal => animal.AnimalType == PetAnimalType.RegisteredOther).Select(animal => animal.OtherCode).ToList();

            if (otherCodes.Count > 0)
                fields.Add(Select("otherAnimalCode", otherCodes, false));

            if ((rule?.MinAnimalAgeWeeksOverride ?? pet.MinAnimalAgeWeeks) is > 0 and var minimumAge)
                fields.Add(new SelectionFieldDescriptor("animalAgeWeeks", SelectionControls.Number, false, Min: minimumAge));

            fields.Add(new SelectionFieldDescriptor(
                "combinedWeightKg",
                SelectionControls.Number,
                true,
                Unit: Kilograms,
                ExclusiveMin: 0m,
                Max: rule?.MaxCombinedKgOverride ?? pet.MaxCombinedWeightKg));
            fields.Add(Dimensions("carrierDimensions"));

            if (pet.RequiredDocumentCodes.Count > 0)
                fields.Add(new SelectionFieldDescriptor("documentAcknowledgements", SelectionControls.MultiSelect, true, Choices(pet.RequiredDocumentCodes.Select(code => code.Code))));

            if (pet.AllowedHoldAnimalSizeBrackets.Count > 0)
                fields.Add(Select("sizeBracket", pet.AllowedHoldAnimalSizeBrackets.Select(bracket => bracket.Code)));

            return fields;
        }

        private static IReadOnlyList<SelectionFieldDescriptor> AirportService(AirportServiceSpecification airport, AncillaryProvision provision)
        {
            var fields = new List<SelectionFieldDescriptor>();

            if (airport.RequiresSpecificAppointment)
                fields.Add(new SelectionFieldDescriptor("timeWithOffset", SelectionControls.DateTime, true));

            if (airport.MaxGuestsPerPrimary is > 0)
                fields.Add(new SelectionFieldDescriptor(
                    "guestCount",
                    SelectionControls.Number,
                    false,
                    Min: 0,
                    Max: provision.AirportServiceRule?.MaxGuestsPerPrimary ?? airport.MaxGuestsPerPrimary));

            return fields;
        }

        private static SelectionFieldDescriptor Dimensions(string name)
            => new(name, SelectionControls.Dimensions, true, Unit: Centimetres, Fields: [Centimetre("lengthCm"), Centimetre("widthCm"), Centimetre("heightCm")]);

        private static SelectionFieldDescriptor Centimetre(string name) => new(name, SelectionControls.Number, true, Unit: Centimetres, ExclusiveMin: 0m);

        private static SelectionFieldDescriptor Select(string name, IEnumerable<string> values, bool required = true)
            => new(name, SelectionControls.Select, required, Choices(values));

        private static IReadOnlyList<SelectionFieldChoice> Choices(IEnumerable<string> values)
            => values.Distinct(StringComparer.Ordinal).Select(value => new SelectionFieldChoice(value, value)).ToList();
    }
}
