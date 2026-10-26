# 05 — مکانیزم رزرو از ورودی Type-specific تا Allocation / Confirm / Partial

## 1. Create pipeline — یک جریان، بدون دوباره‌کاری
1. Rest controller typed JSON، `idempotencyKey`, `orderId`, `reference`, expiry و گروه‌های Family را می‌خواند. **هیچ Ordering dependency/call وجود ندارد**.
2. Validator نوع/ساختار: family موجود، variantCode وجود دارد، products/units nonempty، orderServiceId nonduplicated، `unit/coverage/selected` فقط fields مجاز، Qty معتبر. در DTO از `JsonUnmappedMemberHandling.Disallow` استفاده شود.
3. `ReservationHoldMapper` (Application/REST adapter کوچک، نه سرویس/Engine تازه) **حفظ ساختار Nesting ولی flatten داخلی به UnitArgs**: برای هر `{family,variantCode,provisionId,orderServiceId,...}` یک UnitArgs. از `provisionId` Active Provision خوانده، Definition/Variant/Scope/Quantity/Policy را خودش استخراج می‌کند.
4. The type-specific choice is taken from `selected.options.baggage|assistedTravel|airportService|priority|connectivity` (the `pet` key is refused while A13/A14 are GATED). One Application port, following the `ISeatSelectionVerifier` pattern and implemented in Query with the Price rules (`TypedSelectionForm.Fields`, `TypedSelectionForm.WithFixedChoices`), validates the options against the static Definition/Provision rules and returns the fixed choices to capture (Owner Q2). `selected.quantity` is optional (fixed 1) or required when chosen. The typed selection is stored immutable for the GET; **no JSON blob**.
5. `AncillaryHoldChecks` موجود: Active Definition/Provision/Supplier، Quantity، Family/Variant، Source/Policy، coverage own rules؛ no Order membership checks. برای Local FlightCount/Weight/Slot از `coverage.flightIds` عددی یا appointment selected استفاده شود. `InventoryHoldFacts` DTO وجود ندارد.
6. `AncillaryHoldAllocator` موجود demand هر Source را از Policy و Quantity/TypedSelection می‌سازد؛ semantic timeWithOffset/GuestCount را **از Selected AirportService** به دست بیاورد. flightIds فقط وقتی لازم است. Source ID را از DB بگیرد، نه Client.
7. `InventoryLedger.OpenAsync` موجود: distinct source keys sort، SQL transaction + `sp_getapplock` (owner Transaction)، current capacity and ActiveUsage read under lock، sum demands all Units per Source. Shortage => 409 و rollback کل Request. `Unlimited`/Seat-only: zero allocations اما همان transaction مربوط Root.
8. `AncillaryReservation.Hold` Root + Unit + immutable selected + child allocations؛ یک `SaveChanges` و commit. Same IdempotencyKey + same semantic payload => همان Hold؛ differing typed options/qty => conflict.
9. `201` small Receipt فقط HoldId, Reference, Status, ExpiresAt, UnitIds+OrderServiceIds+UnitStatus.

## 2. Inventory source × consumer
| Pattern current | Needed from Hold | Derived internally | Allocation |
|---|---|---|---|
| Unlimited | Product `provisionId` + selected if applicable | Active Policy | 0 rows |
| Local FlightCount | `coverage.flightIds`, `selected.quantity` | Policy count.ResourceId + CountPerUnit; inventory source per Flight | Count per Flight |
| Local FlightWeight FixedKg | `coverage.flightIds`, `selected.quantity` | Policy WeightResourceId + FixedKgPerUnit | fixed kg × Qty per Flight |
| Local FlightCountPlusWeight | same | both Policy sources | **both in same transaction** |
| Local AirportSlot | `selected.options.airportService.timeWithOffset` and `guestCount` when allowed | Definition AirportId + Policy FacilityId, OccupancyMinutes, peoplePerUnit | each covering time slice persons = `peoplePerUnit×(Qty+Guests)` |
| SeatMap | dedicated seatMapSelectionId etc | commercial verifier | 0 Local allocations |
| Supplier / AcceptedWeight / unsupported Pattern | — | lacks provider/approved policy | explicit fail-closed |

