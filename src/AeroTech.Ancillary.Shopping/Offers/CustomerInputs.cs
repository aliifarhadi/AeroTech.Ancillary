using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Offers
{
    public static class CustomerInputs
    {
        public static bool FixedChoicesAreDefined(AncillaryServiceDefinition definition)
            => definition.VariantCode switch
            {
                AncillaryVariant.SpecialEquipment => definition.Baggage!.EquipmentKind is not null,
                AncillaryVariant.CabinUpgrade => definition.Upgrade!.AllowedUpgradeKind == UpgradeKind.FixedAncillary,
                AncillaryVariant.FreeSpecialMeal => definition.Meal!.MealCode is not null,
                AncillaryVariant.PaidPreorderMeal => definition.Meal!.MenuItemRef is not null,
                _ => true
            };

        public static bool AreRequested(AncillaryServiceDefinition definition, AncillaryProvision provision)
            => TypedSelectionForm.Fields(definition, provision, [], string.Empty).Count > 0;
    }
}
