using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm
{
    public static class AncillaryProvisionFormMapper
    {
        public static BackofficeProvisionFormDto ToBackofficeForm(BackofficeProvisionDto provision)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Status,
                provision.Profile,
                provision.VariantCode,
                provision.ApplicableRuleSections,
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                new ProvisionFormDto(
                    provision.Sequence,
                    Of<ServiceCoverageScope>(provision.CoverageScope),
                    Of<PurchaseStage>(provision.PurchaseStage),
                    Of<PriceOrigin>(provision.PriceOrigin),
                    provision.QuoteProviderKey,
                    new ProvisionQuantityInput(Of<AncillaryQuantityUnit>(provision.QuantityUnit), provision.MinQuantity, provision.MaxQuantity),
                    Of<ProvisionApplicationType>(provision.ApplicationType),
                    new ProvisionOutcomeInput(Of<CommercialDisposition>(provision.Disposition), provision.DocumentRequired, provision.BookingRequired),
                    new ProvisionSettlementInput(
                        Of<ReissueRefundPolicy>(provision.ReissueRefund),
                        OfNullable<FormOfRefund>(provision.FormOfRefund),
                        provision.Commissionable,
                        provision.InterlineSettlement,
                        provision.CancellationRules.Count == 0 ? null : provision.CancellationRules.Select(ToForm).ToList()),
                    new ProvisionAvailabilityInput(provision.MustCheckAvailability),
                    new ProvisionFulfillmentInput(provision.FulfillmentProviderKey),
                    provision.PassengerEligibility is null ? null : ToForm(provision.PassengerEligibility),
                    provision.SalesRestrictions is null ? null : ToForm(provision.SalesRestrictions),
                    provision.Geography is null ? null : ToForm(provision.Geography),
                    provision.FlightApplication is null ? null : ToForm(provision.FlightApplication),
                    provision.FareApplication is null ? null : ToForm(provision.FareApplication),
                    provision.TravelDate is null ? null : ToForm(provision.TravelDate),
                    provision.DayTimeApplication is null ? null : ToForm(provision.DayTimeApplication),
                    provision.AdvancePurchase is null ? null : ToForm(provision.AdvancePurchase),
                    provision.BaggageApplication is null ? null : ToForm(provision.BaggageApplication),
                    provision.SeatApplication is null ? null : ToForm(provision.SeatApplication),
                    provision.PetRule is null ? null : ToForm(provision.PetRule),
                    provision.AssistedTravelRule is null ? null : ToForm(provision.AssistedTravelRule),
                    provision.AirportServiceRule is null ? null : ToForm(provision.AirportServiceRule)));

        private static ProvisionCancellationRuleInput ToForm(BackofficeProvisionCancellationRuleDto rule)
            => new(
                rule.RuleRef,
                Of<CancellationReasonKind>(rule.ReasonKind),
                rule.BeforeStartMinutesFrom,
                rule.BeforeStartMinutesTo,
                Of<PenaltyKind>(rule.PenaltyKind),
                rule.PenaltyFixedAmount,
                rule.PenaltyPercentage,
                rule.PenaltyCurrencyId,
                OfNullable<CancellationPercentageBase>(rule.PercentageBase),
                rule.WaiverPermitted,
                rule.SourceReference);

        private static ProvisionPassengerEligibilityInput ToForm(BackofficeProvisionPassengerEligibilityDto rule)
            => new(
                rule.AllowedPassengerTypes.Select(row => Of<PassengerTypeCode>(row.Value)).ToList(),
                rule.AllowedAgeBands.Select(band => new ProvisionAgeBandInput(band.AgeFromInclusive, band.AgeToExclusive)).ToList());

        private static ProvisionSalesRestrictionsInput ToForm(BackofficeProvisionSalesRestrictionsDto rule)
            => new(
                rule.SalesEffectiveFrom,
                rule.SalesDiscontinueAt,
                Values(rule.AllowedPointsOfSale),
                Values(rule.AllowedCustomers),
                rule.AllowedCustomerTypes.Select(row => Of<CustomerType>(row.Value)).ToList());

        private static ProvisionGeographyInput ToForm(BackofficeProvisionGeographyDto rule)
            => new(
                Values(rule.AllowedOriginAirports),
                Values(rule.AllowedDestinationAirports),
                Values(rule.AllowedViaAirports),
                rule.AllowedRoutePairs
                    .Select(pair => new ProvisionRoutePairInput(pair.OriginAirportId, pair.DestinationAirportId, Of<RoutePairDirection>(pair.Direction)))
                    .ToList(),
                rule.ServiceLocations
                    .Select(location => new ProvisionServiceLocationInput(Of<ServiceLocationType>(location.LocationType), location.LocationId))
                    .ToList(),
                Values(rule.CoverageCountries));

        private static ProvisionFlightApplicationInput ToForm(BackofficeProvisionFlightApplicationDto rule)
            => new(
                Values(rule.AllowedMarketingAirlines),
                Values(rule.AllowedOperatingAirlines),
                Values(rule.AllowedFlightNumbers),
                Values(rule.AllowedFlights),
                Values(rule.AllowedAircraft));

        private static ProvisionFareApplicationInput ToForm(BackofficeProvisionFareApplicationDto rule)
            => new(
                Values(rule.AllowedAirFares),
                rule.AllowedAirFareTypes.Select(row => Of<AirFareType>(row.Value)).ToList(),
                Values(rule.AllowedFareFamilies),
                Values(rule.AllowedFareBases),
                Values(rule.AllowedCabinClasses),
                Values(rule.AllowedRbds));

        private static ProvisionTravelDateInput ToForm(BackofficeProvisionTravelDateDto rule)
            => new(
                rule.PermittedPeriods.Select(period => new ProvisionDatePeriodInput(period.StartDate, period.EndDate)).ToList(),
                rule.BlackoutPeriods.Select(period => new ProvisionDatePeriodInput(period.StartDate, period.EndDate)).ToList());

        private static ProvisionDayTimeApplicationInput ToForm(BackofficeProvisionDayTimeApplicationDto rule)
            => new(
                rule.Windows
                    .Select(window => new ProvisionDayTimeWindowInput(
                        window.DaysOfWeekMask,
                        window.StartLocalTime,
                        window.EndLocalTime,
                        Of<DayTimeRestrictionEffect>(window.Effect)))
                    .ToList());

        private static ProvisionAdvancePurchaseInput ToForm(BackofficeProvisionAdvancePurchaseDto rule)
            => new(rule.MinimumPeriod, Of<TimeUnit>(rule.Unit), rule.SameTimeAsTicketed, rule.MaximumPeriod);

        private static ProvisionBaggageApplicationInput ToForm(BackofficeProvisionBaggageApplicationDto rule)
            => new(
                rule.FreePieces,
                rule.FirstExcessPiece,
                rule.LastExcessPiece,
                rule.Weight,
                Of<WeightUnit>(rule.WeightUnit),
                OfNullable<BaggageTravelApplication>(rule.TravelApplication),
                Of<BaggagePurchaseApplication>(rule.PurchaseApplication),
                OfNullable<BaggageRuleDeference>(rule.RuleDeference),
                OfNullable<BaggageChargeKind>(rule.ChargeKind),
                OfNullable<BaggageAllowanceConcept>(rule.AllowanceConcept));

        private static ProvisionSeatApplicationInput ToForm(BackofficeProvisionSeatApplicationDto rule)
            => new(Values(rule.SeatNumbers), Values(rule.SeatCharacteristics));

        private static ProvisionPetRuleInput ToForm(BackofficeProvisionPetRuleDto rule)
            => new(
                rule.CountryExceptionCode,
                rule.MinAnimalAgeWeeksOverride,
                rule.MaxCombinedKgOverride,
                Of<ConfirmationRequirement>(rule.AcceptanceMode));

        private static ProvisionAssistedTravelRuleInput ToForm(BackofficeProvisionAssistedTravelRuleDto rule)
            => new(rule.MinimumLeadTimeMinutes, OfNullable<MinorConnectionPolicy>(rule.ConnectionPolicy), rule.MedicalApprovalRequired);

        private static ProvisionAirportServiceRuleInput ToForm(BackofficeProvisionAirportServiceRuleDto rule)
            => new(
                rule.TerminalRef,
                OfNullable<AirportServiceDirection>(rule.Direction),
                rule.ServiceWindowStart,
                rule.ServiceWindowEnd,
                rule.FacilityId,
                rule.MaxGuestsPerPrimary);

        private static List<TValue> Values<TValue>(IReadOnlyList<BackofficeProvisionRuleRowDto<TValue>> rows)
            => rows.Select(row => row.Value).ToList();

        private static TEnum Of<TEnum>(EnumValueDto value)
            where TEnum : struct, Enum
            => (TEnum)Enum.ToObject(typeof(TEnum), value.Value);

        private static TEnum? OfNullable<TEnum>(EnumValueDto? value)
            where TEnum : struct, Enum
            => value is null ? null : Of<TEnum>(value);
    }
}
