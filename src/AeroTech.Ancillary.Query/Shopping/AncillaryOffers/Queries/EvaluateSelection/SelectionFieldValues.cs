using System.Globalization;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection
{
    public static class SelectionFieldValues
    {
        private static readonly IReadOnlyDictionary<string, Func<AncillarySelection, string?>> Readers =
            new Dictionary<string, Func<AncillarySelection, string?>>(StringComparer.Ordinal)
            {
                ["packageProductRef"] = selection => selection.PackageProductRef,
                ["equipmentKind"] = selection => selection.EquipmentKind,
                ["purpose"] = selection => selection.Purpose?.ToString(),
                ["targetCabinId"] = selection => selection.TargetCabinId?.ToString(CultureInfo.InvariantCulture),
                ["quoteRef"] = selection => selection.QuoteRef,
                ["mealCode"] = selection => selection.MealCode,
                ["menuItemRef"] = selection => selection.MenuItemRef,
                ["animalType"] = selection => selection.AnimalType?.ToString(),
                ["otherAnimalCode"] = selection => selection.OtherAnimalCode,
                ["sizeBracket"] = selection => selection.SizeBracket,
                ["assistanceSsrCode"] = selection => selection.AssistanceSsrCode,
                ["equipmentCode"] = selection => selection.EquipmentCode,
                ["infantRef"] = selection => selection.InfantRef,
                ["guardianRef"] = selection => selection.GuardianRef,
                ["childRef"] = selection => selection.ChildRef,
                ["airportId"] = selection => selection.AirportId?.ToString(CultureInfo.InvariantCulture),
                ["facilityRef"] = selection => selection.FacilityRef?.ToString(CultureInfo.InvariantCulture),
                ["planCode"] = selection => selection.PlanCode
            };

        public static string? Of(AncillarySelection selection, string fieldName)
            => Readers.TryGetValue(fieldName, out var read) && read(selection) is { Length: > 0 } value ? value : null;
    }
}
