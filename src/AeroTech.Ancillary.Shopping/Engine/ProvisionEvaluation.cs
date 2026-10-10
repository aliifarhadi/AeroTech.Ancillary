using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed partial class ProvisionEvaluation
    {
        private readonly AncillaryServiceDefinition _definition;
        private readonly AncillaryProvision _provision;
        private readonly EvaluationUnit _unit;

        public ProvisionEvaluation(AncillaryServiceDefinition definition, AncillaryProvision provision, EvaluationUnit unit)
        {
            _definition = definition;
            _provision = provision;
            _unit = unit;
            Timeline = new UnitTimeline(definition, unit);
        }

        public RuleOutcome Outcome { get; } = new();

        public UnitTimeline Timeline { get; }

        private AncillaryShoppingContext Context => _unit.Context;

        private AncillarySelection? Selection => _unit.Selection;

        public RuleOutcome Run()
        {
            Stage();
            PassengerEligibility();
            SalesRestrictions();
            Geography();
            FlightApplication();
            FareApplication();
            TravelDate();
            DayTimeApplication();
            AdvancePurchase();
            BaggageApplication();
            SeatApplication();
            ProfileRules();
            SelectionContract();

            return Outcome;
        }

        private void Stage()
        {
            var applies = Context.ShoppingStage switch
            {
                ShoppingStage.PreOrder => _provision.PurchaseStage is PurchaseStage.PreOrder or PurchaseStage.Both,
                ShoppingStage.PostTicketed => _provision.PurchaseStage is PurchaseStage.PostTicketed or PurchaseStage.Both,
                ShoppingStage.OnBoard => _provision.PurchaseStage == PurchaseStage.OnBoard,
                _ => false
            };

            if (!applies)
                Outcome.Fail(ShoppingReasonCodes.StageNotApplicable);
        }

        private IReadOnlyList<ServiceOccurrence>? Occurrences()
        {
            if (Timeline.Occurrences is not null)
                return Timeline.Occurrences;

            if (Timeline.OccurrencePendingSelection)
                Outcome.AwaitSelection(Timeline.OccurrenceReason!, Timeline.OccurrenceField!);
            else
                Outcome.Unknown(Timeline.OccurrenceReason!, Timeline.OccurrenceField!);

            return null;
        }

        private IReadOnlyList<int>? Ages()
        {
            if (Occurrences() is null)
                return null;

            if (Timeline.Ages is null)
                Outcome.Unknown(ShoppingReasonCodes.AgeNotVerified, $"Travellers[{_unit.Traveller.TravellerRef}].DateOfBirth");

            return Timeline.Ages;
        }

        private void Allow<TValue>(IEnumerable<TValue> allowed, TValue? actual, string unknownCode, string failCode, string field)
            where TValue : struct
        {
            var values = allowed.ToList();

            if (values.Count == 0)
                return;

            if (actual is null)
                Outcome.Unknown(unknownCode, field);
            else if (!values.Contains(actual.Value))
                Outcome.Fail(failCode);
        }

        private void AllowText(IEnumerable<string> allowed, string? actual, string unknownCode, string failCode, string field)
        {
            var values = allowed.ToList();

            if (values.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(actual))
                Outcome.Unknown(unknownCode, field);
            else if (!values.Contains(actual.Trim().ToUpperInvariant(), StringComparer.Ordinal))
                Outcome.Fail(failCode);
        }
    }
}
