# AeroTech Ancillary - FINAL v12.0 - Single Project Authority

Date: 2026-10-08. Status: FINAL DESIGN AUTHORITY; ONLY PHASE 1 IMPLEMENTATION AUTHORIZED. Owner controls the stage sequence. Coding Agent writes and tests code; no independent domain or architectural decisions.

## Repository and decision
- Main: https://github.com/aliifarhadi/AeroTech.Ancillary ; branch k8s-stg ; verified HEAD `6b0a80ff708815ce3a6a957fbef3380b83a3f4cd`.
- Historical skeleton `843cf7ccda45a1e16b1bd537172346fcdd76ae35`, old baseline `e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8`.
- AirPrice benchmark: https://github.com/aliifarhadi/Aerotech.AirPrice ; branch k8s-stg ; verified HEAD `1b41f08e9a22d3c27b807d5ea275120f0f7273ad`.
- **Continue from current Ancillary HEAD, do not reset**. Existing working modules, migrations and tests are substantial. Change the schema deliberately and preserve data. If actual HEAD moved: compare source changes and stop on material conflict.

## Single authority and priorities
1. Latest express Owner decisions; 2. this complete v12 pack, in document-number order; 3. actual code verified at pinned commits; 4. official ATPCO Optional Services semantics and AirPrice source patterns; 5. older v11 only for historical explanations if no conflict.

Supersedes v11 on normalized Provision and independently versioned Pricing, age selectors and three-phase project sequence. Do not mix older v11 prohibitions of age bands/separate Pricing into v12. Do not interpret v12 as authorization to start phases 2 or 3.

## Three sequential phases; no autonomous phase
**Phase 1 NOW**: Supplier unchanged supporting aggregate; commercial aggregate roots (1) `AncillaryServiceDefinition`, (2) `AncillaryProvision`, (3) `AncillaryPricing` with all typed child entities, invariants, behavior, persistence, SQL migrations, Backoffice authoring/list/detail/lifecycle, fixed filed prices and tests. No runtime shopping matcher or inventory.
**Phase 2 FUTURE OWNER GATE**: capacity, stock pools, finite/unlimited/external availability classification, administration and concurrency-safe stock accounting. No Hold/Confirm implementation yet.
**Phase 3 FUTURE OWNER GATE**: reservation, Hold/Get/Confirm/Expire/Release/Cancel/partial servicing, allocation and supplier boundary, change/refund/EMD-related servicing and integration with Ordering and the flight/offer contexts as separately authorized. No phase 4.

## Commercial topology
```
(existing) Supplier (supporting AR, frozen)
    1 -> N AncillaryServiceDefinition (commercial AR #1, identity + fixed PricingUnit)
              1 -> N AncillaryProvision (commercial AR #2, eligibility + outcome)
                        1 -> N AncillaryPricing (commercial AR #3, price versions)
                                  1 -> N AncillaryPricingLine (entity)
```
No M:N shared provisions. Pricing conditions of eligibility stay on Provision; monetary rate selectors on PricingLine. One active Pricing per Provision, database filtered unique index. One fixed PricingUnit per service identity. No runtime matcher in Ancillary Phase 1; publishing means usable authored data, not a shopping quote.

## Domain rules for all phases
- No invented ATPCO/IATA codes, provider protocols or raw cross-service reference masters. No generic EAV, JSON rule DSL, reflection matcher, fake preview engine or family-specific aggregates.
- Keep source conventions for IDs, EF mappings, DI, MediatR, CQRS projection, unit of work, errors, timestamps, tests. Treat AirPrice as modeling benchmark, not code donor.
- Preserve Source-of-Truth boundaries: Ordering owns Order/Ticket/Coupon/EMD; FlightFlow owns live flight/seat state; AirPrice owns air fare rules. Supplier adapter boundaries are not inferred from frozen Supplier metadata.
- Zero mutation in Phase 1 to AncillaryReservation, Hold/Get/Confirm, AirAvail, Ordering, FlightFlow, JetPay or other service source.
- Any fundamental unresolved semantic issue: present exact `REPORT_GAP_AND_STOP` and minimal options; do not invent missing behavior.
- On every implementation milestone give actual commit SHA, changed files, migration proof, tests, negative audit, and precise follow-up Coding Agent prompt where needed.

## Reading order / pack contents
- `01-SOURCE-AND-BENCHMARK.md`: audited AirPrice and Ancillary source and official industry benchmark.
- `02-DOMAIN-ADR.md`: three commercial AR responsibilities, state transitions, invariants.
- `03-PROVISION-ENTITY-SCHEMA.md`: **field-by-field** typed normalized criteria, dates, day/time, blackout and all child rows.
- `04-PRICING-ADR.md`: exact price schema, unit/selector/rate composition and publication.
- `05-BACKOFFICE-CONTRACTS.md`: CRUD, lifecycle, projections, examples and validation behavior.
- `06-PERSISTENCE-MIGRATION.md`: data-preserving migration from v11 plus repository impact.
- `07-PHASE2-STOCK-CAPACITY.md`: complete bounded design and future decisions, not executable now.
- `08-PHASE3-RESERVATION.md`: operational scope, FlightFlow comparison and future contracts, not executable now.
- `09-SCENARIOS-TESTS-EXIT.md`: executable phase-1 scenarios and gates.
- `10-PHASE1-CODING-AGENT-PROMPT.md`: **self-contained current Coding Agent prompt**.
- `11-ROADMAP-HANDOFF.md`: exact phase gates and decisions.
- `12-NEXT-CHAT-PROMPT.md`: preserved continuity context.

Current implementation task = ONLY document 10 (read 00-09 first). Owner reviews Phase 1 code before any Phase 2 coding.
