using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Shopping.Options;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions
{
    public static class SelectionOptionsMapper
    {
        public static SelectionOptionsDto Map(SelectionOptionsForm form)
            => new(
                form.VariantCode,
                form.Fields.ToDictionary(
                    field => field.Name,
                    field => new SelectionFieldDto(
                        field.Control,
                        field.Required,
                        field.Choices?.Select(choice => new SelectionFieldChoiceDto(choice.Value, choice.Label)).ToList(),
                        field.Unit,
                        field.Min,
                        field.ExclusiveMin,
                        field.Max),
                    StringComparer.Ordinal));
    }
}