**Use existing Aggregate source rows**, do not decrement `TotalCapacity`. `Available = Capacity − SUM(Held nonexpired + Confirmed Allocations)` including shared products in same source. For Portion with two flights, both source rows reserved atomically. For AirportSlot no guessed appointment (do not substitute `UtcNow`).

## 3. Runtime errors, concurrency and Admin safety
- Two different Orders competing for last Count/Weight/Slot unit: exactly one commit; other 16417 (resource lock, not only `ancillary-hold:{orderId}`). SQL proof mandatory.
- Same source shared by two Products → same usage ledger. No duplicate counters; no source ID in request.
- `Retire` under same source lock must verify `ActiveUsage == 0`, otherwise 409 `16621` existing; otherwise Retire + re-Define can sidestep active allocation. Do it for Count, Weight, Slot, without new service.
- Adjust capacity under lock checks `newCapacity >= ActiveUsage`, already v10; Close/Suspend forbid NEW Hold, old Release/Cancel remain allowed.
- Connection Pool pressure measured/tested; don't build new Lock Manager or async background orchestrator for this change.
- All reservation rows on dev are deleted before the v11 migration (Owner Q5); no legacy reservation remains to audit. Never delete Order or other history data without an Owner instruction.

## 4. State matrix
| Action | Root/Unit requirement | Effect on typed details | Effect on allocation |
|---|---|---|---|
| Create Hold | Future expiry, valid inputs, Local capacity | Persist immutable | write allocation once |
| Confirm | Held not expired; released members untouched | unchanged | allocation remains counted |
| Release all | Root Held | unchanged, Unit→Released | logically no longer counted |
| Release `unitIds[]` | selected Units Held or previously Released | unchanged for all; only specified transitions | specified held no longer counted |
| Cancel all | Root Confirmed | unchanged, Unit→Cancelled with reason | logically no longer counted |
| Cancel `unitIds[]` | selected Units Confirmed or matching Cancel replay | unchanged for all | specified cancelled no longer counted |
| Expire | Held and now≥ExpiresAt | unchanged, Unit→Expired lazily/poller | logically no longer counted before poller |
| Seat append | same Root Held, same Header, unique Seat selection | create commercial immutable Seat selection | **zero physical/Local Seat allocation** |

**Atomicity:** Partial actions all-or-nothing for explicitly provided UnitIds. `?unitIds=` empty should be treated as absent (=all) only if v10 behavior kept and tested; explicit `unitIds` zero/invalid/duplicate must not silently expand. No standalone Unit endpoint.

## 5. Partial lifecycle scenarios
- Hold includes A01/Baggage, A11/Meal, A15/WCHR. Release only Meal by unitId; other Units Held and allocations unchanged. Confirm -> Baggage+WCHR Confirmed; Meal Released historical. Cancel only Baggage -> Baggage Cancelled and capacity free; WCHR remains Confirmed.
- Another Hold shares same Count source: capacity reflects only active Unit allocations. Deleting ledger rows on Cancel prohibited; inventory usage query predicate controls accounting.
- Replay Create after Seat appended: compare original Create Unit set only (v10 behavior), no new Reservation. Seat append own IdempotencyKey and Header comparison.

## 6. SeatMap exclusive responsibility
`POST /Service/v1/Ancillaries/SeatMaps/Holds` remains distinct; Verifier may use AirInfo and SeatMap commercial pricing. Ancillary **does not call** FlightFlow's Seat-Holds/OccupiedSeats/Confirm/Release nor Ordering. Same HoldId can contain Seat commercial Unit and other Typed Units. Real seat physical occupancy/cancellation between Ordering and FlightFlow outside scope. `Price`, `Resolve`, `SeatMaps/Resolve` unchanged.

## 7. What is NOT implemented
No Supplier provider/reservation until real contract. No EMD issue/refund/Payment in Ancillary, no Request parameter `orderCommercialVersion` on generic Hold, no Rate/Price check, no Order membership validation, no Gift generalization, no Event Sourcing, no Saga, no 24 ancillary reservation aggregates.
