using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class EngineR1RegressionTests
{
    private static readonly ShoppingFilter Everything = new() { IncludeNonSellable = true };

    private static ServiceSpecificationInput NoMinimumPetAge(ServiceSpecificationInput input) => input with { Pet = input.Pet! with { MinAnimalAgeWeeks = null } };

    private static ProvisionBaggageApplicationArgs Bag(
        BaggageChargeKind chargeKind = BaggageChargeKind.ExtraPiece,
        BaggageAllowanceConcept? concept = BaggageAllowanceConcept.Piece,
        decimal? weight = null,
        WeightUnit unit = WeightUnit.Kg,
        BaggageTravelApplication? travel = null,
        BaggagePurchaseApplication purchase = BaggagePurchaseApplication.Prepaid,
        BaggageRuleDeference? deference = null)
        => new(null, null, null, weight, unit, travel, purchase, deference, chargeKind, concept);

    private static async Task<(ShoppingLab Lab, CanonicalAncillaryOfferResult Result)> BagAsync(
        string code,
        ProvisionBaggageApplicationArgs baggage,
        AncillaryShoppingContext? context = null)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition, rules: Rules(baggage: baggage)), Rate(100m));

        return (lab, await lab.Engine.ShopAsync(context ?? Trip.Context(), Everything));
    }

    private static void Blocked(CanonicalAncillaryOfferCandidate candidate, string reasonCode, string missingField)
    {
        Assert.Equal((OfferReadiness.NeedsVerification, EligibilityStatus.InsufficientContext), (candidate.OfferReadiness, candidate.Eligibility.Status));
        Assert.Contains(reasonCode, candidate.ReasonCodes);
        Assert.Contains(missingField, candidate.Eligibility.MissingContextFields);
    }

    [Fact]
    public async Task SEL_R1_01_an_upgrade_with_an_arbitrary_quote_reference_is_never_accepted_and_stays_without_an_amount()
    {
        var lab = new ShoppingLab();

        lab.Sellable("A10");

        var shown = (await lab.Engine.ShopAsync(Trip.Context())).One();
        var evaluation = await lab.Engine.EvaluateSelectionAsync(
            Trip.Context(),
            shown.CandidateIdentity,
            new AncillarySelection { TargetCabinId = 2, QuoteRef = "ANY-TEXT-TYPED-BY-THE-BUYER" });
        var price = evaluation.Candidate.Price;

        Assert.Empty(evaluation.FieldIssues);
        Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsQuote, PriceAssessmentStatus.QuoteRequired), (evaluation.Status, evaluation.Candidate.OfferReadiness, price.Status));
        Assert.Null(price.BaseAmount);
        Assert.Null(price.CompleteUnitTotal);
        Assert.Null(price.RequestedQuantityTotal);
        Assert.Null(price.ExternalQuoteRef);
        Assert.False(price.IsOrderLevelTotalComplete);
    }

    [Fact]
    public async Task SEL_R1_02_a_selection_is_accepted_only_when_the_candidate_is_selectable()
    {
        async Task<CanonicalAncillarySelectionEvaluation> ChosenAsync(Action<ShoppingLab, Domain.AncillaryServiceDefinitionAggregate.AncillaryServiceDefinition> arrange)
        {
            var lab = new ShoppingLab();
            var definition = lab.Definition("A23");

            arrange(lab, definition);

            var shown = (await lab.Engine.ShopAsync(Trip.Context(), Everything)).One();

            return await lab.Engine.EvaluateSelectionAsync(Trip.Context(), shown.CandidateIdentity, new AncillarySelection { OptIn = true });
        }

        void Sell(ShoppingLab lab, Domain.AncillaryServiceDefinitionAggregate.AncillaryServiceDefinition definition)
            => lab.Price(definition, lab.Provision(definition, disposition: CommercialDisposition.Paid), Rate(30m));

        var sold = await ChosenAsync(Sell);
        var notAvailable = await ChosenAsync((lab, definition) => lab.Provision(definition, disposition: CommercialDisposition.NotAvailable));
        var closed = await ChosenAsync((lab, definition) =>
        {
            Sell(lab, definition);
            lab.Policy(definition, InventoryAuthority.Unlimited, status: InventoryRecordStatus.Suspended);
        });
        var blockedReference = await ChosenAsync((lab, definition) =>
        {
            Sell(lab, definition);
            lab.Policy(definition, InventoryAuthority.FlightFlow);
        });
        var incompletePrice = await ChosenAsync((lab, definition) =>
        {
            Sell(lab, definition);
            lab.Catalog.CurrencyScales.Remove(Eur);
        });

        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable, false), (sold.Status, sold.Candidate.OfferReadiness, sold.Candidate.Availability.IsGuaranteed));
        Assert.Equal((PriceAssessmentStatus.Complete, 30m), (sold.Candidate.Price.Status, sold.Candidate.Price.CompleteUnitTotal));
        Assert.Equal((SelectionEvaluationStatus.Rejected, OfferReadiness.Unavailable), (notAvailable.Status, notAvailable.Candidate.OfferReadiness));
        Assert.Equal((SelectionEvaluationStatus.Rejected, OfferReadiness.Unavailable, InventoryCapacityReadState.ClosedForSale), (closed.Status, closed.Candidate.OfferReadiness, closed.Candidate.Availability.ConfigState));
        Assert.Equal(
            (SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification, AvailabilityCheckState.SourceUnavailable),
            (blockedReference.Status, blockedReference.Candidate.OfferReadiness, blockedReference.Candidate.Availability.CheckedState));
        Assert.Equal(
            (SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification, PriceAssessmentStatus.Incomplete),
            (incompletePrice.Status, incompletePrice.Candidate.OfferReadiness, incompletePrice.Candidate.Price.Status));
        Assert.Null(incompletePrice.Candidate.Price.CompleteUnitTotal);
    }

    [Fact]
    public async Task USG_R1_01_a_cumulative_limit_is_enforced_only_on_complete_usage_evidence_of_a_verified_family()
    {
        var lab = new ShoppingLab { Catalog = { FamilyAnswer = InventoryReferenceCheck.Verified } };
        var meal = lab.Definition("A12");

        lab.Price(meal, lab.Provision(meal, maxQuantity: 5), Rate(20m));

        var policy = lab.Policy(
            meal,
            InventoryAuthority.Unlimited,
            limits: new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerFlightOccurrence, 3, "MEAL_ITEMS", UsageConsumptionUnit.PurchasedUnit, null));
        var key = policy.PassengerUsageLimits.Single().KeyFor(new PassengerUsageSubject("PAX-T1", null, null, 101, null));

        AncillaryShoppingContext Used(decimal consumed, bool complete = true)
            => Trip.Context() with
            {
                UsageEvidence = [new() { UsageSubjectKey = key, UnitsConsumed = consumed, IsCompleteForScope = complete, AsOfUtc = Now, SourceAuthority = "TEST-LEDGER" }]
            };

        async Task<CanonicalAncillarySelectionEvaluation> BuyAsync(AncillaryShoppingContext context, int quantity)
        {
            var shown = (await lab.Engine.ShopAsync(context, Everything)).One();

            return await lab.Engine.EvaluateSelectionAsync(context, shown.CandidateIdentity, new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = quantity });
        }

        var within = await BuyAsync(Used(1m), 2);
        var beyond = await BuyAsync(Used(1m), 3);
        var incompleteLedger = await BuyAsync(Used(1m, false), 1);
        var noLedger = await BuyAsync(Trip.Context(), 1);
        var exhausted = await lab.Engine.ShopAsync(Used(3m));
        var exhaustedShown = (await lab.Engine.ShopAsync(Used(3m), Everything)).One();
        var exhaustedChoice = await BuyAsync(Used(3m), 1);

        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable, 40m), (within.Status, within.Candidate.OfferReadiness, within.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal((true, 1m, 2m), (within.Candidate.Quantity.CumulativeLimits.Single().FamilyVerified, within.Candidate.Quantity.CumulativeLimits.Single().VerifiedConsumed, within.Candidate.Quantity.CumulativeLimits.Single().VerifiedRemaining));
        Assert.Equal((SelectionEvaluationStatus.Rejected, OfferReadiness.NotEligible), (beyond.Status, beyond.Candidate.OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.QuantityExceedsVerifiedRemaining, beyond.Candidate.ReasonCodes);

        foreach (var unproven in new[] { incompleteLedger, noLedger })
        {
            Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (unproven.Status, unproven.Candidate.OfferReadiness));
            Assert.Contains(ShoppingReasonCodes.UsageNotVerified, unproven.Candidate.ReasonCodes);
            Assert.Null(unproven.Candidate.Quantity.CumulativeLimits.Single().VerifiedConsumed);
            Assert.Null(unproven.Candidate.Quantity.CumulativeLimits.Single().VerifiedRemaining);
        }

        Assert.Empty(exhausted.Candidates);
        Assert.Equal((OfferReadiness.NotEligible, 0m), (exhaustedShown.OfferReadiness, exhaustedShown.Quantity.CumulativeLimits.Single().VerifiedRemaining));
        Assert.Equal(SelectionEvaluationStatus.Rejected, exhaustedChoice.Status);

        var unverifiedUsage = await lab.Engine.ShopAsync(Trip.Context());

        Assert.True(unverifiedUsage.HasBlockedCandidates);
        Assert.Contains(unverifiedUsage.Diagnostics, diagnostic => diagnostic is { Code: ShoppingReasonCodes.UsageNotVerified, Severity: ShoppingDiagnosticSeverity.Blocker });

        lab.Catalog.FamilyAnswer = InventoryReferenceCheck.SourceUnavailable;

        var familyAbsent = await BuyAsync(Used(1m), 1);
        var familyAbsentBeyond = await BuyAsync(Used(1m), 3);

        foreach (var unproven in new[] { familyAbsent, familyAbsentBeyond })
        {
            Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (unproven.Status, unproven.Candidate.OfferReadiness));
            Assert.Contains(ShoppingReasonCodes.CountingFamilyNotVerified, unproven.Candidate.ReasonCodes);
            Assert.False(unproven.Candidate.Quantity.CumulativeLimits.Single().FamilyVerified);
        }
    }

    [Fact]
    public async Task PET_R1_01_a_minimum_pet_age_is_never_passed_by_an_unknown_or_a_declared_age()
    {
        async Task<(ShoppingLab Lab, CanonicalAncillaryOfferCandidate Shown)> PetAsync(
            ProvisionPetRuleArgs? rule = null,
            Func<ServiceSpecificationInput, ServiceSpecificationInput>? specification = null)
        {
            var lab = new ShoppingLab();
            var definition = lab.Definition("A13", specification: specification);

            lab.Price(definition, lab.Provision(definition, rules: Rules(pet: rule)), Rate(90m));

            return (lab, (await lab.Engine.ShopAsync(Trip.Context(), Everything)).One());
        }

        var cat = new AncillarySelection { AnimalType = PetAnimalType.Cat, CombinedWeightKg = 5m, CarrierDimensions = new(50m, 35m, 20m), DocumentAcknowledgements = ["ENTRY_RULES_ACK"] };
        var (lab, shown) = await PetAsync();

        Task<CanonicalAncillarySelectionEvaluation> ChooseAsync(ShoppingLab engine, CanonicalAncillaryOfferCandidate candidate, AncillarySelection selection)
            => engine.Engine.EvaluateSelectionAsync(Trip.Context(), candidate.CandidateIdentity, selection);

        var unknown = await ChooseAsync(lab, shown, cat);
        var tooYoung = await ChooseAsync(lab, shown, cat with { AnimalAgeWeeks = 11 });
        var declaredOldEnough = await ChooseAsync(lab, shown, cat with { AnimalAgeWeeks = 30 });

        Blocked(shown, ShoppingReasonCodes.PetAgeNotVerified, "VerifiedAnimalAgeWeeks");
        Assert.DoesNotContain(shown.Selection.Fields, field => field.Name.Contains("Age", StringComparison.Ordinal));
        Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (unknown.Status, unknown.Candidate.OfferReadiness));
        Assert.Equal((SelectionEvaluationStatus.Rejected, OfferReadiness.NotEligible), (tooYoung.Status, tooYoung.Candidate.OfferReadiness));
        Assert.Contains(tooYoung.FieldIssues, issue => issue is { Field: "AnimalAgeWeeks", ReasonCode: ShoppingReasonCodes.PetTooYoung });
        Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (declaredOldEnough.Status, declaredOldEnough.Candidate.OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.PetAgeNotVerified, declaredOldEnough.Candidate.ReasonCodes);

        var (overridden, overriddenShown) = await PetAsync(new ProvisionPetRuleArgs(null, 16, null, ConfirmationRequirement.SubjectToConfirmation));
        var youngerThanOverride = await ChooseAsync(overridden, overriddenShown, cat with { AnimalAgeWeeks = 14 });

        Assert.Equal(SelectionEvaluationStatus.Rejected, youngerThanOverride.Status);
        Assert.Contains(youngerThanOverride.FieldIssues, issue => issue is { Field: "AnimalAgeWeeks", ReasonCode: ShoppingReasonCodes.PetTooYoung });

        var (free, freeShown) = await PetAsync(specification: NoMinimumPetAge);
        var accepted = await ChooseAsync(free, freeShown, cat);
        var ageNotAsked = await ChooseAsync(free, freeShown, cat with { AnimalAgeWeeks = 30 });

        Assert.Equal(OfferReadiness.NeedsSelection, freeShown.OfferReadiness);
        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable), (accepted.Status, accepted.Candidate.OfferReadiness));
        Assert.Equal(SelectionEvaluationStatus.Rejected, ageNotAsked.Status);
        Assert.Contains(ageNotAsked.FieldIssues, issue => issue is { Field: "AnimalAgeWeeks", ReasonCode: ShoppingReasonCodes.SelectionFieldNotAllowed });
    }

    [Theory]
    [InlineData(BaggageTravelApplication.AllSectors)]
    [InlineData(BaggageTravelApplication.AtLeastOneSector)]
    [InlineData(BaggageTravelApplication.MostSignificantSector)]
    [InlineData(BaggageTravelApplication.AnySectorOnJourney)]
    public async Task BAG_R1_01_a_populated_travel_application_blocks_the_candidate_until_its_meaning_is_approved(BaggageTravelApplication travelApplication)
    {
        var (_, restricted) = await BagAsync("A01", Bag(travel: travelApplication));
        var (_, open) = await BagAsync("A01", Bag());

        Blocked(restricted.One(), ShoppingReasonCodes.BaggageTravelApplicationNotSupported, "BaggageApplication.TravelApplication");
        Assert.True(restricted.HasBlockedCandidates);
        Assert.Equal(OfferReadiness.Selectable, open.One().OfferReadiness);
    }

    [Theory]
    [InlineData(BaggageRuleDeference.MarketingCarrier)]
    [InlineData(BaggageRuleDeference.OperatingCarrier)]
    public async Task BAG_R1_02_a_populated_rule_deference_blocks_the_candidate_until_its_meaning_is_approved(BaggageRuleDeference deference)
    {
        var (_, restricted) = await BagAsync("A01", Bag(deference: deference));

        Blocked(restricted.One(), ShoppingReasonCodes.BaggageRuleDeferenceNotSupported, "BaggageApplication.RuleDeference");
    }

    [Fact]
    public async Task BAG_R1_03_a_rule_weight_matches_only_as_the_same_kilograms_as_the_specification()
    {
        var weightAllowance = Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] };
        var (_, samePiece) = await BagAsync("A01", Bag(weight: 23m));
        var (_, otherPiece) = await BagAsync("A01", Bag(weight: 32m));
        var (_, pounds) = await BagAsync("A01", Bag(weight: 23m, unit: WeightUnit.Lbs));
        var (_, unitOnly) = await BagAsync("A01", Bag(unit: WeightUnit.Lbs));
        var (_, samePackage) = await BagAsync("A02", Bag(BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 10m), weightAllowance);
        var (_, otherPackage) = await BagAsync("A02", Bag(BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 20m), weightAllowance);
        var (_, poundPackage) = await BagAsync("A02", Bag(BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 10m, WeightUnit.Lbs), weightAllowance);

        Assert.Equal(OfferReadiness.Selectable, samePiece.One().OfferReadiness);
        Assert.Equal(OfferReadiness.Selectable, unitOnly.One().OfferReadiness);
        Assert.Equal(OfferReadiness.Selectable, samePackage.One().OfferReadiness);

        foreach (var contradicting in new[] { otherPiece, pounds, otherPackage, poundPackage })
            Blocked(contradicting.One(), ShoppingReasonCodes.BaggageRuleWeightNotVerified, "BaggageApplication.Weight");
    }

    [Fact]
    public async Task BAG_R1_04_every_purchase_application_value_has_one_outcome_in_shopping()
    {
        var (_, prepaid) = await BagAsync("A01", Bag(purchase: BaggagePurchaseApplication.Prepaid));
        var (_, both) = await BagAsync("A01", Bag(purchase: BaggagePurchaseApplication.PrepaidAndCheckIn));
        var (_, checkIn) = await BagAsync("A01", Bag(purchase: BaggagePurchaseApplication.CheckIn));

        Assert.Equal(OfferReadiness.Selectable, prepaid.One().OfferReadiness);
        Assert.Equal(OfferReadiness.Selectable, both.One().OfferReadiness);
        Assert.Empty(checkIn.Candidates);
        Assert.Contains(checkIn.Diagnostics, diagnostic => diagnostic.Code == ShoppingReasonCodes.NoMatchingProvision);

        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag, sequence: 10, rules: Rules(baggage: Bag(purchase: BaggagePurchaseApplication.CheckIn))), Rate(70m));

        var online = lab.Provision(bag, sequence: 20, rules: Rules(baggage: Bag(purchase: BaggagePurchaseApplication.Prepaid)));

        lab.Price(bag, online, Rate(50m));

        var shown = (await lab.Engine.ShopAsync(Trip.Context())).One();

        Assert.Equal((online.Id, 50m), (shown.ProvisionId, shown.Price.CompleteUnitTotal));
    }

    [Fact]
    public async Task BAG_R1_05_the_allowance_concept_is_matched_on_verified_allowance_facts_and_a_contradicting_rule_cannot_be_published()
    {
        var pieceAllowance = Trip.Context();
        var weightAllowance = Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] };
        var partial = Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance() with { SourceCompleteness = FactEvidence.Partial }] };
        var none = Trip.Context() with { TravellerBaggageFacts = [] };
        var package = Bag(BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 10m);
        var (_, packageOnWeight) = await BagAsync("A02", package, weightAllowance);
        var (_, packageOnPiece) = await BagAsync("A02", package, pieceAllowance);
        var (_, packageOnPartial) = await BagAsync("A02", package, partial);
        var (_, packageOnNone) = await BagAsync("A02", package, none);
        var (_, pieceOnWeight) = await BagAsync("A01", Bag(), weightAllowance);
        var (_, overweightForWeight) = await BagAsync("A03", Bag(BaggageChargeKind.Overweight, BaggageAllowanceConcept.Weight), weightAllowance);
        var (_, overweightForPiece) = await BagAsync("A03", Bag(BaggageChargeKind.Overweight, BaggageAllowanceConcept.Weight), pieceAllowance);
        var (_, overweightForAny) = await BagAsync("A03", Bag(BaggageChargeKind.Overweight, null), none);

        Assert.Equal(OfferReadiness.Selectable, packageOnWeight.One().OfferReadiness);
        Assert.Empty(packageOnPiece.Candidates);
        Blocked(packageOnPartial.One(), ShoppingReasonCodes.BaggageAllowanceNotVerified, "TravellerBaggageFacts[T1,F1]");
        Blocked(packageOnNone.One(), ShoppingReasonCodes.BaggageAllowanceNotVerified, "TravellerBaggageFacts[T1,F1]");
        Assert.Empty(pieceOnWeight.Candidates);
        Assert.Equal(OfferReadiness.NeedsSelection, overweightForWeight.One().OfferReadiness);
        Assert.Empty(overweightForPiece.Candidates);
        Assert.Equal(OfferReadiness.NeedsSelection, overweightForAny.One().OfferReadiness);

        var otherKind = await Assert.ThrowsAsync<BusinessException>(() => BagAsync("A01", Bag(BaggageChargeKind.Oversize, null)));
        var otherConcept = await Assert.ThrowsAsync<BusinessException>(() => BagAsync("A02", Bag(BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Piece, 10m)));

        Assert.Equal(16321, otherKind.Code);
        Assert.InRange(otherConcept.Code, 16000, 16999);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(BaggageChargeCombination.Separate, false)]
    [InlineData(BaggageChargeCombination.Combined, true)]
    [InlineData(BaggageChargeCombination.MutuallyExclusive, true)]
    public async Task BAG_R1_06_a_combined_or_exclusive_charge_combination_blocks_the_candidate_without_the_other_charge_of_the_bag(BaggageChargeCombination? combination, bool blocked)
    {
        var lab = new ShoppingLab();
        var oversize = lab.Definition("A04", specification: input => input with { Baggage = input.Baggage! with { ChargeCombination = combination } });

        lab.Price(oversize, lab.Provision(oversize), Rate(100m));

        var shown = (await lab.Engine.ShopAsync(Trip.Context(), Everything)).One();

        if (blocked)
            Blocked(shown, ShoppingReasonCodes.BaggageChargeCombinationNotVerified, "Baggage.ChargeCombination");
        else
            Assert.Equal((OfferReadiness.NeedsSelection, EligibilityStatus.Eligible), (shown.OfferReadiness, shown.Eligibility.Status));
    }

    [Fact]
    public async Task TTL_R1_01_an_expired_source_context_is_never_selectable_or_accepted()
    {
        var lab = new ShoppingLab();

        lab.Sellable("A23");

        AncillaryShoppingContext Until(DateTimeOffset? validUntilUtc)
            => Trip.Context() with { SourceIdentity = new SourceIdentityContext { SourceKind = ShoppingSourceKind.Offer, TrustedSourceReference = "OFFER-1", ValidUntilUtc = validUntilUtc } };

        async Task<(CanonicalAncillaryOfferResult Result, CanonicalAncillarySelectionEvaluation Chosen)> AtAsync(DateTimeOffset? validUntilUtc)
        {
            var result = await lab.Engine.ShopAsync(Until(validUntilUtc));

            return (result, await lab.Engine.EvaluateSelectionAsync(Until(validUntilUtc), result.One().CandidateIdentity, new AncillarySelection { OptIn = true }));
        }

        var oneTickBefore = await AtAsync(Now.AddTicks(1));
        var unbounded = await AtAsync(null);
        var equal = await AtAsync(Now);
        var after = await AtAsync(Now.AddTicks(-1));

        Assert.Equal((OfferReadiness.Selectable, SelectionEvaluationStatus.Accepted, Now.AddTicks(1)), (oneTickBefore.Result.One().OfferReadiness, oneTickBefore.Chosen.Status, oneTickBefore.Chosen.Candidate.ExpiresAtUtc));
        Assert.False(oneTickBefore.Result.HasBlockedCandidates);
        Assert.Equal((OfferReadiness.Selectable, SelectionEvaluationStatus.Accepted), (unbounded.Result.One().OfferReadiness, unbounded.Chosen.Status));
        Assert.Null(unbounded.Chosen.Candidate.ExpiresAtUtc);
        Assert.DoesNotContain(ShoppingReasonCodes.SourceContextExpired, unbounded.Result.One().ReasonCodes);

        foreach (var expired in new[] { equal, after })
        {
            Assert.Equal((OfferReadiness.NeedsVerification, SelectionEvaluationStatus.Incomplete), (expired.Result.One().OfferReadiness, expired.Chosen.Status));
            Assert.Equal(OfferReadiness.NeedsVerification, expired.Chosen.Candidate.OfferReadiness);
            Assert.Contains(ShoppingReasonCodes.SourceContextExpired, expired.Result.One().ReasonCodes);
            Assert.Contains(ShoppingReasonCodes.SourceContextExpired, expired.Chosen.Candidate.ReasonCodes);
            Assert.True(expired.Result.HasBlockedCandidates);
            Assert.Contains(expired.Result.Diagnostics, diagnostic => diagnostic is { Code: ShoppingReasonCodes.SourceContextExpired, Severity: ShoppingDiagnosticSeverity.Blocker });
        }
    }

    [Fact]
    public async Task SLOT_R1_01_the_whole_occupancy_must_lie_in_slots_of_the_right_facility_and_airport()
    {
        const long facility = 7001;

        var start = new DateTimeOffset(2026, 12, 1, 5, 0, 0, TimeSpan.Zero);
        var first = new SlotInventorySource(facility, Trip.Ika, start, start.AddHours(1), InventoryRecordStatus.Active, false);
        var second = new SlotInventorySource(facility, Trip.Ika, start.AddHours(1), start.AddHours(2), InventoryRecordStatus.Active, false);
        var otherAirport = new SlotInventorySource(facility, Trip.Thr, start.AddHours(2), start.AddHours(4), InventoryRecordStatus.Active, false);
        var fromIka = Trip.Context([Trip.Flight(origin: Trip.Ika)]);

        async Task<CanonicalAncillarySelectionEvaluation> VisitAsync(
            DateTimeOffset at,
            IReadOnlyList<SlotInventorySource> slots,
            int guests = 0,
            int? quantity = null,
            int? facilityAirport = null)
        {
            var lab = new ShoppingLab { Catalog = { FacilityAnswer = InventoryReferenceCheck.Verified, FacilityAirportId = facilityAirport } };
            var definition = lab.Definition("A22");

            lab.Price(definition, lab.Provision(definition), Rate(100m));
            lab.Policy(definition, InventoryAuthority.Local, LocalInventoryPattern.AirportSlot, slot: AirportSlotConsumption.Create(facility, 120, 2), slotSources: slots);

            var shown = (await lab.Engine.ShopAsync(fromIka, Everything)).One();

            return await lab.Engine.EvaluateSelectionAsync(
                fromIka,
                shown.CandidateIdentity,
                new AncillarySelection { AirportId = Trip.Ika, FacilityRef = facility, TimeWithOffset = at, GuestCount = guests, Quantity = quantity });
        }

        void Unproven(CanonicalAncillarySelectionEvaluation visit, string reasonCode)
        {
            Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (visit.Status, visit.Candidate.OfferReadiness));
            Assert.Equal((InventoryCapacityReadState.Unknown, AvailabilityCheckState.Unknown, false), (visit.Candidate.Availability.ConfigState, visit.Candidate.Availability.CheckedState, visit.Candidate.Availability.IsGuaranteed));
            Assert.Contains(reasonCode, visit.Candidate.ReasonCodes);
        }

        var adjacent = await VisitAsync(start, [first, second, otherAirport], guests: 2);
        var endBoundary = await VisitAsync(start, [new SlotInventorySource(facility, Trip.Ika, start.AddHours(-1), start.AddHours(2), InventoryRecordStatus.Active, false)]);
        var overrun = await VisitAsync(start.AddMinutes(1), [first, second, otherAirport]);
        var startOnly = await VisitAsync(start, [first]);
        var gap = await VisitAsync(start, [first, second with { StartUtc = start.AddMinutes(90), EndUtc = start.AddHours(3) }]);
        var wrongAirport = await VisitAsync(start.AddHours(2), [first, second, otherAirport]);
        var draftPart = await VisitAsync(start, [first, second with { Status = InventoryRecordStatus.Draft }]);
        var closedPart = await VisitAsync(start, [first, second with { ClosedForSale = true }]);
        var facilityElsewhere = await VisitAsync(start, [first, second], facilityAirport: Trip.Thr);
        var facilityConfirmed = await VisitAsync(start, [first, second], facilityAirport: Trip.Ika);
        var manyUnits = await VisitAsync(start, [first, second], quantity: 2);

        foreach (var fitting in new[] { adjacent, endBoundary, facilityConfirmed })
        {
            Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable), (fitting.Status, fitting.Candidate.OfferReadiness));
            Assert.Equal(
                (InventoryCapacityReadState.ConfiguredNotGuaranteed, AvailabilityCheckState.NotChecked, false, true),
                (fitting.Candidate.Availability.ConfigState, fitting.Candidate.Availability.CheckedState, fitting.Candidate.Availability.IsGuaranteed, fitting.Candidate.Availability.RequiresCheckAtReserve));
        }

        Assert.Equal((120, 6), (adjacent.Candidate.Availability.ConsumptionPreview!.OccupancyMinutes, adjacent.Candidate.Availability.ConsumptionPreview.OccupiedPersons));
        Assert.Equal(2, endBoundary.Candidate.Availability.ConsumptionPreview!.OccupiedPersons);
        Unproven(overrun, ShoppingReasonCodes.AppointmentOutsideConfiguredSlots);
        Unproven(startOnly, ShoppingReasonCodes.AppointmentOutsideConfiguredSlots);
        Unproven(gap, ShoppingReasonCodes.AppointmentOutsideConfiguredSlots);
        Unproven(wrongAirport, ShoppingReasonCodes.AppointmentOutsideConfiguredSlots);
        Unproven(draftPart, ShoppingReasonCodes.AppointmentOutsideConfiguredSlots);
        Unproven(facilityElsewhere, ShoppingReasonCodes.FacilityNotVerified);
        Assert.Equal((SelectionEvaluationStatus.Rejected, OfferReadiness.Unavailable, InventoryCapacityReadState.ClosedForSale), (closedPart.Status, closedPart.Candidate.OfferReadiness, closedPart.Candidate.Availability.ConfigState));
        Assert.Equal(SelectionEvaluationStatus.Rejected, manyUnits.Status);
        Assert.Contains(manyUnits.FieldIssues, issue => issue is { Field: "Quantity", ReasonCode: ShoppingReasonCodes.SelectionFieldNotAllowed });
    }

    [Fact]
    public async Task PRICE_R1_01_a_ticket_fee_leaves_the_unit_total_and_removes_the_selected_total()
    {
        var lab = new ShoppingLab();
        var meal = lab.Definition("A12");

        lab.Price(
            meal,
            lab.Provision(meal),
            Rate(100m, components: [Tax("VAT", 9m, TaxTreatment.AddedToBase), Tax("INC", 5m, TaxTreatment.IncludedInBase), Fee("TKT", 7m, FeeApplicationUnit.Ticket)]));

        var outbound = Trip.Flight("F1", 101, Trip.Thr, Trip.Ika);
        var onward = Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, outbound.DepartureAt.AddHours(5));
        var back = Trip.Flight("F3", 103, Trip.Ist, Trip.Mhd, outbound.DepartureAt.AddDays(5), Trip.Istanbul);
        var context = Trip.Context([outbound, onward, back], [Trip.Portion("P1", 1, outbound, onward), Trip.Portion("P2", 2, back)], [Trip.Adult("T1"), Trip.Adult("T2")]);
        var result = await lab.Engine.ShopAsync(context);

        Assert.Equal(6, result.Candidates.Count);
        Assert.All(result.Candidates, candidate =>
        {
            var price = candidate.Price;

            Assert.Equal((PriceAssessmentStatus.Complete, 100m, 109m, false), (price.Status, price.BaseAmount, price.CompleteUnitTotal, price.IsOrderLevelTotalComplete));
            Assert.Equal(("INC", 5m), (price.IncludedTaxLines.Single().Code, price.IncludedTaxLines.Single().Amount));
            Assert.Equal(("TKT", 7m, FeeApplicationUnit.Ticket), (price.UnappliedFeeLines.Single().Code, price.UnappliedFeeLines.Single().Amount, price.UnappliedFeeLines.Single().FeeApplicationUnit));
            Assert.Empty(price.AppliedUnitFeeLines);
            Assert.Null(price.RequestedQuantityTotal);
        });

        foreach (var quantity in new[] { 1, 2 })
        {
            var chosen = await lab.Engine.EvaluateSelectionAsync(context, result.Candidates[0].CandidateIdentity, new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = quantity });

            Assert.Equal((109m, 7m, false), (chosen.Candidate.Price.CompleteUnitTotal, chosen.Candidate.Price.UnappliedFeeLines.Single().Amount, chosen.Candidate.Price.IsOrderLevelTotalComplete));
            Assert.Null(chosen.Candidate.Price.RequestedQuantityTotal);
            Assert.Contains(ShoppingReasonCodes.FeeNotAppliedAtUnitLevel, chosen.Candidate.ReasonCodes);
        }

        var itemFeeOnly = new ShoppingLab();
        var plain = itemFeeOnly.Definition("A12");

        itemFeeOnly.Price(plain, itemFeeOnly.Provision(plain), Rate(100m, components: [Tax("VAT", 9m, TaxTreatment.AddedToBase), Tax("INC", 5m, TaxTreatment.IncludedInBase)]));

        var complete = await itemFeeOnly.Engine.EvaluateSelectionAsync(
            Trip.Context(),
            (await itemFeeOnly.Engine.ShopAsync(Trip.Context())).One().CandidateIdentity,
            new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = 2 });

        Assert.Equal((109m, 218m, true), (complete.Candidate.Price.CompleteUnitTotal, complete.Candidate.Price.RequestedQuantityTotal, complete.Candidate.Price.IsOrderLevelTotalComplete));
    }

    [Fact]
    public async Task SEAT_R1_01_a_seat_characteristic_is_proven_only_by_a_verified_seat_fact_of_the_context()
    {
        var lab = new ShoppingLab();

        lab.Sellable("A08");

        var shown = (await lab.Engine.ShopAsync(Trip.Context())).One();
        var seat = new AncillarySelection { SeatNumber = "12A", ExitRowTermsAccepted = true };

        Task<CanonicalAncillarySelectionEvaluation> ChooseAsync(params SeatCharacteristicFacts[] facts)
            => lab.Engine.EvaluateSelectionAsync(Trip.Context() with { SeatCharacteristicFacts = facts }, shown.CandidateIdentity, seat);

        void Unproven(CanonicalAncillarySelectionEvaluation evaluation)
        {
            Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (evaluation.Status, evaluation.Candidate.OfferReadiness));
            Assert.Contains(ShoppingReasonCodes.SeatMapNotVerified, evaluation.Candidate.ReasonCodes);
            Assert.Contains("SeatCharacteristicFacts[F1,12A]", evaluation.Candidate.Eligibility.MissingContextFields);
        }

        var proven = await ChooseAsync(Trip.Seat("12a", codes: ["W", "E"]));
        var otherCharacteristic = await ChooseAsync(Trip.Seat("12A", codes: ["W"]));
        var noCharacteristic = await ChooseAsync(Trip.Seat("12A"));

        Assert.Null(typeof(AncillarySelection).GetProperty("VerifiedSeatCharacteristicCodes"));
        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable, false), (proven.Status, proven.Candidate.OfferReadiness, proven.Candidate.Availability.IsGuaranteed));
        Unproven(await ChooseAsync());
        Unproven(await ChooseAsync(Trip.Seat("12B", codes: ["E"])));
        Unproven(await ChooseAsync(Trip.Seat("12A", evidence: FactEvidence.Partial, codes: ["E"])));
        Unproven(await ChooseAsync(Trip.Seat("12A", evidence: FactEvidence.Missing, codes: ["E"])));

        foreach (var disproven in new[] { otherCharacteristic, noCharacteristic })
        {
            Assert.Equal(SelectionEvaluationStatus.Rejected, disproven.Status);
            Assert.Contains(disproven.FieldIssues, issue => issue is { Field: "SeatNumber", ReasonCode: ShoppingReasonCodes.SeatCharacteristicNotAllowed });
        }

        var duplicate = await Assert.ThrowsAsync<BusinessException>(() => ChooseAsync(Trip.Seat("12A", codes: ["E"]), Trip.Seat("12a", codes: ["W"])));
        var foreignFlight = await Assert.ThrowsAsync<BusinessException>(() => ChooseAsync(Trip.Seat("12A", "F9", codes: ["E"])));

        Assert.Equal((16801, 16801), (duplicate.Code, foreignFlight.Code));
    }

    [Fact]
    public async Task SEAT_R1_02_typed_fields_of_another_profile_are_rejected()
    {
        var lab = new ShoppingLab();
        var pet = lab.Definition("A13", specification: NoMinimumPetAge);
        var meal = lab.Definition("A12");

        lab.Price(pet, lab.Provision(pet), Rate(90m));
        lab.Price(meal, lab.Provision(meal), Rate(9m));

        var result = await lab.Engine.ShopAsync(Trip.Context());
        var cat = new AncillarySelection { AnimalType = PetAnimalType.Cat, CombinedWeightKg = 5m, CarrierDimensions = new(50m, 35m, 20m), DocumentAcknowledgements = ["ENTRY_RULES_ACK"] };
        var pasta = new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = 1 };

        async Task RejectedAsync(string variantCode, AncillarySelection selection, string field)
        {
            var evaluation = await lab.Engine.EvaluateSelectionAsync(Trip.Context(), result.Candidates.Single(candidate => candidate.VariantCode == variantCode).CandidateIdentity, selection);

            Assert.Equal(SelectionEvaluationStatus.Rejected, evaluation.Status);
            Assert.Contains(evaluation.FieldIssues, issue => issue.Field == field && issue.ReasonCode == ShoppingReasonCodes.SelectionFieldNotAllowed);
        }

        await RejectedAsync("A12", pasta with { AnimalAgeWeeks = 20 }, "AnimalAgeWeeks");
        await RejectedAsync("A12", pasta with { OtherAnimalCode = "RABBIT" }, "OtherAnimalCode");
        await RejectedAsync("A12", pasta with { AnimalType = PetAnimalType.RegisteredOther, OtherAnimalCode = "RABBIT" }, "OtherAnimalCode");
        await RejectedAsync("A12", pasta with { SeatNumber = "12A" }, "SeatNumber");
        await RejectedAsync("A13", cat with { OtherAnimalCode = "RABBIT" }, "OtherAnimalCode");
        await RejectedAsync("A13", cat with { AnimalAgeWeeks = 20 }, "AnimalAgeWeeks");
        await RejectedAsync("A13", cat with { SeatNumber = "12A" }, "SeatNumber");
    }
}
