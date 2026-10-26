namespace AeroTech.Ancillary.Shopping.Selections
{
    public static class SelectionBasketRules
    {
        public static string? Conflict(IReadOnlyList<SelectionBasketLine> earlier, SelectionBasketLine line)
        {
            if (earlier.Any(other => SameUnit(other, line)))
                return "the same unit is selected twice";

            if (line.Definition.Meal?.ExclusiveMealFamilyCode is { } family
                && earlier.Any(other => other.Definition.Meal?.ExclusiveMealFamilyCode == family && other.TravellerRef == line.TravellerRef && other.CoverageRef == line.CoverageRef))
                return $"another meal of family {family} is selected for the same traveller and unit";

            return null;
        }

        private static bool SameUnit(SelectionBasketLine left, SelectionBasketLine right)
            => left.PricingRateId == right.PricingRateId
               && left.CoverageScope == right.CoverageScope
               && left.TravellerRef == right.TravellerRef
               && left.CoverageRef == right.CoverageRef;
    }
}
