using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed record ChargedUnits(int? Count, string? ReasonCode)
    {
        private const string QuantityField = "Quantity";
        private const int SingleOccurrence = 1;

        public static ChargedUnits Of(CustomerSelectionContract contract, AncillaryProvision provision, AncillarySelection? selection)
        {
            if (selection is null)
                return new ChargedUnits(null, null);

            var limits = provision.Quantity;

            if (contract.Fields.Any(field => field is { Name: QuantityField, Required: true }))
                return new ChargedUnits(
                    selection.Quantity is > 0 and var quantity && quantity >= limits.MinQuantity && quantity <= limits.MaxQuantity ? quantity : null,
                    null);

            if (limits.MinQuantity > SingleOccurrence)
                return new ChargedUnits(null, ShoppingReasonCodes.QuantityBelowMinimum);

            return limits.MaxQuantity < SingleOccurrence
                ? new ChargedUnits(null, ShoppingReasonCodes.QuantityAboveMaximum)
                : new ChargedUnits(SingleOccurrence, null);
        }
    }
}
