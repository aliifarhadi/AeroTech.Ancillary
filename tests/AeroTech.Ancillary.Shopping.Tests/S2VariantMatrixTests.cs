using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2VariantMatrixTests
{
    private const decimal Amount = 100m;

    public sealed record MatrixRow(
        string TestId,
        string Code,
        SelectionKind Kind,
        Func<AncillarySelection> Positive,
        SelectionEvaluationStatus PositiveStatus,
        decimal? LineTotal,
        string Outcome);

    private static ShoppingTraveller Infant()
        => new() { TravellerRef = "T2", PassengerTypeCode = PassengerTypeCode.INF, DateOfBirth = new DateOnly(2026, 6, 1), AssociatedAdultRef = "T1", StableTravellerIdentity = "PAX-T2" };

    public static readonly IReadOnlyList<MatrixRow> Matrix =
    [
        new("S2M-A01", "A01", SelectionKind.QuantityChoice, () => new() { Quantity = 1 }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable"),
        new("S2M-A02", "A02", SelectionKind.QuantityChoice, () => new() { PackageProductRef = "WEIGHT_PACK_10", Quantity = 1 }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable"),
        new("S2M-A03", "A03", SelectionKind.TypedForm, () => new() { BagRef = "BAG-1", MeasuredWeightKg = 28m }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable per piece; PerKilogram stays INTERNAL_GAP"),
        new("S2M-A04", "A04", SelectionKind.TypedForm, () => new() { Dimensions = new(110m, 50m, 30m) }, SelectionEvaluationStatus.Incomplete, null, "FailClosed: charge combination not verified"),
        new("S2M-A05", "A05", SelectionKind.QuantityChoice, () => new() { Quantity = 1 }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable"),
        new("S2M-A06", "A06", SelectionKind.TypedForm, () => new() { EquipmentKind = "BIKE", Dimensions = new(190m, 70m, 30m), WeightKg = 20m }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable, check at reserve"),
        new("S2M-A07", "A07", SelectionKind.SeatMapSelection, () => new() { SeatNumber = "14C" }, SelectionEvaluationStatus.Accepted, Amount, "SeatMaps + Resolve only"),
        new("S2M-A08", "A08", SelectionKind.SeatMapSelection, () => new() { SeatNumber = "12A", ExitRowTermsAccepted = true }, SelectionEvaluationStatus.Incomplete, null, "SeatMaps + Resolve only; characteristic not verified without a map"),
        new("S2M-A09", "A09", SelectionKind.SeatMapSelection, () => new() { SeatNumber = "12A", Purpose = ExtraSeatPurpose.PassengerComfort }, SelectionEvaluationStatus.Incomplete, null, "FailClosed: adjacency not verified"),
        new("S2M-A10", "A10", SelectionKind.ExternalQuote, () => new() { TargetCabinId = 2, QuoteRef = "Q-1" }, SelectionEvaluationStatus.Incomplete, null, "NeedsQuote"),
        new("S2M-A11", "A11", SelectionKind.TypedForm, () => new() { MealCode = "VGML" }, SelectionEvaluationStatus.Accepted, 0m, "Free, zero total"),
        new("S2M-A12", "A12", SelectionKind.QuantityChoice, () => new() { MenuItemRef = "MENU_PASTA_01", Quantity = 1 }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable"),
        new("S2M-A13", "A13", SelectionKind.TypedForm, () => new() { AnimalType = PetAnimalType.Cat, CombinedWeightKg = 5m, CarrierDimensions = new(50m, 30m, 20m), DocumentAcknowledgements = ["ENTRY_RULES_ACK"], AnimalAgeWeeks = 20 }, SelectionEvaluationStatus.Incomplete, null, "FailClosed: animal age not verified"),
        new("S2M-A14", "A14", SelectionKind.TypedForm, () => new() { AnimalType = PetAnimalType.Dog, CombinedWeightKg = 20m, CarrierDimensions = new(100m, 60m, 70m), DocumentAcknowledgements = ["HEALTH_CERT"], SizeBracket = "MEDIUM", AnimalAgeWeeks = 20 }, SelectionEvaluationStatus.Incomplete, null, "FailClosed: animal age not verified"),
        new("S2M-A15", "A15", SelectionKind.TypedForm, () => new() { AssistanceSsrCode = "WCHR" }, SelectionEvaluationStatus.Accepted, 0m, "Free, zero total, check at reserve"),
        new("S2M-A16", "A16", SelectionKind.TypedForm, () => new() { AssistanceSsrCode = "BLND" }, SelectionEvaluationStatus.Accepted, 0m, "Free, zero total"),
        new("S2M-A17", "A17", SelectionKind.TypedForm, () => new() { EquipmentCode = "AOXY", EvidenceDocumentRefs = ["MEDIF-1"], OxygenUnits = 1m }, SelectionEvaluationStatus.Incomplete, null, "FailClosed: medical approval not verified"),
        new("S2M-A18", "A18", SelectionKind.TypedForm, () => new() { InfantRef = "T2", GuardianRef = "T1" }, SelectionEvaluationStatus.Accepted, 0m, "Free, verified infant-guardian pair"),
        new("S2M-A19", "A19", SelectionKind.TypedForm, () => new() { ChildRef = "T1", GuardianHandoffContactProvided = true, GuardianPickupContactProvided = true }, SelectionEvaluationStatus.Incomplete, null, "FailClosed: guardian contacts not verified"),
        new("S2M-A20", "A20", SelectionKind.TypedForm, () => new() { AirportId = Trip.Thr }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable, check at reserve"),
        new("S2M-A21", "A21", SelectionKind.TypedForm, () => new() { AirportId = Trip.Thr }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable, check at reserve"),
        new("S2M-A22", "A22", SelectionKind.TypedForm, () => new() { AirportId = Trip.Ika, FacilityRef = 7001, TimeWithOffset = new DateTimeOffset(2026, 12, 1, 8, 0, 0, Trip.TehranOffset) }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable per passenger; Order scope needs a PerItem rate"),
        new("S2M-A23", "A23", SelectionKind.SimpleOptIn, () => new() { OptIn = true }, SelectionEvaluationStatus.Accepted, 0m, "Free, zero total"),
        new("S2M-A24", "A24", SelectionKind.TypedForm, () => new() { PlanCode = "WIFI_FULL_FLIGHT", DeviceCount = 1 }, SelectionEvaluationStatus.Accepted, Amount, "Purchasable")
    ];

    public static TheoryData<string> Codes => new(Matrix.Select(row => row.Code));

    [Fact]
    public void S2M_00_the_matrix_has_exactly_one_row_per_registered_variant()
    {
        Assert.Equal(24, AncillaryVariant.All.Count);
        Assert.Equal(AncillaryVariant.All.Select(variant => variant.Code).Order(StringComparer.Ordinal), Matrix.Select(row => row.Code));
        Assert.Equal(24, Matrix.Select(row => row.TestId).Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public async Task S2M_each_variant_has_a_source_backed_positive_or_fail_closed_result_and_a_negative_and_is_never_purchasable_from_the_list(string code)
    {
        var row = Matrix.Single(entry => entry.Code == code);
        var lab = new ShoppingLab();
        var variant = V122Catalog.Case(code);
        var definition = lab.Definition(code);
        var provision = lab.Provision(definition);

        if (provision.PriceOrigin == PriceOrigin.Filed)
            lab.Price(definition, provision, Rate(Amount));

        if (variant.Authority == InventoryAuthority.Supplier)
            lab.Policy(definition, InventoryAuthority.Supplier);

        var context = code switch
        {
            "A02" => Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] },
            "A07" => Trip.Context().WithSeat("14C", "E"),
            "A18" => Trip.Context(travellers: [Trip.Adult("T1"), Infant()]),
            "A19" => Trip.Context(travellers: [Trip.Child("T1")]),
            "A22" => Trip.Context([Trip.Flight(origin: Trip.Ika)]),
            _ => Trip.Context()
        };
        var listed = (await lab.Engine.ShopAsync(context, new ShoppingFilter { IncludeNonSellable = true })).Candidates.First(candidate => candidate.CandidateIdentity.TravellerRef == "T1");
        var positive = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, row.Positive());
        var negative = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection());

        Assert.Equal(row.Kind, listed.Selection.Kind);
        Assert.False(OfferPurchase.CanPurchase(listed));
        Assert.Equal((row.PositiveStatus, row.LineTotal), (positive.Status, positive.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal(row.LineTotal is not null, OfferPurchase.CanPurchase(positive));
        Assert.NotEqual(SelectionEvaluationStatus.Accepted, negative.Status);
        Assert.False(OfferPurchase.CanPurchase(negative));
    }
}
