# AeroTech Ancillary FINAL Pack v9.1

**Status:** `FINAL_IMPLEMENTATION_AUTHORITY`  
**Supersedes:** v9.0 and every earlier Ancillary pack on conflict.

v9.1 preserves the ATPCO/IATA/Amadeus/Sabre/Flydubai/Lufthansa benchmark decisions from v9.0 and corrects the execution/routing issues identified before implementation starts.

---

# 1. Files

1. `00-FINAL-Authority-and-Source-Freeze-v9.1.md`
2. `01-FINAL-Industry-Benchmark-and-Traceability-v9.1.md`
3. `02-FINAL-Ancillary-Domain-Master-v9.1.md`
4. `03-FINAL-Backoffice-Authoring-Contracts-v9.1.md`
5. `04-FINAL-AirAvail-Retailing-and-ACL-v9.1.md`
6. `05-FINAL-Ordering-Fulfillment-Contracts-v9.1.md`
7. `06-FINAL-Scenario-Conformance-v9.1.md`
8. `07-FINAL-Implementation-Plan-v9.1.md`
9. `08-FINAL-Gap-Register-v9.1.md`
10. `09-CODING-AGENT-PROMPT-v9.1.md`
11. `10-NEXT-CHAT-PROMPT-v9.1.md`
12. `SHA256-MANIFEST-v9.1.md`

---

# 2. What v9.1 changes from v9.0

## 2.1 Execution is vertical and feedback-first

The old three giant phases are removed.

Current execution plan:

```text
M1 Walking Ancillary
M2 Marketplace routing + post-order shopping
M3 Commercial rules + complete Backoffice
M4 Baggage + quota + EMD
M5 Paid seat
M6 Closure/conformance
```

After **every milestone**, the coding agent stops for architect audit. It does not implement multiple milestones in one run.

M1 proves an actual ancillary sale/reservation before full Backoffice/reference/rule expansion.

## 2.2 Supplier routing is typed

`SupplierId` is identity only.

Supplier includes:

```text
FulfillmentKind          Local | External
FulfillmentProviderKey   string?   // required for External
```

External behavior is resolved by provider key, never numeric Supplier ID.

## 2.3 Speculative Supplier Issuance removed

There is no generic supplier-side post-confirm `Issuance`/`Activation` endpoint in CORE and no no-op capability.

A future real Supplier may add such an operation only when its actual contract proves the need.

Ordering remains accountable ETKT/EMD authority.

---

# 3. Frozen core decisions

1. Aggregate roots exactly:
   `Supplier`, `AncillaryServiceDefinition`, `AncillaryProvision`, `AncillaryReservation`, `AncillaryStockPool`.
2. Ancillary is an ancillary **marketplace**; multiple Suppliers may provide the same broad service/subcode.
3. Supplier routing uses `FulfillmentKind + FulfillmentProviderKey`; no `switch(SupplierId)`.
4. `ServiceDefinitionId`, `ProvisionId`, `SupplierId` are runtime identities.
5. `ServiceDefinitionRef + Version` is audit/display metadata, not a runtime FK protocol.
6. Industry ServiceSubCode semantics are validated from read-only reference data; no mutable ServiceSubCode root.
7. AirAvail is the single runtime ancillary evaluator.
8. Backoffice production simulation uses AirAvail; Ancillary Preview is static authoring validation only.
9. Pre-order shopping starts in AirAvail.
10. Post-order shopping starts in Ordering and calls AirAvail one-way.
11. `AirAvail -> Ordering` order-context fetch is forbidden.
12. AirAvail owns retail FX; Ordering snapshots accepted price/ROE evidence.
13. Ancillary Hold does not validate converted selling-currency `AcceptedRevenue`.
14. Normal money follows platform precision; ROE keeps platform high precision.
15. AirInfo owns static aircraft/cabin/seat reference data.
16. FlightFlow owns live physical seat state and seat fulfillment.
17. Ancillary owns paid-seat commercial rules only.
18. Generic ancillary offers and physical SeatOffers remain separate shopping flows.
19. Ordering owns accountable ETKT/EMD stock, issuance and lifecycle.
20. No dead Age/Occurrence/FF/advanced qualifier fields before authoritative context exists.
21. No generic Supplier Issuance/Activation API without a concrete real Supplier contract.
22. Existing repository architecture/layers are implementation authority; the Pack does not redesign them.

---

# 4. Benchmark stance

Primary semantic authority remains:

```text
ATPCO Optional Services
IATA NDC / PADIS / EMD
```

PSS/airline implementation benchmarks retained:

```text
Amadeus Altéa
Sabre / SabreSonic / NDC
flydubai Developer APIs
Lufthansa Group partner/developer APIs
Navitaire New Skies / Travel Commerce
Hitit Crane
IBS iFly RES
Accelya FLX ONE
Datalex
PROS
KIU
```

Flydubai remains especially useful for:

- ancillary shopping vs reservation commitment separation;
- separate service vs seat pricing flows;
- compact technical service IDs in commitment flows.

Lufthansa remains additive evidence for:

- provider/settlement identity distinct from validating/commercial carrier;
- static seat reference vs dynamic seat availability separation.

No private/inaccessible vendor schema is invented.

---

# 5. Implementation start

The coding agent starts with:

`09-CODING-AGENT-PROMPT-v9.1.md`

and implements **Milestone 1 only**.

Required first marker:

`ANCILLARY_V91_M1_WALKING_SLICE_READY`

Then stop for audit using:

`10-NEXT-CHAT-PROMPT-v9.1.md`

Final program marker only after M6:

`ANCILLARY_V91_IMPLEMENTED_READY_FOR_ARCHITECT_AUDIT`
