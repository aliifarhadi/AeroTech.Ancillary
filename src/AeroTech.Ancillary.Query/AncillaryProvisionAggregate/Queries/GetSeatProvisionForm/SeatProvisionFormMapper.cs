using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm
{
    public static class SeatProvisionFormMapper
    {
        private const int PublishedPointOfSaleCount = 1;

        public static SeatProvisionConstraintsDto Constraints(string variantCode)
            => new(
                EnumValueDto.Of(AncillaryQuantityUnit.Each),
                variantCode is AncillaryVariant.StandardSeat or AncillaryVariant.PreferredSeat ? 1 : null,
                true,
                true,
                PublishedPointOfSaleCount,
                variantCode == AncillaryVariant.PreferredSeat);

        public static BackofficeSeatProvisionFormDto ToSeatForm(BackofficeProvisionFormDto provision, EnumValueDto profile, string variantCode)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Status,
                profile,
                variantCode,
                provision.Status.Value == (int)ProvisionStatus.Draft,
                provision.ApplicableRuleSections,
                Constraints(variantCode),
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                new SeatProvisionFormDto(
                    provision.Form.Sequence,
                    provision.Form.CoverageScope,
                    provision.Form.PurchaseStage,
                    provision.Form.PriceOrigin,
                    provision.Form.QuoteProviderKey,
                    provision.Form.Quantity,
                    provision.Form.Outcome,
                    provision.Form.Settlement,
                    provision.Form.Availability,
                    provision.Form.Fulfillment,
                    provision.Form.PassengerEligibility,
                    provision.Form.SalesRestrictions,
                    provision.Form.Geography,
                    provision.Form.FlightApplication,
                    provision.Form.FareApplication,
                    provision.Form.TravelDate,
                    provision.Form.DayTimeApplication,
                    provision.Form.AdvancePurchase,
                    provision.Form.SeatApplication));
    }
}
