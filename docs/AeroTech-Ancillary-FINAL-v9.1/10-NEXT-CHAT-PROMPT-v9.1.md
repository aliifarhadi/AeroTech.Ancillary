# 10 — NEXT CHAT PROMPT — Ancillary v9.1 Implementation Audit

Use this prompt in the next chat when the coding agent reports a milestone implementation.

---

We are implementing **AeroTech Ancillary FINAL v9.1**.

Act as:

- DDD architect;
- airline/PSS ancillary domain expert;
- cross-service contract auditor;
- implementation PM.

The coding agent writes code only. Do not trust its summary. Inspect the actual pushed commits/source/tests.

## Authority pack

Use, in order:

1. `00-FINAL-Authority-and-Source-Freeze-v9.1.md`
2. `01-FINAL-Industry-Benchmark-and-Traceability-v9.1.md`
3. `02-FINAL-Ancillary-Domain-Master-v9.1.md`
4. `03-FINAL-Backoffice-Authoring-Contracts-v9.1.md`
5. `04-FINAL-AirAvail-Retailing-and-ACL-v9.1.md`
6. `05-FINAL-Ordering-Fulfillment-Contracts-v9.1.md`
7. `06-FINAL-Scenario-Conformance-v9.1.md`
8. `07-FINAL-Implementation-Plan-v9.1.md`
9. `08-FINAL-Gap-Register-v9.1.md`

v9.1 supersedes v9.0 and all earlier Ancillary packs on conflict.

## Frozen design decisions to audit

### Domain roots

Exactly:

```text
Supplier
AncillaryServiceDefinition
AncillaryProvision
AncillaryReservation
AncillaryStockPool
```

No target Product/PriceRule/ServiceSubCode aggregate replacement.

### Supplier marketplace

Supplier is mandatory.

Required routing model:

```text
SupplierId                 = identity only
FulfillmentKind            = Local | External
FulfillmentProviderKey     = required for External
```

Audit for any `switch/if` keyed by numeric Supplier ID. That is a blocker.

`Supplier.FulfillmentProviderKey` is Ancillary-internal adapter routing and must not be confused with Ordering's `OrderService.FulfillmentProviderKey = "Ancillary"`.

No credentials/URLs/secrets in Supplier domain state. `FulfillmentKind` is routing classification only; audit against speculative supplier-specific capability enums/matrices that are not backed by a real integration contract.

### No speculative supplier issuance

v9.1 has no generic supplier post-confirm `Issuance`/`Activation` endpoint or no-op capability. A concrete real Supplier contract is required before introducing one.

Ordering still owns accountable ETKT/EMD issuance.

### Identity

Runtime accepted/fulfillment relationships use numeric:

```text
ServiceDefinitionId
ProvisionId
SupplierId
OrderServiceId
```

Never use `ServiceDefinitionRef + Version` as a runtime relationship protocol.

### Money

AirAvail owns retail FX. Ordering snapshots accepted commercial evidence. Ancillary Hold receives fulfillment facts and does not validate converted selling amount.

Normal platform money precision follows existing platform convention; ROE retains its separate high precision.

### Single evaluator

AirAvail is the only runtime ancillary rule evaluator.

Ancillary owns authoring/invariants/publication and supplier/quota fulfillment state.

Backoffice production simulation uses AirAvail. No second evaluator is permitted.

### Post-order flow

Required:

```text
Surface -> Ordering -> AirAvail
```

Forbidden:

```text
AirAvail -> Ordering order-context fetch
```

### ServiceSubCode

Industry code semantics come from read-only authoritative reference data; carrier-defined codes use frozen validation. No mutable ServiceSubCode aggregate.

### Seats

```text
AirInfo static layout
FlightFlow live seat state
Ancillary commercial rule
AirAvail priced SeatOffers
Ordering -> FlightFlow fulfillment
```

### Dead fields

Age/Occurrence/FF status and other source-gated qualifiers must not exist as inactive nullable domain fields.

## Execution model — critical

v9.1 is implemented through small vertical milestones, not three giant phases.

```text
M1 Walking Ancillary
M2 Marketplace routing + post-order
M3 Commercial rules + full Backoffice
M4 Baggage + quota + EMD
M5 Paid seat
M6 Closure/conformance
```

**The coding agent must stop after each milestone. Do not authorize the next milestone until the current one is audited.**

Do not penalize the implementation for features belonging to a later milestone. Do penalize final-model violations introduced early.

## Audit procedure

1. Verify repository SHA/commit claimed by the agent.
2. Inspect actual diff and changed files.
3. Run/read relevant tests; do not accept test-count summaries without test contents where semantics matter.
4. Compare only against the current milestone plus cross-cutting frozen rules.
5. Map implemented scenarios to `06` IDs.
6. Check migrations and persisted relationship keys.
7. Check public/internal DTO leakage.
8. Check Supplier typed routing and absence of numeric-ID strategy switches.
9. Check no generic Supplier Issuance endpoint/capability appeared.
10. Check no duplicate evaluator appeared in Ancillary.
11. Check no FX/repricing validation appeared in Ancillary Hold.
12. Check post-order call direction when M2 or later is in scope.
13. Check J2/FareFamilyId completeness when M3 or later is in scope.
14. Check EMD authority when M4 or later is in scope.
15. Check paid-seat ownership split when M5 or later is in scope.
16. Perform the milestone-specific negative/deletion audit.

## Verdict classes

Classify findings as:

```text
BLOCKER
REQUIRED
NON-BLOCKING
FUTURE / SOURCE-GATED
```

Give one milestone verdict:

```text
GO_TO_NEXT_MILESTONE
```

or:

```text
NO_GO_CORRECTION_REQUIRED
```

If GO, create a downloadable `.md` prompt for the coding agent containing **only the next milestone**.

If NO_GO, create a downloadable `.md` correction prompt with exact required changes and tests.

Always also create a downloadable next-chat prompt carrying forward v9.1 authority and the actual audited SHA/status.

Do not reopen frozen design decisions merely because a donor implementation differs. Reopen only with hard contradictory source/industry evidence and state that evidence explicitly.
