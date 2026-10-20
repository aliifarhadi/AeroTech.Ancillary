using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2PricingCorrectnessTests
{
    private static readonly ShoppingFilter Everything = new() { IncludeNonSellable = true };

    [Fact]
    public async Task PRICE_01_quantity_two_doubles_the_base_the_added_tax_and_the_item_fee_but_never_the_included_tax()
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");

        lab.Price(
            bag,
            lab.Provision(bag),
            Rate(100m, components: [Tax("VAT", 10m, TaxTreatment.AddedToBase), Tax("INC", 5m, TaxTreatment.IncludedInBase), Fee("SVC", 2m, FeeApplicationUnit.Item)]));

        var context = Trip.Context();
        var listed = (await lab.Engine.ShopAsync(context)).One();
        var one = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection { Quantity = 1 });
        var two = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection { Quantity = 2 });
        var zero = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection { Quantity = 0 });

        Assert.Equal((112m, (int?)null, (decimal?)null), (listed.Price.CompleteUnitTotal, listed.Price.Units, listed.Price.RequestedQuantityTotal));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 1, 112m), (one.Status, one.Candidate.Price.Units, one.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 2, 224m, true), (two.Status, two.Candidate.Price.Units, two.Candidate.Price.RequestedQuantityTotal, two.Candidate.Price.IsOrderLevelTotalComplete));
        Assert.Equal((SelectionEvaluationStatus.Rejected, (decimal?)null), (zero.Status, zero.Candidate.Price.RequestedQuantityTotal));
    }

    [Theory]
    [InlineData("A24", 30)]
    [InlineData("A20", 40)]
    public async Task PRICE_04_a_typed_form_service_is_one_commercial_unit_and_its_line_total_is_its_unit_total(string code, int amount)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition), Rate(amount));

        var context = Trip.Context();
        var listed = (await lab.Engine.ShopAsync(context, Everything)).One();
        var selection = code == "A24"
            ? new AncillarySelection { PlanCode = definition.ServiceDefinitionRef, DeviceCount = 1 }
            : new AncillarySelection { AirportId = Trip.Thr };
        var chosen = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, selection);

        Assert.Equal(((int?)null, (decimal?)amount, (decimal?)null), (listed.Price.Units, listed.Price.CompleteUnitTotal, listed.Price.RequestedQuantityTotal));
        Assert.Equal((SelectionEvaluationStatus.Accepted, OfferReadiness.Selectable, 1, (decimal?)amount), (chosen.Status, chosen.Candidate.OfferReadiness, chosen.Candidate.Price.Units, chosen.Candidate.Price.RequestedQuantityTotal));
    }

    [Fact]
    public async Task PRICE_06_a_per_kilogram_product_without_a_chargeable_weight_basis_has_no_line_total()
    {
        var lab = new ShoppingLab();
        var overweight = lab.Definition("A03", pricingUnit: PricingUnit.PerKilogram);

        lab.Price(overweight, lab.Provision(overweight, quantityUnit: AncillaryQuantityUnit.Kilogram), Rate(8m));

        var context = Trip.Context();
        var listed = (await lab.Engine.ShopAsync(context, Everything)).One();
        var chosen = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 28m });

        Assert.Contains(ShoppingReasonCodes.ChargeableWeightNotDetermined, listed.Price.ReasonCodes);
        Assert.Equal((false, (decimal?)null), (listed.Price.IsOrderLevelTotalComplete, listed.Price.RequestedQuantityTotal));
        Assert.Equal((false, (decimal?)null), (chosen.Candidate.Price.IsOrderLevelTotalComplete, chosen.Candidate.Price.RequestedQuantityTotal));
    }

    [Fact]
    public async Task PRICE_15_an_explicitly_free_meal_is_a_zero_total_in_the_offer_currency_and_a_case_changed_code_is_rejected()
    {
        var lab = new ShoppingLab();
        var meal = lab.Definition("A11");

        lab.Provision(meal);

        var context = Trip.Context();
        var listed = (await lab.Engine.ShopAsync(context, Everything)).One();
        var accepted = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection { MealCode = "VGML" });
        var changed = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, new AncillarySelection { MealCode = "vgml" });

        Assert.Equal((PriceOrigin.Free, PriceAssessmentStatus.Complete, (int?)Eur, (decimal?)0m, (decimal?)0m), (listed.Price.Origin, listed.Price.Status, listed.Price.CurrencyId, listed.Price.BaseAmount, listed.Price.CompleteUnitTotal));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 1, (decimal?)0m), (accepted.Status, accepted.Candidate.Price.Units, accepted.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal(SelectionEvaluationStatus.Rejected, changed.Status);
        Assert.Contains(changed.FieldIssues, issue => issue is { Field: "MealCode", ReasonCode: ShoppingReasonCodes.MealCodeMismatch });
    }

    [Fact]
    public async Task FLOW_07_an_order_scoped_service_with_a_per_passenger_rate_is_never_priced_from_an_invented_traveller()
    {
        var lab = new ShoppingLab();
        var cip = lab.Definition("A22");

        lab.Price(cip, lab.Provision(cip, scope: ServiceCoverageScope.Order), Rate(100m));

        var context = Trip.Context(flights: [Trip.Flight(origin: Trip.Ika)], travellers: [Trip.Adult("T1"), Trip.Child("T2")]);
        var order = (await lab.Engine.ShopAsync(context, Everything)).Candidates.Single(candidate => candidate.CandidateIdentity.CoverageScope == ServiceCoverageScope.Order);

        Assert.Equal((PriceAssessmentStatus.Incomplete, (decimal?)null), (order.Price.Status, order.Price.RequestedQuantityTotal));
        Assert.Contains(ShoppingReasonCodes.BeneficiaryRequired, order.Price.ReasonCodes);
        Assert.Empty(order.Travellers);
    }

    [Theory]
    [InlineData("A02", " WEIGHT_PACK_10", "PackageProductRef", ShoppingReasonCodes.SelectedOptionNotOffered)]
    [InlineData("A02", "weight_pack_10", "PackageProductRef", ShoppingReasonCodes.SelectedOptionNotOffered)]
    [InlineData("A24", "WIFI_FULL_FLIGHT ", "PlanCode", ShoppingReasonCodes.SelectedOptionNotOffered)]
    [InlineData("A12", "menu_pasta_01", "MenuItemRef", ShoppingReasonCodes.MenuItemMismatch)]
    public async Task SEC_13_a_case_or_whitespace_changed_authored_choice_is_rejected(string code, string value, string field, string reason)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition), Rate(20m));

        var context = code == "A02" ? Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] } : Trip.Context();
        var listed = (await lab.Engine.ShopAsync(context, Everything)).Candidates.First(candidate => candidate.VariantCode == code);
        var selection = code switch
        {
            "A02" => new AncillarySelection { PackageProductRef = value, Quantity = 1 },
            "A24" => new AncillarySelection { PlanCode = value, DeviceCount = 1 },
            _ => new AncillarySelection { MenuItemRef = value, Quantity = 1 }
        };
        var chosen = await lab.Engine.EvaluateSelectionAsync(context, listed.CandidateIdentity, selection);

        Assert.Equal(SelectionEvaluationStatus.Rejected, chosen.Status);
        Assert.Contains(chosen.FieldIssues, issue => issue.Field == field && issue.ReasonCode == reason);
        Assert.Null(chosen.Candidate.Price.RequestedQuantityTotal);
    }

    [Fact]
    public async Task S2_A13_a_pet_subject_to_confirmation_is_checked_at_reserve_without_claiming_an_availability_check()
    {
        var lab = new ShoppingLab();
        var pet = lab.Definition("A13");

        lab.Price(pet, lab.Provision(pet), Rate(50m));

        var listed = (await lab.Engine.ShopAsync(Trip.Context(), Everything)).One();

        Assert.True(listed.Availability.RequiresCheckAtReserve);
        Assert.DoesNotContain(ShoppingReasonCodes.AvailabilityMustBeChecked, listed.Availability.ReasonCodes);
    }

    [Fact]
    public async Task S2_A18_a_bassinet_needs_the_real_infant_and_guardian_pair_a_recorded_link_and_the_infant_age()
    {
        var lab = new ShoppingLab();
        var bassinet = lab.Definition("A18");

        lab.Provision(bassinet);

        var linked = Trip.Context(travellers: [Trip.Adult("T1"), Trip.Adult("T2"), Infant("T3", "T1")]);
        var unlinked = Trip.Context(travellers: [Trip.Adult("T1"), Infant("T3", null)]);
        var undated = Trip.Context(travellers: [Trip.Adult("T1"), Infant("T3", "T1") with { DateOfBirth = null }]);

        async Task<CanonicalAncillarySelectionEvaluation> ChooseAsync(AncillaryShoppingContext context, string guardian)
        {
            var shown = (await lab.Engine.ShopAsync(context, Everything)).Candidates.Single(candidate => candidate.CandidateIdentity.TravellerRef == guardian);

            return await lab.Engine.EvaluateSelectionAsync(context, shown.CandidateIdentity, new AncillarySelection { InfantRef = "T3", GuardianRef = guardian });
        }

        var accepted = await ChooseAsync(linked, "T1");
        var otherAdult = await ChooseAsync(linked, "T2");
        var noLink = await ChooseAsync(unlinked, "T1");
        var noAge = await ChooseAsync(undated, "T1");

        Assert.Equal(SelectionEvaluationStatus.Accepted, accepted.Status);
        Assert.Equal(SelectionEvaluationStatus.Rejected, otherAdult.Status);
        Assert.Contains(otherAdult.FieldIssues, issue => issue is { Field: "GuardianRef", ReasonCode: ShoppingReasonCodes.InfantGuardianRequired });
        Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (noLink.Status, noLink.Candidate.OfferReadiness));
        Assert.Contains("Travellers[T3].AssociatedAdultRef", noLink.Candidate.Eligibility.MissingContextFields);
        Assert.Equal((SelectionEvaluationStatus.Incomplete, OfferReadiness.NeedsVerification), (noAge.Status, noAge.Candidate.OfferReadiness));
        Assert.Contains("Travellers[T3].DateOfBirth", noAge.Candidate.Eligibility.MissingContextFields);
    }

    private static ShoppingTraveller Infant(string reference, string? guardian)
        => new()
        {
            TravellerRef = reference,
            PassengerTypeCode = PassengerTypeCode.INF,
            DateOfBirth = new DateOnly(2026, 6, 1),
            AssociatedAdultRef = guardian,
            StableTravellerIdentity = $"PAX-{reference}"
        };
}
