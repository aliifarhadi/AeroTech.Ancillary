# Pack v8 — Stage1: signed identity, issuance gate, zero rate

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v8` from `k8s-stg@49e9209`; HEAD `851ff5a` (your commit: pack + Gate0 reports); Stage1 uncommitted |
| Answers applied | Q1–Q4 of `reports/V8-QUESTIONS-AND-CONFLICTS.md`, all as recommended |
| Endpoints | none added or changed (Stage1 scope) |
| Migration | none |
| SeatMap | 48 frozen files unchanged; combined hash `2cab2be6…25fd`, same as Gate0 |

## 1. What was built, by layer

| Layer | Change |
|---|---|
| Contracts | enums `SelectionIdKind` (Reference, Direct) and `SignedSelectionStatus` (Valid, Expired, Rejected) |
| Domain | `AncillaryProvision.AllowedDispositions` now allows Paid, Free and NotAvailable for every variant; A11 and A15 were Free/NotAvailable only (Q2) |
| Shopping `Selections/` | `SelectionIdCodec` (JWS HS256 with `Shopping:SelectionSigningKey`, prefix `r_`/`d_`, purpose claim `p`, expiry, max 1024 characters, fail closed without a 32-byte key); `SelectionBindings` (offer digest exactly as pack 04 §2.1; sales digest = owner, point of sale, customer — see below); `SelectionSigner` (issuance gate); `SignedSelection`, `SignedSelectionReading`, `SelectionIdOptions` (`Shopping:SelectionMaxAge` required at start) |
| Shopping `Reading/` | `IActiveAncillaryPricingReader.FindActiveByRateIdsAsync` + SQL implementation: only an Active pricing that owns the rate |
| ServiceHost | `AddAncillaryShopping(configuration)`; `appsettings.json` `Shopping:SelectionMaxAge = 00:15:00` |
| Query, Application, RestApi | no change |

Two approved departures from pack 04: the token is the existing JWS of the repository, not the hand-made `prefix|payload.MAC`; the sales binding is `SHA256("AERO-SALE-v1|owner|pointOfSale|customerOrDash")` from the engine context only — channel, agency, office and country were removed on your order (the `Core.Enums.Channel` enum is not to be used, the three ids are unrelated). The offer binding still equals the pack golden vector; the pack's sales vector no longer applies.

## 2. Issuance gate (Shopping, `SelectionSigner`)

A `d_` is signed only for a `Selectable` candidate with quantity ≥ 1, an `r_` only for a `NeedsSelection` candidate. Both require:

- an Offer source with offer id, trusted version and `ValidUntilUtc`; expiry = `min(ValidUntilUtc, now + SelectionMaxAge)` in whole seconds, and later than now;
- price: `Filed`, `Complete`, real `PricingRateId` and `PricingRevisionId`, unit total known, sales currency = filed currency (no FX), no external quote, no unapplied fee, not `PerKilogram`;
- `ConfirmationRequirement = Immediate` (no approval);
- not the Seat profile (A07/A08/A09), not A17 (no encryption at rest) and not A19 (text/boolean mismatch);
- exactly one traveller, equal to the unit's traveller; none for an Order unit.

Bindings come only from the trusted engine context: offer id and trusted version, owner, the candidate's point of sale and the offer's customer. The `r_` also carries the offer id (Q4); `d_` does not.

Finding: the engine fills `RateOfExchange` even without conversion (identity rate 1, same currency). "No FX" is therefore checked as filed currency = sales currency.

## 3. Dev data (Q2)

SQL, same pattern as the seat change of 2026-10-21: 35 Active non-seat Free provisions became Paid / Filed, each with an Active pricing (unit of its definition, version 1) and one IRR rate of 0, in both stores. Converted: A03 1, A06 2, A11 8, A15 3, A16 3, A18 1, A20 8, A21 2, A22 2, A23 2, A24 3. After the change: 0 Free non-seat provisions, 0 Paid provisions without an Active pricing, command and read store pricings equal. Document and booking flags of those provisions were left as authored.

## 4. Tests

On your order of 2026-10-23 the whole `tests/AeroTech.Ancillary.Shopping.Tests` project was deleted, with the Stage1 tests in it, and removed from the solution. No test is written until the last stage. All tests are then added in one batch after your approval.

Adjusted existing tests, kept because the Q2 rule changed: `S1_CR05` and `S1R1_CR07` in Domain, `PROF_06` and `PROF_08_R2` in Acceptance now expect Paid to be allowed for A11. The refused-change example of `PROF_08_R2` and `S1R1_CR07` is now an OnBoard purchase stage (16323).

## 5. Proof (final tree)

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` after the test project was deleted | 0 errors, 57 warnings |
| SeatMap freeze | hash unchanged, 0 files changed against `49e9209` |
| Comments, CRLF | none added; line endings checked |

The proof is the clean build only. No live HTTP in this stage: nothing is reachable over HTTP yet.

Stage2 changed this stage's signer. A `d_` is now also signed for a candidate whose rules are decided but whose form is pending, and an `r_` is also signed for a Selectable candidate. See `reports/V8-STAGE2-SHOPPING.md` §2.

## 6. Next

Stage2 (after your review): three surface list contracts with family arrays and `quantityOptions`, using `SelectionSigner`; deletion of Details / Evaluate / EvaluateBatch / Resolve and of `Query/Shopping/Common/PricedSelections`; the shared constant for the SeatMap Resolve limit (Q3).
