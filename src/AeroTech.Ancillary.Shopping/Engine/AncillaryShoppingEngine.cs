using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    public sealed class AncillaryShoppingEngine : IAncillaryShoppingEngine
    {
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryPricingReader _pricings;
        private readonly IInventoryConfigurationReader _inventory;
        private readonly ICurrencyReference _currencies;
        private readonly IInventoryResourceReference _resources;
        private readonly IAirportFacilityReference _facilities;
        private readonly ICountingFamilyReference _families;
        private readonly IFlightFlowDelegationReference _flightFlow;

        public AncillaryShoppingEngine(
            IActiveAncillaryDefinitionReader definitions,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryPricingReader pricings,
            IInventoryConfigurationReader inventory,
            ICurrencyReference currencies,
            IInventoryResourceReference resources,
            IAirportFacilityReference facilities,
            ICountingFamilyReference families,
            IFlightFlowDelegationReference flightFlow)
        {
            _definitions = definitions;
            _provisions = provisions;
            _pricings = pricings;
            _inventory = inventory;
            _currencies = currencies;
            _resources = resources;
            _facilities = facilities;
            _families = families;
            _flightFlow = flightFlow;
        }

        public async Task<CanonicalAncillaryOfferResult> ShopAsync(
            AncillaryShoppingContext context,
            ShoppingFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            filter ??= ShoppingFilter.None;
            ShoppingContextValidator.Validate(context, filter);

            var catalog = await LoadAsync(context, definition => Selected(definition, filter), cancellationToken);
            var candidates = new List<CanonicalAncillaryOfferCandidate>();
            var diagnostics = new List<ShoppingDiagnostic>();
            var travellers = context.Travellers
                .Where(traveller => filter.TravellerRefs is null || filter.TravellerRefs.Contains(traveller.TravellerRef, StringComparer.Ordinal))
                .ToList();

            foreach (var definition in catalog.Definitions)
            {
                var provisions = catalog.ProvisionsOf(definition.Id);

                foreach (var traveller in travellers)
                {
                    foreach (var scope in provisions.Select(provision => provision.CoverageScope).Distinct().Order())
                    {
                        var scoped = provisions.Where(provision => provision.CoverageScope == scope).ToList();
                        var units = CoveragePlanner.Units(context, traveller, scope)
                            .Where(unit => filter.FlightRefs is null || unit.Flights.Any(flight => filter.FlightRefs.Contains(flight.FlightRef, StringComparer.Ordinal)));

                        foreach (var unit in units)
                        {
                            var decision = await DecideAsync(catalog, definition, scoped, unit, null, cancellationToken);

                            if (decision is null)
                            {
                                if (filter.IncludeNonSellable)
                                    diagnostics.Add(new ShoppingDiagnostic(
                                        ShoppingReasonCodes.NoMatchingProvision,
                                        ShoppingDiagnosticSeverity.Info,
                                        null,
                                        [definition.ServiceDefinitionRef, traveller.TravellerRef, unit.CoverageRef],
                                        null,
                                        ShoppingEvidenceKind.Source));

                                continue;
                            }

                            if (decision.Candidate.OfferReadiness is OfferReadiness.NotEligible or OfferReadiness.Unavailable && !filter.IncludeNonSellable)
                                continue;

                            candidates.Add(decision.Candidate);
                            diagnostics.AddRange(Diagnostics(decision.Candidate));
                        }
                    }
                }
            }

            return new CanonicalAncillaryOfferResult(
                context.ContextSchemaVersion,
                context.EvaluatedAtUtc,
                context.OwnerAirlineId,
                context.PointOfSaleId,
                context.CurrencyId,
                context.SourceIdentity.TrustedSourceVersion,
                candidates,
                diagnostics,
                diagnostics.Any(diagnostic => diagnostic.Severity == ShoppingDiagnosticSeverity.Blocker));
        }

        public async Task<CanonicalAncillarySelectionEvaluation> EvaluateSelectionAsync(
            AncillaryShoppingContext context,
            CandidateIdentity candidate,
            AncillarySelection selection,
            CancellationToken cancellationToken = default)
        {
            ShoppingContextValidator.Validate(context, ShoppingFilter.None);

            var catalog = await LoadAsync(context, definition => definition.Id == candidate.DefinitionVersionId, cancellationToken);
            var definition = catalog.Definitions.SingleOrDefault() ?? throw ExceptionFactory.ShoppingCandidateNotFound();
            var traveller = context.Travellers.FirstOrDefault(row => row.TravellerRef == candidate.TravellerRef) ?? throw ExceptionFactory.ShoppingCandidateNotFound();
            var unit = CoveragePlanner.Units(context, traveller, candidate.CoverageScope).FirstOrDefault(row => row.CoverageRef == candidate.CoverageRef)
                       ?? throw ExceptionFactory.ShoppingCandidateNotFound();
            var provisions = catalog.ProvisionsOf(definition.Id).Where(provision => provision.CoverageScope == candidate.CoverageScope).ToList();

            if (provisions.All(provision => provision.Id != candidate.ProvisionId))
                throw ExceptionFactory.ShoppingCandidateNotFound();

            var decision = (await DecideAsync(catalog, definition, provisions, unit with { Selection = selection }, candidate.ProvisionId, cancellationToken))!;
            var changed = decision.Candidate.ProvisionId != candidate.ProvisionId;
            var status = decision.Candidate.OfferReadiness == OfferReadiness.NotEligible
                ? SelectionEvaluationStatus.Rejected
                : decision.Outcome.OnlySelectionPending ? SelectionEvaluationStatus.Incomplete : SelectionEvaluationStatus.Accepted;

            return new CanonicalAncillarySelectionEvaluation(
                status,
                changed
                    ? decision.Candidate with { ReasonCodes = decision.Candidate.ReasonCodes.Append(ShoppingReasonCodes.ProvisionChangedBySelection).ToList() }
                    : decision.Candidate,
                changed,
                decision.Outcome.FieldIssues);
        }

        private static bool Selected(AncillaryServiceDefinition definition, ShoppingFilter filter)
            => definition.IsClassified
               && (filter.Profiles is null || filter.Profiles.Contains(definition.Profile!.Value))
               && (filter.VariantCodes is null || filter.VariantCodes.Contains(definition.VariantCode, StringComparer.Ordinal))
               && (filter.ServiceDefinitionRefs is null || filter.ServiceDefinitionRefs.Contains(definition.ServiceDefinitionRef, StringComparer.Ordinal));

        private static IEnumerable<ShoppingDiagnostic> Diagnostics(CanonicalAncillaryOfferCandidate candidate)
        {
            if (candidate.Eligibility.Status == EligibilityStatus.InsufficientContext && candidate.OfferReadiness == OfferReadiness.NeedsVerification)
            {
                foreach (var code in candidate.Eligibility.ReasonCodes)
                    yield return new ShoppingDiagnostic(
                        code,
                        ShoppingDiagnosticSeverity.Blocker,
                        candidate.CandidateIdentity,
                        candidate.Eligibility.MissingContextFields,
                        null,
                        ShoppingEvidenceKind.Source);
            }

            if (candidate.Price.Status == PriceAssessmentStatus.Incomplete)
            {
                foreach (var code in candidate.Price.ReasonCodes)
                    yield return new ShoppingDiagnostic(code, ShoppingDiagnosticSeverity.Blocker, candidate.CandidateIdentity, [], nameof(ICurrencyReference), ShoppingEvidenceKind.Source);
            }

            if (candidate.Availability.ReasonCodes.Contains(ShoppingReasonCodes.BlockedExternalReference)
                || candidate.Quantity.ReasonCodes.Contains(ShoppingReasonCodes.CountingFamilyNotVerified))
                yield return new ShoppingDiagnostic(
                    ShoppingReasonCodes.BlockedExternalReference,
                    candidate.OfferReadiness == OfferReadiness.NeedsVerification ? ShoppingDiagnosticSeverity.Blocker : ShoppingDiagnosticSeverity.Warning,
                    candidate.CandidateIdentity,
                    [],
                    candidate.Availability.InventoryAuthority?.ToString(),
                    ShoppingEvidenceKind.Blocked);
        }

        private static OfferReadiness Readiness(
            AncillaryProvision provision,
            RuleOutcome outcome,
            PriceAssessment price,
            AvailabilityAssessment availability,
            QuantityAssessment quantity)
        {
            if (outcome.Verdict == RuleVerdict.NoMatch || quantity.ReasonCodes.Contains(ShoppingReasonCodes.QuantityExceedsVerifiedRemaining))
                return OfferReadiness.NotEligible;

            if (provision.Outcome.Disposition == CommercialDisposition.NotAvailable
                || price.Status == PriceAssessmentStatus.Unavailable
                || availability.ConfigState == InventoryCapacityReadState.ClosedForSale)
                return OfferReadiness.Unavailable;

            if ((outcome.Verdict == RuleVerdict.Insufficient && !outcome.OnlySelectionPending) || price.Status == PriceAssessmentStatus.Incomplete)
                return OfferReadiness.NeedsVerification;

            if (outcome.OnlySelectionPending)
                return OfferReadiness.NeedsSelection;

            if (price.Status == PriceAssessmentStatus.QuoteRequired)
                return OfferReadiness.NeedsQuote;

            var unverifiable = availability.CheckedState is AvailabilityCheckState.SourceUnavailable or AvailabilityCheckState.Unknown
                               || (provision.Availability.MustCheckAvailability
                                   && availability.ConfigState is InventoryCapacityReadState.NotConfigured
                                       or InventoryCapacityReadState.Unknown
                                       or InventoryCapacityReadState.UnsupportedPattern);

            return unverifiable ? OfferReadiness.NeedsVerification : OfferReadiness.Selectable;
        }

        private async Task<ShoppingCatalog> LoadAsync(AncillaryShoppingContext context, Func<AncillaryServiceDefinition, bool> selected, CancellationToken cancellationToken)
        {
            var definitions = (await _definitions.ListActiveAsync(context.OwnerAirlineId, cancellationToken))
                .Where(definition => definition.OwnerAirlineId == context.OwnerAirlineId && definition.Status == ServiceDefinitionStatus.Active && selected(definition))
                .OrderBy(definition => definition.ServiceDefinitionRef, StringComparer.Ordinal)
                .ThenBy(definition => definition.Id)
                .ToList();
            var definitionIds = definitions.Select(definition => definition.Id).ToList();
            var provisions = (await _provisions.ListActiveAsync(definitionIds, context.PointOfSaleId, cancellationToken))
                .Where(provision => provision.Status == ProvisionStatus.Active
                                    && definitionIds.Contains(provision.ServiceDefinitionId)
                                    && provision.SalesRestrictions is { PointsOfSale.Count: 1 } sales
                                    && sales.PointsOfSale.Single().PointOfSaleId == context.PointOfSaleId)
                .ToList();
            var filed = provisions.Where(provision => provision.PriceOrigin == PriceOrigin.Filed).Select(provision => provision.Id).ToList();
            var pricings = (await _pricings.ListActiveAsync(filed, cancellationToken))
                .Where(pricing => pricing.Status == PricingStatus.Active && filed.Contains(pricing.AncillaryProvisionId))
                .ToList();
            var inventory = await _inventory.ListAsync(
                context.OwnerAirlineId,
                definitions.Select(definition => definition.ServiceDefinitionRef).Distinct(StringComparer.Ordinal).ToList(),
                context.Flights.Select(flight => flight.FlightId).Distinct().ToList(),
                context.EvaluatedAtUtc,
                cancellationToken);
            var decimals = await _currencies.FindDecimalPlacesAsync([context.CurrencyId], cancellationToken);

            return new ShoppingCatalog(
                definitions,
                provisions,
                pricings,
                inventory.Where(configuration => configuration.Policy.OwnerAirlineId == context.OwnerAirlineId),
                decimals,
                new InventoryReferenceProbe(_resources, _facilities, _families, _flightFlow, context.OwnerAirlineId));
        }

        private async Task<Decision?> DecideAsync(
            ShoppingCatalog catalog,
            AncillaryServiceDefinition definition,
            IReadOnlyList<AncillaryProvision> provisions,
            EvaluationUnit unit,
            long? fallbackProvisionId,
            CancellationToken cancellationToken)
        {
            ProvisionEvaluation? fallback = null;
            AncillaryProvision? rejected = null;

            foreach (var provision in provisions)
            {
                var evaluation = new ProvisionEvaluation(definition, provision, unit);

                if (evaluation.Run().Verdict != RuleVerdict.NoMatch)
                    return await BuildAsync(catalog, definition, provision, unit, evaluation, cancellationToken);

                if (provision.Id == fallbackProvisionId)
                {
                    fallback = evaluation;
                    rejected = provision;
                }
            }

            return fallback is null ? null : await BuildAsync(catalog, definition, rejected!, unit, fallback, cancellationToken);
        }

        private async Task<Decision> BuildAsync(
            ShoppingCatalog catalog,
            AncillaryServiceDefinition definition,
            AncillaryProvision provision,
            EvaluationUnit unit,
            ProvisionEvaluation evaluation,
            CancellationToken cancellationToken)
        {
            var outcome = evaluation.Outcome;
            var configuration = catalog.InventoryOf(definition.ServiceDefinitionRef);
            var price = PriceAssessor.Assess(provision, catalog.PricingOf(provision.Id), unit, evaluation.Timeline, catalog.CurrencyDecimalPlaces);
            var availability = await AvailabilityAssessor.AssessAsync(provision, configuration, unit, catalog.References, cancellationToken);
            var quantity = await QuantityAssessor.AssessAsync(definition, provision, configuration?.Policy, unit, evaluation.Timeline, catalog.References, cancellationToken);
            var identity = new CandidateIdentity(definition.Id, provision.Id, unit.Scope, unit.Traveller.TravellerRef, unit.CoverageRef, price.PricingRevisionId);
            var eligibility = new EligibilityAssessment(
                outcome.Verdict switch
                {
                    RuleVerdict.Match => EligibilityStatus.Eligible,
                    RuleVerdict.NoMatch => EligibilityStatus.NotEligible,
                    _ => outcome.RulesDecided ? EligibilityStatus.Eligible : EligibilityStatus.InsufficientContext
                },
                definition.Id,
                provision.Id,
                outcome.ReasonCodes,
                outcome.MissingFields);
            var candidate = new CanonicalAncillaryOfferCandidate
            {
                CandidateIdentity = identity,
                ServiceDefinitionRef = definition.ServiceDefinitionRef,
                DefinitionVersionId = definition.Id,
                DefinitionVersion = definition.Version,
                SupplierId = definition.SupplierId,
                ProvisionId = provision.Id,
                ProvisionSequence = provision.Sequence,
                Profile = definition.Profile!.Value,
                VariantCode = definition.VariantCode!,
                ServiceTypeCode = definition.ServiceTypeCode,
                ServiceSubCode = definition.ServiceSubCode,
                CommercialName = definition.CommercialName,
                Description = definition.Description,
                Booking = new BookingSummary(definition.Booking.Method, definition.Booking.SsrCode, provision.Outcome.BookingRequired),
                Document = new DocumentRoutingSummary(
                    definition.DocumentRouting!.Value,
                    definition.Document.Type,
                    definition.Document.Rfic,
                    definition.Document.Rfisc,
                    provision.Outcome.DocumentRequired),
                Scope = new SelectionCoverage(
                    unit.Scope,
                    [unit.Traveller.TravellerRef],
                    unit.Flights.Select(flight => flight.FlightRef).ToList(),
                    unit.Portions.Select(portion => portion.PortionRef).ToList()),
                Eligibility = eligibility,
                Selection = definition.SelectionContract!,
                Quantity = quantity,
                Price = price,
                Availability = availability,
                Confirmation = new ConfirmationAssessment(
                    definition.Booking.Method,
                    provision.PetRule?.AcceptanceMode ?? definition.Booking.ConfirmationRequirement,
                    provision.Fulfillment.FulfillmentProviderKey,
                    ProviderConfirmationStatus.NotRequested),
                OfferReadiness = Readiness(provision, outcome, price, availability, quantity),
                ExpiresAtUtc = unit.Context.SourceIdentity.ValidUntilUtc,
                ReasonCodes = outcome.ReasonCodes
                    .Concat(price.ReasonCodes)
                    .Concat(availability.ReasonCodes)
                    .Concat(quantity.ReasonCodes)
                    .Distinct(StringComparer.Ordinal)
                    .ToList()
            };

            return new Decision(candidate, outcome);
        }

        private sealed record Decision(CanonicalAncillaryOfferCandidate Candidate, RuleOutcome Outcome);
    }
}
