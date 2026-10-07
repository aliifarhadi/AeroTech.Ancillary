# AeroTech Ancillary — FINAL Pack v11.0

**Status:** `FINAL_PHASE1_IMPLEMENTATION_AUTHORITY`  
**Source baseline:** `aliifarhadi/AeroTech.Ancillary @ e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8`  
**Supersedes:** v10.0, v9.1 and every earlier Ancillary pack wherever they conflict with v11.0.

## Owner decision

v11.0 has exactly one authorized implementation phase:

> **Phase 1 = complete definition, pricing-rule authoring and Backoffice management of ancillary products.**

The owner alone decides what happens after Phase 1.

Phase 1 MUST NOT implement or redesign:

- AirAvail shopping or ancillary evaluation;
- Ordering integration;
- FlightFlow integration;
- Reservation/Hold/Confirm/Cancel/Issue expansion;
- supplier adapters;
- stock/quota;
- EMD issuance;
- FX;
- any other downstream workflow.

The existing Hold/Confirm/Reservation code at the source baseline is **frozen baseline code** during Phase 1. It is not deleted, expanded, normalized or used as a reason to invent future architecture.

## Core design

The model intentionally follows the simple airline Optional Services split:

```text
Supplier
   |
   +-- AncillaryServiceDefinition   ~= what the service is (S5-like)
           |
           +-- AncillaryProvision   ~= when/to whom/where/at what filed price (S7-like)
```

No generic rule engine. No EAV. No JSON rule DSL. No evaluator in Ancillary.

## Files

1. `00-FINAL-Authority-and-Source-Freeze-v11.0.md`
2. `01-FINAL-Industry-Benchmark-and-Phase1-Stress-Test-v11.0.md`
3. `02-FINAL-Ancillary-Phase1-Domain-Master-v11.0.md`
4. `03-FINAL-Backoffice-Authoring-Contracts-v11.0.md`
5. `04-FINAL-Pricing-and-Eligibility-Rule-Authoring-v11.0.md`
6. `05-FINAL-Family-Definition-Catalog-v11.0.md`
7. `06-FINAL-Scenario-Conformance-v11.0.md`
8. `07-FINAL-Implementation-Plan-v11.0.md`
9. `08-FINAL-Out-of-Scope-and-Gap-Register-v11.0.md`
10. `09-CODING-AGENT-PHASE1-PROMPT-v11.0.md`
11. `10-NEXT-CHAT-PROMPT-v11.0.md`
12. `11-FINAL-Phase1-Exit-Criteria-v11.0.md`
13. `SHA256-MANIFEST-v11.0.md`
