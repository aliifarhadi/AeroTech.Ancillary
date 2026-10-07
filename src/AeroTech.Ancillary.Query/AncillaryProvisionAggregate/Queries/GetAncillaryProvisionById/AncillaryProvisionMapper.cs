using System.Globalization;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public static class AncillaryProvisionMapper
    {
        public static BackofficeProvisionDto ToBackofficeProvision(
            AncillaryProvisionReadModel provision,
            IReadOnlyList<AncillaryProvisionPriceLineReadModel> priceLines)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                EnumValueDto.Of(provision.Status),
                provision.SalesEffectiveFrom,
                provision.SalesDiscontinueAt,
                EnumValueDto.Of(provision.CoverageScope),
                EnumValueDto.Of(provision.QuantityUnit),
                provision.MinQuantity,
                provision.MaxQuantity,
                EnumValueDto.Of(provision.ApplicationType),
                EnumValueDto.Of(provision.Disposition),
                provision.DocumentRequired,
                provision.BookingRequired,
                provision.FeeCurrencyId,
                provision.FeeApplicationUnit is { } feeApplicationUnit ? EnumValueDto.Of(feeApplicationUnit) : null,
                EnumValueDto.Of(provision.ReissueRefund),
                provision.FormOfRefund is { } formOfRefund ? EnumValueDto.Of(formOfRefund) : null,
                provision.Commissionable,
                provision.InterlineSettlement,
                provision.MustCheckAvailability,
                provision.FulfillmentProviderKey,
                provision.CreatedAt,
                priceLines
                    .OrderBy(line => line.Id)
                    .Select(line => new BackofficeProvisionPriceLineDto(
                        line.Id,
                        EnumValueDto.Of(line.Category),
                        line.Code,
                        line.Name,
                        line.UnitAmount))
                    .ToList());

        public static ProvisionPaginatedRowDto ToPaginatedRow(
            AncillaryProvisionReadModel provision,
            IReadOnlyList<AncillaryProvisionPriceLineReadModel> priceLines,
            string? currency)
            => new()
            {
                Id = provision.Id.ToString(CultureInfo.InvariantCulture),
                ServiceDefinitionId = provision.ServiceDefinitionId.ToString(CultureInfo.InvariantCulture),
                Sequence = provision.Sequence,
                CoverageScope = EnumValueDto.Of(provision.CoverageScope),
                Disposition = EnumValueDto.Of(provision.Disposition),
                QuantityUnit = EnumValueDto.Of(provision.QuantityUnit),
                MinQuantity = provision.MinQuantity,
                MaxQuantity = provision.MaxQuantity,
                FiledAmount = priceLines.Count == 0
                    ? null
                    : priceLines.Sum(line => line.UnitAmount).ToString("n2", CultureInfo.InvariantCulture),
                Currency = currency,
                SalesEffectiveFrom = provision.SalesEffectiveFrom,
                SalesDiscontinueAt = provision.SalesDiscontinueAt,
                Status = EnumValueDto.Of(provision.Status),
                CreatedAt = provision.CreatedAt
            };
    }
}
