using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById
{
    public static class InventoryPolicyMapper
    {
        public static BackofficeInventoryPolicyDto ToBackofficePolicy(
            InventoryPolicyReadModel policy,
            IEnumerable<InventoryPassengerUsageLimitReadModel> limits,
            long? currentServiceDefinitionId)
            => new(
                policy.Id,
                policy.OwnerAirlineId,
                policy.ServiceDefinitionRef,
                policy.ServiceDefinitionId,
                currentServiceDefinitionId,
                EnumValueDto.Of(policy.Authority),
                policy.LocalPattern is { } pattern ? EnumValueDto.Of(pattern) : null,
                policy.ProviderKey,
                policy.CountResourceId is { } countResourceId
                    ? new BackofficeFlightCountConsumptionDto(countResourceId, policy.CountPerAcceptedUnit!.Value, EnumValueDto.Of(policy.CountUnit!.Value))
                    : null,
                policy.WeightResourceId is { } weightResourceId
                    ? new BackofficeFlightWeightConsumptionDto(weightResourceId, EnumValueDto.Of(policy.WeightConsumptionMode!.Value), policy.WeightFixedKgPerUnit)
                    : null,
                policy.SlotFacilityId is { } facilityId
                    ? new BackofficeAirportSlotConsumptionDto(facilityId, policy.SlotOccupancyMinutes!.Value, policy.SlotPeoplePerAcceptedUnit!.Value)
                    : null,
                limits
                    .OrderBy(limit => limit.Id)
                    .Select(limit => new BackofficePassengerUsageLimitDto(
                        limit.Id,
                        EnumValueDto.Of(limit.LimitScope),
                        limit.MaxUnits,
                        limit.CountingFamilyCode,
                        EnumValueDto.Of(limit.ConsumptionUnit),
                        limit.UnitsPerPurchase))
                    .ToList(),
                EnumValueDto.Of(policy.Status),
                policy.Version,
                policy.CreatedAt,
                policy.UpdatedAt,
                policy.ActivatedAt,
                policy.SuspendedAt,
                policy.RetiredAt);

        public static InventoryPolicyPaginatedRowDto ToPaginatedRow(InventoryPolicyReadModel policy)
            => new()
            {
                Id = policy.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = policy.OwnerAirlineId,
                ServiceDefinitionRef = policy.ServiceDefinitionRef,
                Authority = EnumValueDto.Of(policy.Authority),
                LocalPattern = policy.LocalPattern is { } pattern ? EnumValueDto.Of(pattern) : null,
                ProviderKey = policy.ProviderKey,
                Status = EnumValueDto.Of(policy.Status),
                Version = policy.Version,
                UpdatedAt = policy.UpdatedAt
            };
    }
}
