using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class EngineVariantTests
{
    private sealed record Shop(ShoppingLab Lab, AncillaryShoppingContext Context, IReadOnlyList<CanonicalAncillaryOfferCandidate> Candidates)
    {
        public CanonicalAncillaryOfferCandidate Shown => Candidates[0];

        public Task<CanonicalAncillarySelectionEvaluation> ChooseAsync(AncillarySelection selection, AncillaryShoppingContext? context = null)
            => Lab.Engine.EvaluateSelectionAsync(context ?? Context, Shown.CandidateIdentity, selection);
    }

    private static async Task<Shop> ShopAsync(
        string code,
        AncillaryShoppingContext? context = null,
        ProvisionRulesArgs? rules = null,
        Func<ServiceSpecificationInput, ServiceSpecificationInput>? specification = null,
        ServiceDateBasis? basis = null,
        ConfirmationRequirement? confirmation = null,
        Action<ShoppingLab, Domain.AncillaryServiceDefinitionAggregate.AncillaryServiceDefinition>? arrange = null,
        bool diagnostics = false)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code, specification: specification, basis: basis, confirmation: confirmation);
        var provision = lab.Provision(definition, rules: rules);

        if (provision.PriceOrigin == PriceOrigin.Filed)
            lab.Price(definition, provision, Rate(100m));

        arrange?.Invoke(lab, definition);

        var trip = context ?? Trip.Context();

        return new Shop(lab, trip, (await lab.Engine.ShopAsync(trip, new ShoppingFilter { IncludeNonSellable = diagnostics })).Candidates);
    }

    private static void Rejected(CanonicalAncillarySelectionEvaluation evaluation, string field, string reasonCode)
    {
        Assert.Equal(SelectionEvaluationStatus.Rejected, evaluation.Status);
        Assert.Contains(evaluation.FieldIssues, issue => issue.Field == field && issue.ReasonCode == reasonCode);
    }

    private static AncillaryShoppingContext Connection(params ShoppingTraveller[] travellers)
    {
        var first = Trip.Flight("F1", 101, Trip.Thr, Trip.Ika);
        var second = Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, first.DepartureAt.AddHours(5));

        return Trip.Context([first, second], travellers: travellers.Length == 0 ? null : travellers);
    }

    [Fact]
    public async Task E_A01_an_extra_checked_bag_is_sold_per_portion_by_quantity_on_top_of_the_verified_allowance()
    {
        var shop = await ShopAsync("A01", Connection());
        var shown = shop.Shown;

        Assert.Single(shop.Candidates);
        Assert.Equal((OfferReadiness.Selectable, ServiceCoverageScope.Portion, 100m, SelectionKind.QuantityChoice), (shown.OfferReadiness, shown.Scope.CoverageScope, shown.Price.CompleteUnitTotal, shown.Selection.Kind));
        Assert.Equal(new[] { "F1", "F2" }, shown.FlightRefs);
        Assert.Equal((AncillaryQuantityUnit.Piece, 1, 6, true), (shown.Quantity.Unit, shown.Quantity.MinPerSelection, shown.Quantity.MaxPerSelection, shown.Quantity.ZeroIsDeselect));

        var two = await shop.ChooseAsync(new AncillarySelection { Quantity = 2 });

        Assert.Equal((SelectionEvaluationStatus.Accepted, 200m, 100m), (two.Status, two.Candidate.Price.RequestedQuantityTotal, two.Candidate.Price.CompleteUnitTotal));
        Rejected(await shop.ChooseAsync(new AncillarySelection { Quantity = 0 }), "Quantity", ShoppingReasonCodes.QuantityBelowMinimum);
        Rejected(await shop.ChooseAsync(new AncillarySelection { Quantity = 7 }), "Quantity", ShoppingReasonCodes.QuantityAboveMaximum);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(new AncillarySelection())).Status);
    }

    [Fact]
    public async Task E_A02_a_weight_package_is_priced_per_package_never_per_kilogram()
    {
        var shop = await ShopAsync("A02");
        var two = await shop.ChooseAsync(new AncillarySelection { PackageProductRef = "PACK10", Quantity = 2 });

        Assert.Equal((OfferReadiness.Selectable, PricingUnit.PerItem, AncillaryQuantityUnit.Each, 2), (shop.Shown.OfferReadiness, shop.Shown.Price.PricingUnit, shop.Shown.Quantity.Unit, shop.Shown.Quantity.MaxPerSelection));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 200m), (two.Status, two.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(new AncillarySelection { Quantity = 1 })).Status);
        Rejected(await shop.ChooseAsync(new AncillarySelection { PackageProductRef = "PACK10", Quantity = 3 }), "Quantity", ShoppingReasonCodes.QuantityAboveMaximum);
    }

    [Fact]
    public async Task E_A03_an_overweight_bag_needs_its_measured_weight_inside_the_authored_bracket()
    {
        var shop = await ShopAsync("A03");

        Assert.Equal((OfferReadiness.NeedsSelection, SelectionKind.TypedForm), (shop.Shown.OfferReadiness, shop.Shown.Selection.Kind));
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 27m })).Status);
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 32m })).Status);
        Rejected(await shop.ChooseAsync(new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 23m }), "MeasuredWeightKg", ShoppingReasonCodes.BaggageWeightOutsideBracket);
        Rejected(await shop.ChooseAsync(new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 32.5m }), "MeasuredWeightKg", ShoppingReasonCodes.BaggageWeightOutsideBracket);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(new AncillarySelection { BagRef = "BAG-1" })).Status);
    }

    [Fact]
    public async Task E_A04_an_oversize_bag_needs_dimensions_within_the_maximum_and_may_stay_subject_to_confirmation()
    {
        var shop = await ShopAsync("A04", confirmation: ConfirmationRequirement.SubjectToConfirmation);

        Assert.Equal((OfferReadiness.NeedsSelection, ConfirmationRequirement.SubjectToConfirmation), (shop.Shown.OfferReadiness, shop.Shown.Confirmation.ConfirmationRequirement));
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(new AncillarySelection { Dimensions = new(60m, 110m, 30m) })).Status);
        Rejected(await shop.ChooseAsync(new AncillarySelection { Dimensions = new(130m, 40m, 20m) }), "Dimensions", ShoppingReasonCodes.BaggageDimensionsAboveMaximum);
        Rejected(await shop.ChooseAsync(new AncillarySelection { Dimensions = new(118m, 60m, 30m) }), "Dimensions", ShoppingReasonCodes.BaggageDimensionsAboveMaximum);
        Rejected(await shop.ChooseAsync(new AncillarySelection { Dimensions = new(100m, 50m, 30m), MeasuredWeightKg = 27m }), "MeasuredWeightKg", ShoppingReasonCodes.SelectionFieldNotAllowed);
    }

    [Fact]
    public async Task E_A05_an_extra_cabin_bag_needs_the_verified_cabin_allowance()
    {
        var known = await ShopAsync("A05");
        var unknown = await ShopAsync("A05", Trip.Context() with { TravellerBaggageFacts = [Trip.Baggage("T1", "F1", "P1") with { CabinPieces = null }] });
        var missing = await ShopAsync("A05", Trip.Context() with { TravellerBaggageFacts = [] });

        Assert.Equal((OfferReadiness.Selectable, 100m, 1), (known.Shown.OfferReadiness, known.Shown.Price.CompleteUnitTotal, known.Shown.Quantity.MaxPerSelection));
        Assert.Equal((OfferReadiness.NeedsVerification, EligibilityStatus.InsufficientContext), (unknown.Shown.OfferReadiness, unknown.Shown.Eligibility.Status));
        Assert.Contains(ShoppingReasonCodes.BaggageAllowanceNotVerified, unknown.Shown.ReasonCodes);
        Assert.Contains("TravellerBaggageFacts[T1,F1]", missing.Shown.Eligibility.MissingContextFields);
    }

    [Fact]
    public async Task E_A06_sports_equipment_is_a_typed_form_with_a_delegated_supplier_and_no_local_capacity()
    {
        var shop = await ShopAsync("A06", arrange: (lab, definition) => lab.Policy(definition, InventoryAuthority.Supplier));
        var bike = new AncillarySelection { EquipmentKind = "bike", Dimensions = new(190m, 70m, 30m), WeightKg = 20m };
        var accepted = await shop.ChooseAsync(bike);

        Assert.Equal(OfferReadiness.NeedsSelection, shop.Shown.OfferReadiness);
        Assert.Equal((SelectionEvaluationStatus.Accepted, InventoryCapacityReadState.DelegatedCheckRequired, false), (accepted.Status, accepted.Candidate.Availability.ConfigState, accepted.Candidate.Availability.IsGuaranteed));
        Assert.Null(accepted.Candidate.Availability.ConsumptionPreview);
        Rejected(await shop.ChooseAsync(bike with { EquipmentKind = "SURF" }), "EquipmentKind", ShoppingReasonCodes.EquipmentKindNotAllowed);
        Rejected(await shop.ChooseAsync(bike with { WeightKg = 33m }), "WeightKg", ShoppingReasonCodes.BaggageWeightAboveMaximum);
        Rejected(await shop.ChooseAsync(bike with { Dimensions = new(210m, 70m, 30m) }), "Dimensions", ShoppingReasonCodes.BaggageDimensionsAboveMaximum);
    }

    [Fact]
    public async Task E_A07_a_standard_seat_is_a_seat_map_selection_and_metadata_never_confirms_an_assignment()
    {
        var shop = await ShopAsync("A07");
        var chosen = await shop.ChooseAsync(new AncillarySelection { SeatNumber = "14C", VerifiedSeatCharacteristicCodes = ["E"] });

        Assert.Equal((OfferReadiness.NeedsSelection, SelectionKind.SeatMapSelection, ServiceCoverageScope.Sector), (shop.Shown.OfferReadiness, shop.Shown.Selection.Kind, shop.Shown.Scope.CoverageScope));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 100m, OfferReadiness.Selectable), (chosen.Status, chosen.Candidate.Price.CompleteUnitTotal, chosen.Candidate.OfferReadiness));
        Assert.Equal((false, ProviderConfirmationStatus.NotRequested), (chosen.Candidate.Availability.IsGuaranteed, chosen.Candidate.Confirmation.ProviderConfirmationStatus));
        Assert.Equal(1, chosen.Candidate.Quantity.MaxPerSelection);
    }

    [Fact]
    public async Task E_A08_an_exit_row_seat_needs_verified_eligibility_accepted_terms_and_a_verified_characteristic()
    {
        var adult = await ShopAsync("A08");
        var child = await ShopAsync("A08", Trip.Context(travellers: [Trip.Child("T1")]));
        var unverified = await ShopAsync("A08", Trip.Context(travellers: [Trip.Adult() with { VerifiedExitRowEligible = null }]));
        var seat = new AncillarySelection { SeatNumber = "12A", VerifiedSeatCharacteristicCodes = ["E"], ExitRowTermsAccepted = true };

        Assert.Equal(OfferReadiness.NeedsSelection, adult.Shown.OfferReadiness);
        Assert.Contains(adult.Shown.Selection.Fields, field => field is { Name: "ExitRowTermsAccepted", Required: true });
        Assert.Empty(child.Candidates);
        Assert.Equal(OfferReadiness.NeedsVerification, unverified.Shown.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.ExitRowEligibilityNotVerified, unverified.Shown.ReasonCodes);
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await adult.ChooseAsync(seat)).Status);
        Rejected(await adult.ChooseAsync(seat with { ExitRowTermsAccepted = false }), "ExitRowTermsAccepted", ShoppingReasonCodes.ExitRowTermsNotAccepted);
        Rejected(await adult.ChooseAsync(seat with { VerifiedSeatCharacteristicCodes = ["W"] }), "SeatNumber", ShoppingReasonCodes.SeatCharacteristicNotAllowed);
        Assert.Equal(OfferReadiness.NeedsVerification, (await adult.ChooseAsync(seat with { VerifiedSeatCharacteristicCodes = null })).Candidate.OfferReadiness);
    }

    [Fact]
    public async Task E_A09_an_extra_seat_is_its_own_product_with_a_purpose_and_ticket_or_exchange_routing()
    {
        var shop = await ShopAsync("A09");
        var seat = new AncillarySelection { SeatNumber = "14B", VerifiedSeatCharacteristicCodes = ["E"], Purpose = ExtraSeatPurpose.PassengerComfort };

        Assert.Equal(("A09", DocumentRouting.TicketOrExchange, OfferReadiness.NeedsSelection), (shop.Shown.VariantCode, shop.Shown.Document.Routing, shop.Shown.OfferReadiness));
        Assert.Contains(shop.Shown.Selection.Fields, field => field is { Name: "Purpose", Required: true });
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(seat)).Status);
        Rejected(await shop.ChooseAsync(seat with { Purpose = ExtraSeatPurpose.CabinBaggage }), "Purpose", ShoppingReasonCodes.ExtraSeatPurposeMismatch);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(seat with { Purpose = null })).Status);
    }

    [Fact]
    public async Task E_A10_a_cabin_upgrade_needs_a_quote_and_never_shows_an_amount()
    {
        var shop = await ShopAsync("A10");
        var otherCabin = await ShopAsync("A10", Trip.Context() with { TravellerFareFacts = [Trip.Fare("T1", "F1", "P1") with { CabinClassId = 2 }] });
        var otherFamily = await ShopAsync("A10", Trip.Context() with { TravellerFareFacts = [Trip.Fare("T1", "F1", "P1") with { FareFamilyId = 9 }] });
        var textFamily = await ShopAsync("A10", Trip.Context() with { TravellerFareFacts = [Trip.Fare("T1", "F1", "P1") with { FareFamilyId = null, FareFamilyCodeOrName = "5" }] });
        var quoted = await shop.ChooseAsync(new AncillarySelection { TargetCabinId = 2, QuoteRef = "Q-1" });

        Assert.Equal((OfferReadiness.NeedsQuote, PriceAssessmentStatus.QuoteRequired, DocumentRouting.TicketOrExchange), (shop.Shown.OfferReadiness, shop.Shown.Price.Status, shop.Shown.Document.Routing));
        Assert.Null(shop.Shown.Price.CompleteUnitTotal);
        Assert.Equal(Quote, shop.Shown.Price.QuoteProviderKey);
        Assert.Empty(otherCabin.Candidates);
        Assert.Empty(otherFamily.Candidates);
        Assert.Equal(OfferReadiness.NeedsVerification, textFamily.Shown.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.FareFamilyNotVerified, textFamily.Shown.ReasonCodes);
        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.NeedsQuote), (quoted.Status, quoted.Candidate.OfferReadiness));
        Assert.Null(quoted.Candidate.Price.RequestedQuantityTotal);
        Rejected(await shop.ChooseAsync(new AncillarySelection { TargetCabinId = 3, QuoteRef = "Q-1" }), "TargetCabinId", ShoppingReasonCodes.UpgradeTargetCabinMismatch);
    }

    [Fact]
    public async Task E_A11_a_free_special_meal_has_no_price_respects_the_catering_cutoff_and_the_meal_family()
    {
        var shop = await ShopAsync("A11");
        var late = await ShopAsync("A11", Trip.Context([Trip.Flight(departure: Now.AddHours(12).ToOffset(Trip.TehranOffset))]));
        var taken = await ShopAsync(
            "A11",
            Trip.Context() with
            {
                ExistingServiceFacts = [new() { ServiceRef = "MEAL_PASTA", CountingFamilyCode = "MAIN_MEAL", TravellerRef = "T1", FlightRefs = ["F1"], Quantity = 1, CommercialState = ExistingServiceCommercialState.Active }]
            });
        var chosen = await shop.ChooseAsync(new AncillarySelection { MealCode = "vgml" });

        Assert.Equal((OfferReadiness.NeedsSelection, PriceOrigin.Free), (shop.Shown.OfferReadiness, shop.Shown.Price.Origin));
        Assert.Null(shop.Shown.Price.CompleteUnitTotal);
        Assert.Equal((BookingMethod.Ssr, "VGML"), (shop.Shown.Booking.Method, shop.Shown.Booking.SsrCode));
        Assert.Empty(late.Candidates);
        Assert.Empty(taken.Candidates);
        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable), (chosen.Status, chosen.Candidate.OfferReadiness));
        Rejected(await shop.ChooseAsync(new AncillarySelection { MealCode = "KSML" }), "MealCode", ShoppingReasonCodes.MealCodeMismatch);
    }

    [Fact]
    public async Task E_A12_a_paid_pre_order_meal_is_priced_per_item_for_the_authored_menu_item()
    {
        var shop = await ShopAsync("A12");
        var late = await ShopAsync("A12", Trip.Context([Trip.Flight(departure: Now.AddHours(40).ToOffset(Trip.TehranOffset))]));
        var two = await shop.ChooseAsync(new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = 2 });

        Assert.Equal((OfferReadiness.Selectable, PricingUnit.PerItem, 100m), (shop.Shown.OfferReadiness, shop.Shown.Price.PricingUnit, shop.Shown.Price.CompleteUnitTotal));
        Assert.Empty(late.Candidates);
        Assert.Equal((SelectionEvaluationStatus.Accepted, 200m), (two.Status, two.Candidate.Price.RequestedQuantityTotal));
        Rejected(await shop.ChooseAsync(new AncillarySelection { MenuItemRef = "MENU_FISH_02", Quantity = 1 }), "MenuItemRef", ShoppingReasonCodes.MenuItemMismatch);
        Rejected(await shop.ChooseAsync(new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = 1, MealCode = "VGML" }), "MealCode", ShoppingReasonCodes.SelectionFieldNotAllowed);
    }

    [Fact]
    public async Task E_A13_a_cabin_pet_is_checked_on_every_covered_flight_and_stays_subject_to_supplier_approval()
    {
        var outbound = Trip.Flight("F1", 101, Trip.Thr, Trip.Ika);
        var onward = Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, outbound.DepartureAt.AddHours(5));
        var back = Trip.Flight("F3", 103, Trip.Ist, Trip.Thr, outbound.DepartureAt.AddDays(7), Trip.Istanbul);
        var bothWays = Trip.Context([outbound, onward, back], [Trip.Portion("P1", 1, outbound, onward), Trip.Portion("P2", 2, back)]);
        var rules = Rules(flights: new([], [], [], [], [1]), pet: new(null, null, 6m, ConfirmationRequirement.SubjectToConfirmation));
        var shop = await ShopAsync("A13", bothWays, rules, arrange: (lab, definition) => lab.Policy(definition, InventoryAuthority.Supplier));
        var otherAircraft = await ShopAsync("A13", bothWays with { Flights = [outbound, onward with { AircraftId = 4 }, back] }, rules);
        var cat = new AncillarySelection { AnimalType = PetAnimalType.Cat, CombinedWeightKg = 5m, CarrierDimensions = new(50m, 35m, 20m), DocumentAcknowledgements = ["ENTRY_RULES_ACK"] };
        var accepted = await shop.ChooseAsync(cat);

        Assert.Equal(new[] { "P1", "P2" }, shop.Candidates.Select(candidate => Assert.Single(candidate.PortionRefs)));
        Assert.Equal(new[] { "F1", "F2" }, shop.Shown.FlightRefs);
        Assert.Equal(new[] { "P2" }, otherAircraft.Candidates.Select(candidate => Assert.Single(candidate.PortionRefs)));
        Assert.Equal((OfferReadiness.NeedsSelection, ConfirmationRequirement.SubjectToConfirmation), (shop.Shown.OfferReadiness, shop.Shown.Confirmation.ConfirmationRequirement));
        Assert.Contains(ShoppingReasonCodes.SupplierConfirmationRequired, shop.Shown.ReasonCodes);
        Assert.Contains(ShoppingReasonCodes.PetAgeNotVerified, shop.Shown.ReasonCodes);
        Assert.Equal((SelectionEvaluationStatus.Accepted, InventoryCapacityReadState.DelegatedCheckRequired, false), (accepted.Status, accepted.Candidate.Availability.ConfigState, accepted.Candidate.Availability.IsGuaranteed));
        Rejected(await shop.ChooseAsync(cat with { CombinedWeightKg = 7m }), "CombinedWeightKg", ShoppingReasonCodes.PetWeightAboveMaximum);
        Rejected(await shop.ChooseAsync(cat with { AnimalType = PetAnimalType.RegisteredOther, OtherAnimalCode = "RABBIT" }), "AnimalType", ShoppingReasonCodes.PetAnimalNotAllowed);
        Rejected(await shop.ChooseAsync(cat with { CarrierDimensions = new(60m, 35m, 20m) }), "CarrierDimensions", ShoppingReasonCodes.PetCarrierAboveMaximum);
        Rejected(await shop.ChooseAsync(cat with { DocumentAcknowledgements = [] }), "DocumentAcknowledgements", ShoppingReasonCodes.PetDocumentsNotAcknowledged);
    }

    [Fact]
    public async Task E_A14_a_hold_pet_needs_the_named_size_bracket_that_contains_its_weight()
    {
        var shop = await ShopAsync("A14");
        var dog = new AncillarySelection
        {
            AnimalType = PetAnimalType.Dog,
            CombinedWeightKg = 20m,
            CarrierDimensions = new(100m, 70m, 75m),
            DocumentAcknowledgements = ["HEALTH_CERT"],
            SizeBracket = "MEDIUM"
        };

        Assert.Equal(("A14", OfferReadiness.NeedsSelection), (shop.Shown.VariantCode, shop.Shown.OfferReadiness));
        Assert.Contains(shop.Shown.Selection.Fields, field => field is { Name: "SizeBracket", Required: true });
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(dog)).Status);
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(dog with { CombinedWeightKg = 40m, SizeBracket = "LARGE" })).Status);
        Rejected(await shop.ChooseAsync(dog with { SizeBracket = "LARGE" }), "SizeBracket", ShoppingReasonCodes.PetSizeBracketMismatch);
        Rejected(await shop.ChooseAsync(dog with { CombinedWeightKg = 76m, SizeBracket = "LARGE" }), "CombinedWeightKg", ShoppingReasonCodes.PetWeightAboveMaximum);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(dog with { SizeBracket = null })).Status);
    }

    [Fact]
    public async Task E_A15_wheelchair_assistance_is_a_free_request_with_an_allowed_code_and_a_lead_time()
    {
        var shop = await ShopAsync("A15");
        var late = await ShopAsync("A15", Trip.Context([Trip.Flight(departure: Now.AddHours(24).ToOffset(Trip.TehranOffset))]));
        var chosen = await shop.ChooseAsync(new AncillarySelection { AssistanceSsrCode = "WCHS" });

        Assert.Equal((OfferReadiness.NeedsSelection, PriceOrigin.Free, DocumentRouting.NoAncillaryDocument, false), (shop.Shown.OfferReadiness, shop.Shown.Price.Origin, shop.Shown.Document.Routing, shop.Shown.Document.DocumentRequired));
        Assert.Empty(shop.Shown.Price.AddedTaxLines.Concat(shop.Shown.Price.AppliedUnitFeeLines));
        Assert.Empty(late.Candidates);
        Assert.Equal((SelectionEvaluationStatus.Accepted, ConfirmationRequirement.SubjectToConfirmation), (chosen.Status, chosen.Candidate.Confirmation.ConfirmationRequirement));
        Assert.Contains(ShoppingReasonCodes.SupplierConfirmationRequired, chosen.Candidate.ReasonCodes);
        Assert.Null(chosen.Candidate.Price.RequestedQuantityTotal);
        Rejected(await shop.ChooseAsync(new AncillarySelection { AssistanceSsrCode = "BLND" }), "AssistanceSsrCode", ShoppingReasonCodes.AssistanceCodeNotAllowed);
    }

    [Fact]
    public async Task E_A16_disability_assistance_takes_only_its_typed_code_and_no_medical_data()
    {
        var shop = await ShopAsync("A16");

        Assert.Equal(new[] { "AssistanceSsrCode", "TravellerRef", "FlightRef" }, shop.Shown.Selection.Fields.Select(field => field.Name));
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(new AncillarySelection { AssistanceSsrCode = "DEAF" })).Status);
        Rejected(await shop.ChooseAsync(new AncillarySelection { AssistanceSsrCode = "WCHR" }), "AssistanceSsrCode", ShoppingReasonCodes.AssistanceCodeNotAllowed);
        Rejected(await shop.ChooseAsync(new AncillarySelection { AssistanceSsrCode = "BLND", OxygenUnits = 1m }), "OxygenUnits", ShoppingReasonCodes.SelectionFieldNotAllowed);
        Rejected(await shop.ChooseAsync(new AncillarySelection { AssistanceSsrCode = "BLND", EvidenceDocumentRefs = ["DOC-1"] }), "EvidenceDocumentRefs", ShoppingReasonCodes.SelectionFieldNotAllowed);
    }

    [Fact]
    public async Task E_A17_medical_equipment_takes_evidence_references_and_stays_quoted_and_subject_to_approval()
    {
        var shop = await ShopAsync("A17");
        var oxygen = new AncillarySelection { EquipmentCode = "AOXY", EvidenceDocumentRefs = ["MEDIF-REF-1"], OxygenUnits = 2m };
        var accepted = await shop.ChooseAsync(oxygen);

        Assert.Equal((OfferReadiness.NeedsSelection, PriceAssessmentStatus.QuoteRequired), (shop.Shown.OfferReadiness, shop.Shown.Price.Status));
        Assert.Contains(ShoppingReasonCodes.MedicalApprovalRequired, shop.Shown.ReasonCodes);
        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.NeedsQuote, ConfirmationRequirement.SubjectToConfirmation), (accepted.Status, accepted.Candidate.OfferReadiness, accepted.Candidate.Confirmation.ConfirmationRequirement));
        Assert.Null(accepted.Candidate.Price.CompleteUnitTotal);
        Rejected(await shop.ChooseAsync(oxygen with { OxygenUnits = 3m }), "OxygenUnits", ShoppingReasonCodes.OxygenUnitsAboveMaximum);
        Rejected(await shop.ChooseAsync(oxygen with { EvidenceDocumentRefs = [] }), "EvidenceDocumentRefs", ShoppingReasonCodes.MedicalEvidenceMissing);
        Rejected(await shop.ChooseAsync(oxygen with { EquipmentCode = "STCR" }), "EquipmentCode", ShoppingReasonCodes.AssistanceCodeNotAllowed);
        Assert.DoesNotContain(
            typeof(AncillarySelection).GetProperties(),
            property => property.Name.Contains("Diagnosis", StringComparison.Ordinal) || property.Name.Contains("Passport", StringComparison.Ordinal) || property.Name.Contains("Contact", StringComparison.Ordinal) && property.PropertyType != typeof(bool));
    }

    [Fact]
    public async Task E_A18_a_bassinet_needs_the_infant_and_its_guardian_and_has_no_capacity_of_its_own()
    {
        ShoppingTraveller Infant(DateOnly dateOfBirth) => new() { TravellerRef = "T2", PassengerTypeCode = PassengerTypeCode.INF, DateOfBirth = dateOfBirth, AssociatedAdultRef = "T1" };

        var family = Trip.Context(travellers: [Trip.Adult("T1"), Infant(new DateOnly(2026, 6, 1))]);
        var shop = await ShopAsync("A18", family, arrange: (lab, definition) => lab.Policy(definition, InventoryAuthority.Supplier));
        var pair = new AncillarySelection { InfantRef = "T2", GuardianRef = "T1" };
        var accepted = await shop.ChooseAsync(pair);
        var older = Trip.Context(travellers: [Trip.Adult("T1"), Infant(new DateOnly(2025, 12, 1))]);

        Assert.Equal(new[] { "InfantRef", "GuardianRef", "FlightRef" }, shop.Shown.Selection.Fields.Select(field => field.Name));
        Assert.Equal((OfferReadiness.NeedsSelection, PriceOrigin.Free), (shop.Shown.OfferReadiness, shop.Shown.Price.Origin));
        Assert.Equal((SelectionEvaluationStatus.Accepted, InventoryCapacityReadState.DelegatedCheckRequired, false), (accepted.Status, accepted.Candidate.Availability.ConfigState, accepted.Candidate.Availability.IsGuaranteed));
        Assert.Null(accepted.Candidate.Availability.ConsumptionPreview);
        Rejected(await shop.ChooseAsync(pair with { GuardianRef = "T2" }), "GuardianRef", ShoppingReasonCodes.InfantGuardianRequired);
        Rejected(await shop.ChooseAsync(pair with { InfantRef = "T9" }), "GuardianRef", ShoppingReasonCodes.InfantGuardianRequired);
        Rejected(await shop.ChooseAsync(pair, older), "InfantRef", ShoppingReasonCodes.InfantTooOld);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(new AncillarySelection { InfantRef = "T2" })).Status);
    }

    [Fact]
    public async Task E_A19_an_unaccompanied_minor_needs_the_age_band_a_direct_portion_and_both_guardian_contacts()
    {
        var shop = await ShopAsync("A19", Trip.Context(travellers: [Trip.Child("T1")]));
        var adult = await ShopAsync("A19");
        var tooOld = await ShopAsync("A19", Trip.Context(travellers: [Trip.Child("T1", new DateOnly(2014, 11, 30))]));
        var connecting = await ShopAsync("A19", Connection(Trip.Child("T1")));
        var contacts = new AncillarySelection { GuardianHandoffContactProvided = true, GuardianPickupContactProvided = true };

        Assert.Equal((OfferReadiness.NeedsSelection, ServiceCoverageScope.Portion), (shop.Shown.OfferReadiness, shop.Shown.Scope.CoverageScope));
        Assert.Empty(adult.Candidates);
        Assert.Empty(tooOld.Candidates);
        Assert.Empty(connecting.Candidates);
        Assert.Equal((SelectionEvaluationStatus.Accepted, ConfirmationRequirement.SubjectToConfirmation), ((await shop.ChooseAsync(contacts)).Status, shop.Shown.Confirmation.ConfirmationRequirement));
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(contacts with { GuardianPickupContactProvided = false })).Status);
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(contacts with { ChildRef = "T1" })).Status);
        Rejected(await shop.ChooseAsync(contacts with { ChildRef = "T9" }), "ChildRef", ShoppingReasonCodes.SelectionFieldNotAllowed);
    }

    [Fact]
    public async Task E_A20_a_lounge_on_a_service_start_basis_needs_the_visit_time_in_the_zone_of_its_specification()
    {
        var window = Rules(dayTime: new([new(127, new TimeOnly(7, 0), new TimeOnly(21, 0), DayTimeRestrictionEffect.Allow)]));
        var shop = await ShopAsync("A20", rules: window, basis: ServiceDateBasis.ServiceStart);
        var visit = new AncillarySelection { AirportId = Trip.Thr, TimeWithOffset = new DateTimeOffset(2026, 12, 1, 8, 0, 0, Trip.TehranOffset), GuestCount = 1 };
        var noZone = await Assert.ThrowsAsync<AeroTech.Framework.Core.Domain.Exceptions.BusinessException>(() => ShopAsync(
            "A20",
            rules: window,
            specification: input => input with { AirportService = input.AirportService! with { IanaTimeZone = null } },
            basis: ServiceDateBasis.ServiceStart));
        var elsewhere = await ShopAsync("A20", Trip.Context([Trip.Flight(origin: Trip.Ika)]), basis: ServiceDateBasis.ServiceStart);

        Assert.Equal(OfferReadiness.NeedsSelection, shop.Shown.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.AppointmentRequired, shop.Shown.ReasonCodes);
        Assert.Empty(elsewhere.Candidates);
        Assert.Equal((SelectionEvaluationStatus.Accepted, 100m), ((await shop.ChooseAsync(visit)).Status, shop.Shown.Price.CompleteUnitTotal));
        Assert.Equal((16202, 422), (noZone.Code, noZone.HttpStatus));
        Rejected(await shop.ChooseAsync(visit with { TimeWithOffset = new DateTimeOffset(2026, 12, 1, 23, 0, 0, Trip.TehranOffset) }), "TimeWithOffset", ShoppingReasonCodes.AppointmentOutsideServiceWindow);
        Rejected(await shop.ChooseAsync(visit with { GuestCount = 2 }), "GuestCount", ShoppingReasonCodes.GuestsAboveMaximum);
        Rejected(await shop.ChooseAsync(visit with { AirportId = Trip.Ika }), "AirportId", ShoppingReasonCodes.AirportMismatch);
        Assert.Equal(SelectionEvaluationStatus.Rejected, (await shop.ChooseAsync(visit with { TimeWithOffset = new DateTimeOffset(2026, 12, 1, 6, 30, 0, Trip.TehranOffset) })).Status);
        Assert.Null(shop.Shown.Availability.ConsumptionPreview);
    }

    [Fact]
    public async Task E_A21_a_fast_track_time_is_read_through_the_zone_rules_including_daylight_saving_changes()
    {
        ServiceSpecificationInput Paris(ServiceSpecificationInput input)
            => input with { AirportService = input.AirportService! with { IanaTimeZone = Trip.Paris, ServiceWindowStart = new TimeOnly(3, 0), ServiceWindowEnd = new TimeOnly(4, 0) } };

        var shop = await ShopAsync("A21", specification: Paris);
        var unknown = await Assert.ThrowsAsync<AeroTech.Framework.Core.Domain.Exceptions.BusinessException>(
            () => ShopAsync("A21", specification: input => Paris(input) with { AirportService = Paris(input).AirportService! with { IanaTimeZone = "Nowhere/Zone" } }));

        AncillarySelection At(DateTimeOffset instant) => new() { AirportId = Trip.Thr, TimeWithOffset = instant };

        var afterGap = await shop.ChooseAsync(At(new DateTimeOffset(2027, 3, 28, 1, 30, 0, TimeSpan.Zero)));
        var beforeGap = await shop.ChooseAsync(At(new DateTimeOffset(2027, 3, 28, 0, 30, 0, TimeSpan.Zero)));
        var firstTwoThirty = await shop.ChooseAsync(At(new DateTimeOffset(2026, 10, 25, 0, 30, 0, TimeSpan.Zero)));
        var secondTwoThirty = await shop.ChooseAsync(At(new DateTimeOffset(2026, 10, 25, 1, 30, 0, TimeSpan.Zero)));
        var afterFallBack = await shop.ChooseAsync(At(new DateTimeOffset(2026, 10, 25, 2, 30, 0, TimeSpan.Zero)));

        Assert.Equal(OfferReadiness.NeedsSelection, shop.Shown.OfferReadiness);
        Assert.Equal(SelectionEvaluationStatus.Accepted, afterGap.Status);
        Rejected(beforeGap, "TimeWithOffset", ShoppingReasonCodes.AppointmentOutsideServiceWindow);
        Rejected(firstTwoThirty, "TimeWithOffset", ShoppingReasonCodes.AppointmentOutsideServiceWindow);
        Rejected(secondTwoThirty, "TimeWithOffset", ShoppingReasonCodes.AppointmentOutsideServiceWindow);
        Assert.Equal(SelectionEvaluationStatus.Accepted, afterFallBack.Status);
        Assert.Equal(16202, unknown.Code);
    }

    [Fact]
    public async Task E_A22_a_cip_package_is_one_charge_with_an_appointment_and_its_slot_stays_unproven_without_the_facility_source()
    {
        const long facility = 7001;

        var fromIka = Trip.Context([Trip.Flight(origin: Trip.Ika)]);
        var appointment = new DateTimeOffset(2026, 12, 1, 5, 0, 0, TimeSpan.Zero);
        var slot = new SlotInventorySource(facility, Trip.Ika, appointment.AddHours(-1), appointment.AddHours(1), InventoryRecordStatus.Active, false);
        var shop = await ShopAsync(
            "A22",
            fromIka,
            arrange: (lab, definition) => lab.Policy(
                definition,
                InventoryAuthority.Local,
                LocalInventoryPattern.AirportSlot,
                slot: AirportSlotConsumption.Create(facility, 120, 1),
                slotSources: [slot]));
        var visit = new AncillarySelection { AirportId = Trip.Ika, FacilityRef = facility, TimeWithOffset = appointment, GuestCount = 2 };
        var booked = await shop.ChooseAsync(visit);
        var outside = await shop.ChooseAsync(visit with { TimeWithOffset = appointment.AddHours(3) });

        Assert.Single(shop.Candidates);
        Assert.Equal((OfferReadiness.NeedsSelection, 100m, InventoryCapacityReadState.Unknown), (shop.Shown.OfferReadiness, shop.Shown.Price.CompleteUnitTotal, shop.Shown.Availability.ConfigState));
        Assert.Contains(shop.Shown.Selection.Fields, field => field is { Name: "TimeWithOffset", Required: true });
        Assert.Equal((SelectionEvaluationStatus.Accepted, 100m), (booked.Status, booked.Candidate.Price.CompleteUnitTotal));
        Assert.Equal(
            (InventoryCapacityReadState.ConfiguredNotGuaranteed, AvailabilityCheckState.SourceUnavailable, false, OfferReadiness.NeedsVerification),
            (booked.Candidate.Availability.ConfigState, booked.Candidate.Availability.CheckedState, booked.Candidate.Availability.IsGuaranteed, booked.Candidate.OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.BlockedExternalReference, booked.Candidate.ReasonCodes);
        Assert.Equal((120, 3), (booked.Candidate.Availability.ConsumptionPreview!.OccupancyMinutes, booked.Candidate.Availability.ConsumptionPreview.OccupiedPersons));
        Assert.Equal(InventoryCapacityReadState.NotConfigured, outside.Candidate.Availability.ConfigState);
        Rejected(await shop.ChooseAsync(visit with { FacilityRef = 7002 }), "FacilityRef", ShoppingReasonCodes.FacilityMismatch);
        Rejected(await shop.ChooseAsync(visit with { GuestCount = 3 }), "GuestCount", ShoppingReasonCodes.GuestsAboveMaximum);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(visit with { TimeWithOffset = null })).Status);
    }

    [Fact]
    public async Task E_A23_priority_is_not_sold_again_when_the_fare_already_includes_it()
    {
        FareBenefitFacts Benefit(bool included, FactEvidence evidence = FactEvidence.Verified)
            => new() { TravellerRef = "T1", FlightRefs = ["F1"], BenefitCode = "FLEX_BENEFIT", IncludedOrEntitled = included, EvidenceCompleteness = evidence };

        var shop = await ShopAsync("A23");
        var included = await ShopAsync("A23", Trip.Context() with { FareEntitlementFacts = [Benefit(true)] });
        var notIncluded = await ShopAsync("A23", Trip.Context() with { FareEntitlementFacts = [Benefit(false)] });
        var unknown = await ShopAsync("A23", Trip.Context() with { CoverageCompleteness = Trip.Context().CoverageCompleteness with { FareBenefitsComplete = false } });
        var unverified = await ShopAsync("A23", Trip.Context() with { FareEntitlementFacts = [Benefit(true, FactEvidence.Partial)] });
        var otherAirport = await ShopAsync("A23", Trip.Context([Trip.Flight(origin: Trip.Ika)]));

        Assert.Equal((OfferReadiness.Selectable, SelectionKind.SimpleOptIn), (shop.Shown.OfferReadiness, shop.Shown.Selection.Kind));
        Assert.Empty(included.Candidates);
        Assert.Equal(OfferReadiness.Selectable, notIncluded.Shown.OfferReadiness);
        Assert.Equal(OfferReadiness.NeedsVerification, unknown.Shown.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.FareBenefitsNotVerified, unknown.Shown.ReasonCodes);
        Assert.Equal(OfferReadiness.NeedsVerification, unverified.Shown.OfferReadiness);
        Assert.Empty(otherAirport.Candidates);
        Assert.Equal(SelectionEvaluationStatus.Accepted, (await shop.ChooseAsync(new AncillarySelection { OptIn = true })).Status);
    }

    [Fact]
    public async Task E_A24_wifi_needs_an_equipped_aircraft_the_right_stage_and_never_a_guaranteed_supply()
    {
        var shop = await ShopAsync("A24", arrange: (lab, definition) => lab.Policy(definition, InventoryAuthority.Supplier));
        var otherAircraft = await ShopAsync("A24", Trip.Context([Trip.Flight() with { AircraftId = 4 }]));
        var unknownAircraft = await ShopAsync("A24", Trip.Context([Trip.Flight() with { AircraftId = null }]));

        ServiceSpecificationInput OnBoard(ServiceSpecificationInput input) => input with { Connectivity = input.Connectivity! with { DeliveryStage = PurchaseStage.OnBoard } };

        var soldOnBoard = await ShopAsync("A24", Trip.Context(stage: ShoppingStage.OnBoard), specification: OnBoard);
        var notBefore = await ShopAsync("A24", Trip.Context(stage: ShoppingStage.PreOrder), specification: OnBoard);
        var notOnBoard = await ShopAsync("A24", Trip.Context(stage: ShoppingStage.OnBoard));
        var plan = await shop.ChooseAsync(new AncillarySelection { PlanCode = "FULL", DeviceCount = 2 });

        Assert.Equal((OfferReadiness.NeedsSelection, 100m), (shop.Shown.OfferReadiness, shop.Shown.Price.CompleteUnitTotal));
        Assert.Empty(otherAircraft.Candidates);
        Assert.Equal(OfferReadiness.NeedsVerification, unknownAircraft.Shown.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.AircraftNotVerified, unknownAircraft.Shown.ReasonCodes);
        Assert.Equal("A24", soldOnBoard.Shown.VariantCode);
        Assert.Empty(notBefore.Candidates);
        Assert.Empty(notOnBoard.Candidates);
        Assert.Equal(
            (SelectionEvaluationStatus.Accepted, InventoryCapacityReadState.DelegatedCheckRequired, false, true),
            (plan.Status, plan.Candidate.Availability.ConfigState, plan.Candidate.Availability.IsGuaranteed, plan.Candidate.Availability.RequiresCheckAtReserve));
        Rejected(await shop.ChooseAsync(new AncillarySelection { PlanCode = "FULL", DeviceCount = 3 }), "DeviceCount", ShoppingReasonCodes.DevicesAboveMaximum);
        Assert.Equal(SelectionEvaluationStatus.Incomplete, (await shop.ChooseAsync(new AncillarySelection { DeviceCount = 1 })).Status);
    }
}
