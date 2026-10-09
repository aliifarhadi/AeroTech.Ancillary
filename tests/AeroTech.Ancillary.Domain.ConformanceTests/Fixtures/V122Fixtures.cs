using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class V122Fixtures
{
    public const long PointOfSale = 9001;

    public static ProvisionSalesRestrictionsArgs SinglePointOfSale => new(null, null, [PointOfSale], [], []);

    public static PriceOrigin Origin(CommercialDisposition disposition)
        => disposition switch
        {
            CommercialDisposition.Paid => PriceOrigin.Filed,
            CommercialDisposition.Free => PriceOrigin.Free,
            _ => PriceOrigin.NotAvailable
        };

    public static string VariantFor(PricingUnit pricingUnit, ServiceDateBasis serviceDateBasis)
        => (pricingUnit, serviceDateBasis) switch
        {
            (PricingUnit.PerPiece, _) => AncillaryVariant.ExtraCheckedBag,
            (PricingUnit.PerKilogram, _) => AncillaryVariant.Overweight,
            (PricingUnit.PerSeat, _) => AncillaryVariant.StandardSeat,
            (PricingUnit.PerItem, _) => AncillaryVariant.OnboardWifi,
            (_, ServiceDateBasis.ServiceStart) => AncillaryVariant.Lounge,
            _ => AncillaryVariant.PriorityBoardingCheckin
        };

    public static ServiceDefinitionProfileArgs Profile(
        PricingUnit pricingUnit,
        ServiceDateBasis serviceDateBasis,
        AncillaryDocumentType documentType = AncillaryDocumentType.EmdAssociated)
        => Profile(VariantFor(pricingUnit, serviceDateBasis), documentType);

    public static ServiceDefinitionProfileArgs Profile(string variantCode, AncillaryDocumentType documentType = AncillaryDocumentType.EmdAssociated)
        => new(
            AncillaryVariant.Find(variantCode)!.Profile,
            variantCode,
            documentType == AncillaryDocumentType.None ? DocumentRouting.NoAncillaryDocument : DocumentRouting.Emd,
            Specification(variantCode));

    public static ServiceSpecificationArgs Specification(string variantCode)
        => variantCode switch
        {
            AncillaryVariant.ExtraCheckedBag => new(Baggage: new BaggageSpecificationArgs(
                BaggageChargeKind.ExtraPiece, BaggageAllowanceConcept.Piece, null, 23m, null, null, null, null, null, null)),
            AncillaryVariant.ExtraWeightPackage => new(Baggage: new BaggageSpecificationArgs(
                BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 10m, null, null, null, null, null, null, null)),
            AncillaryVariant.Oversize => new(Baggage: new BaggageSpecificationArgs(
                BaggageChargeKind.Oversize, null, null, 32m, null, null, new DimensionsCmArgs(120m, 80m, 60m), 203m, null, BaggageChargeCombination.MutuallyExclusive)),
            AncillaryVariant.CabinBag => new(Baggage: new BaggageSpecificationArgs(
                BaggageChargeKind.ExtraPiece, BaggageAllowanceConcept.Piece, null, 8m, null, null, new DimensionsCmArgs(55m, 40m, 23m), null, null, null)),
            AncillaryVariant.SpecialEquipment => new(Baggage: new BaggageSpecificationArgs(
                BaggageChargeKind.SpecialEquipment, null, null, 32m, null, null, new DimensionsCmArgs(200m, 80m, 40m), null, "BIKE", null)),
            AncillaryVariant.Overweight => new(Baggage: new BaggageSpecificationArgs(
                BaggageChargeKind.Overweight, null, null, 32m, 23m, 32m, null, null, null, BaggageChargeCombination.Separate)),
            AncillaryVariant.StandardSeat => new(Seat: new SeatSpecificationArgs(SeatPurpose.Standard, [], [], false, false, null, null, false)),
            AncillaryVariant.FreeSpecialMeal => new(Meal: new MealSpecificationArgs(MealKind.SpecialRequest, "VGML", null, "VEGETARIAN", 1440, "MAIN_MEAL")),
            AncillaryVariant.Lounge => new(AirportService: new AirportServiceSpecificationArgs(
                AirportServiceKind.Lounge, 1, "T1", null, AirportServiceDirection.Departure, new TimeOnly(6, 0), new TimeOnly(22, 0), "Asia/Tehran", 180, 1, [], false)),
            AncillaryVariant.OnboardWifi => new(Connectivity: new ConnectivitySpecificationArgs(
                ConnectivityPlanKind.FullFlight, null, null, 2, [], PurchaseStage.PreOrder, null)),
            _ => new(Priority: new PrioritySpecificationArgs(PriorityKind.Boarding, "ZONE1", null, null, []))
        };
}
