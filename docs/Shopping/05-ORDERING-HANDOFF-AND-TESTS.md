# 05 — Handoff به Ordering و ماتریس پذیرش Pack v8 (مرجع قطعی آزمون‌ها)


### C0 غیرقابل مذاکره: ممنوعیت ارجاع به قوانین قدیمی

- **Quantity-only:** فقط Shopping `quantityOptions[]` دارد؛ **هیچ POST Options، GET Options، Snapshot، Insert، TTL row** برای Quantity-only وجود ندارد. ID تعداد یک عدد صحیح امضاشده `d_` است. O07/H05/S11/T04/R05/N04 باید همین حکم را اثبات کنند.
- **Retired:** `AncillaryPricingRate` در سورس `Status` مستقل ندارد؛ **Pricing.Status=Retired/Suspended** (یا Provision/Definition غیر Active) مانع صدور، POST Typed و Resolve است. استفاده از Rateهای Retired **حتی برای توکن قبلاً صادرشده** در مرحله ثبت سفارش مجاز نیست. تاریخچه Orders قبلاً ثبت‌شده مسئله دیگری است و موضوع این Flow نیست. موارد H13/T14/T17/T18/T19/R13/R14 الزامی‌اند.
- **A17/A09/A19/Insurance GATED** را به `PASS` تبدیل نکنید. کل ۷۸ مورد Scenario هستند، نه ۷۸ مورد موفق اجتناب‌ناپذیر.
- هیچ مسیر بازگشت به سندی بیرون از این Pack به‌عنوان Authority وجود ندارد.

## A. حدود مسئولیت (فقط قرارداد؛ بدون تغییر کد Ordering در این Stage)

| مسئولیت | Ancillary | Ordering |
|---|---|---|
| Shop + Eligibility + Price at issuance | Y | N |
| HMAC genuine signed ID + TTL | Y | N |
| Typed POST validation + snapshot | Y | N |
| Resolve chosen Rate/Definition/Provision/Quantity/selected | Y | N |
| Match Offer binding and actual Order Offer | return sourceBinding | **Y** |
| Match TravellerRef to OrderTraveller / OrderOffer PTC | return canonical Unit | **Y** |
| Portion BoundRef expansion and FlightRefs membership | return Portion authoritative | **Y** |
| Match sales/channel binding to Order seller/initiating actor | return salesHash | **Y** |
| Deduplicate same commercial service and unit; reject cross-Order replay | no consume | **Y** |
| Hold/Confirm/Cancel/EMD, pricing acceptance | out of this Stage | Owner of Order lifecycle |

**Binding contract:** دقیقاً از الگوریتم Canonical سند `04` استفاده می‌شود.  `sourceBinding.offerHash` is SHA256-base64url از `AERO-OFFER-v1|B64(offerId)|B64(trustedSourceVersion)` from Shop; Ordering must already know source Offer and version or request them from its own authoritative state. `sourceBinding.salesHash` is SHA256-base64url از Canonical Sales fields مطابق `04`; **Ordering must either compute same canonical hash from its verified sales context or reject**. Never add raw PointOfSale/Country to Resolve request. A technical HMAC-valid selection does not prove permission to attach it to an arbitrary Order.

**Unit vs Coverage:** `unit` identifies one sale unit; `coverage` describes applicability only to extent genuinely recorded. `PerTravellerPerPortion` for Bound `B1` can span Flights `B1:F1` and `B1:F2`; if signed d_ has only `B1`, `Resolve.coverage.flightRefs=[]` and Ordering expands from its Order itinerary. Never silently mark it as `PerTravellerPerFlight` or double charge.

## B. Input and output Handoff example

POST from Ordering:
```json
{"ids":["d_<signed-A01-qty2>","s_<opaque-A15>"]}
```

Selected resolved record common fields (real values differ):
```json
{
  "selectionId":"d_<signed-A01-qty2>",
  "variantCode":"A01",
  "selectionScope":"PerTravellerPerPortion",
  "unit":{"travellerRef":"ADT-1","portionRef":"B1"},
  "coverage":{"travellerRefs":["ADT-1"],"portionRefs":["B1"],"flightRefs":[]},
  "sourceBinding":{"offerHash":"<sha256-base64url>","salesHash":"<sha256-base64url>"},
  "definition":{"definitionVersionId":102,"serviceDefinitionRef":"BAG23","variantCode":"A01","serviceTypeCode":"C","serviceSubCode":"0DF"},
  "provision":{"provisionId":202,"coverageScope":"Portion","purchaseStage":"PreOrder","bookingMethod":"Ssr","bookingRequired":true,"confirmationRequirement":"Immediate","documentRouting":"Emd","documentType":"EmdAssociated","documentRequired":true,"requiresCheckAtReserve":true},
  "pricing":{"pricingId":302,"pricingRateId":402,"pricingVersion":1,"currencyId":47,"pricingUnit":"PerPiece","quantity":2,"baseAmount":106.88,"unitAmount":106.88,"totalAmount":213.76,"taxLines":[],"feeLines":[]},
  "details":{"chargeKind":"ExtraPiece","maxKgPerPiece":23}
}
```

**Fixture note:** provider/SSR/RFIC/EMD/amount/units in sample represent **illustrative records**, not actual production configurations; actual values MUST be mapped from Domain. Ordering should not blindly assume `documentRequired` or BookingMethod based on Variant.

## C. Mandatory acceptance — controlled matrices (78 checks)

Each case requires: `PASS|FAIL|GATED` + Test name + execution evidence. GATED cases are not counted as passing functional support.

### C1 Scope × Quantity × Family (16 cases)

| ID | Scenario | Required outcome |
|---|---|---|
| S01 | Order unit Direct | one row + one signed d_ |
| S02 | Order unit Qty(1,2) | one row + two signed IDs |
| S03 | Traveller×Journey Direct for 2 pax | 2 rows, distinct IDs |
| S04 | Traveller×Journey Qty for 2 pax | 2 rows, distinct quantityOptions |
| S05 | Pax×Portion for 2 pax, 2 portions | 4 rows |
| S06 | Pax×Flight for 2 pax, 3 flights | 6 rows |
| S07 | Connecting Portion with 2 flights | 1 row per pax/portion, not 2 |
| S08 | Same Rate, two travellers | IDs differ |
| S09 | Same Rate, two flights | IDs differ |
| S10 | Same Rate, two portions | IDs differ |
| S11 | Same Unit, Qty 1 vs Qty 2 | IDs differ, unit same |
| S12 | Qty 0 | no purchaseable ID |
| S13 | Rate 0 with PricingRateId | signed d_, total 0 |
| S14 | Non-traveller Sector unsupported | no fake Offer item |
| S15 | Unknown passenger in unit | no ID |
| S16 | Wrong flight membership | no ID |

### C2 Shopping by Surface (13 cases)

| ID | Scenario | Required outcome |
|---|---|---|
| H01 | Backoffice offerId-only | grouped family arrays |
| H02 | OtaPanel offerId-only | grouped family arrays |
| H03 | Api offerId-only | grouped family arrays |
| H04 | fixed Direct | selectionId + real Price; no Reference |
| H05 | Qty-only bag A01 | quantityOptions, no snapshot |
| H06 | Typed wheel A15 | reference + options endpoint, no final ID |
| H07 | Quote-needed upgrade | absent from buyable list |
| H08 | Approval-needed A17 | absent |
| H09 | Price incomplete/FX/unapplied Fee | absent |
| H10 | ExtraSeat A09 gated | extraSeats=[] |
| H11 | Insurance nonexistent Domain | travelInsurance=[] |
| H12 | Currency sample EUR | internal id 47 from Catalog (per Owner), not ISO 978 |
| H13 | Pricing parent retired/suspended | candidate never published and no r_/d_ |

### C3 Security and forged tokens (19 cases)

| ID | Scenario | Required outcome |
|---|---|---|
| T01 | Unsigned reference | reject |
| T02 | Unsigned d_ | reject |
| T03 | Mutate PricingRateId | reject invalid MAC |
| T04 | Mutate Quantity | reject |
| T05 | Mutate TravellerRef | reject |
| T06 | Mutate FlightRef | reject |
| T07 | Mutate PortionRef | reject |
| T08 | Mutate expiry | reject |
| T09 | Use r_ as final ID | reject purpose |
| T10 | Use d_ as options reference | reject purpose |
| T11 | Wrong SalesScope/agency actor at POST | 403 |
| T12 | Forge CHD Rate for ADT | no signed ID/snapshot |
| T13 | Other POS cheapest Rate | no ID |
| T14 | Retired Rate as new selection | no ID |
| T15 | Expired d_/r_ | 410 |
| T16 | Expired s_ despite retained DB row | 410 |
| T17 | d_ issued while active, Pricing becomes Retired before Resolve | reject, no historical Rate read |
| T18 | s_ issued while active, Pricing becomes Retired before Resolve | reject, no historical Rate read |
| T19 | r_ issued while active, Pricing becomes Suspended before POST Options | reject, zero new Snapshot rows |

### C4 Typed Options (12 cases)

| ID | Scenario | Required outcome |
|---|---|---|
| O01 | A15 GET SSR allowed options | real Code list |
| O02 | A15 POST allowed SSR | one snapshot + s_ |
| O03 | A15 POST invented SSR | 422, no snapshot |
| O04 | A13 Pet valid dims and weight | typed details, s_ |
| O05 | A13 excess/max constraint | 422, no snapshot |
| O06 | A06 conditional dimension | required iff real product requires |
| O07 | A12 fixed menu + Quantity | signed d_ options, no snapshot |
| O08 | A12 selectable menu | typed reference + snapshot |
| O09 | A23 optIn absent/false | no selection |
| O10 | A23 optIn true | snapshot + s_ |
| O11 | A17 without approved encryption | gated; no storage |
| O12 | Same POST typed twice | two valid IDs permitted; no hidden dedup infra |

### C5 Resolve and Order handoff (14 cases)

| ID | Scenario | Required outcome |
|---|---|---|
| R01 | Mixed d_ and s_ | one ids[] response grouped by actual family |
| R02 | Resolve no OrderId/OfferId/POS/Country | succeeds for valid IDs |
| R03 | Resolve DB rate→Pricing→Provision→Definition | correct IDs/type |
| R04 | Free amount zero | real rateId and total 0 |
| R05 | Qty2 | actual signed qty, total exact |
| R06 | Typed selected values | persisted fields, no Caller-selected arg |
| R07 | Duplicate same ID | batch 400; no double service |
| R08 | Repeat Resolve same ID | same response, snapshot not consumed |
| R09 | Portion resolved without flightRefs | no invented flight IDs; Ordering expands |
| R10 | Mismatched Offer binding | Ordering rejects integration |
| R11 | Mismatched Traveller/Offer | Ordering rejects integration |
| R12 | Wrong sales binding or duplicate service | Ordering rejects |
| R13 | Non-active Pricing (Retired/Suspended) looked up by valid unexpired d_/s_ | Entire Resolve rejected; no data from non-active parent |
| R14 | Mixed batch with one invalid/retired ID | Entire batch fails; no partial resolved services |

### C6 Nonregression (4 cases)

| ID | Scenario | Required outcome |
|---|---|---|
| N01 | SeatMap GET/Resolve | unchanged HTTP body and fields |
| N02 | SeatMap Hold/Confirm | unchanged behavior and test suite |
| N03 | No Order/Legacy unexpected regression | scope-diff audit |
| N04 | No new Aggregate / no supplier fake | source + migration diff audit |

**Count:** 16+13+19+12+14+4=**78**. For R10–R12 in Ancillary-only milestone: **contract/integration fixture** is acceptable, not claim real Ordering code tested. For production closure, integration evidence required.

## D. Runtime verification artifacts for Agent

- Unit tests codec/HMAC with stable golden bytes, no accidental clocks; expiry boundary `now==exp` invalid.
- Service-level tests Shopping/Options/Resolve including 3 Surface HTTP fixture snapshots.
- EF migration and duplicate Id collision test for Typed only; D2 A17 domain fields may require targeted extra migration while endpoint remains gated.
- DB zero-priced Rate proof and no rewrite PriceAssessor.Free engine.
- DB failed POST leaves zero new snapshot rows; no snapshot from Qty-only.
- Domain A17/A19/A09/Insurance gated, no fake authoring.
- SHA, diff, benchmark traceability, SeatMap frozen-path hashes, test output; report which tests run vs skipped.
- Do not claim end-to-end Ordering wiring without repo/code evidence. Phase is closed only with Owner review.

## E. قفل نهایی مدل تست

ماتریس همین فایل **تنها ماتریس پذیرش این Pack** است. اجرای آزمون‌ها صرفاً با همین شناسه‌های `S01..N04` انجام می‌شود؛ هیچ جدول/Prompt قدیمی در Gate5 وارد نشود. وجود شناسهٔ `d_` در خرید Quantity فقط حاصل Shopping است و Snapshot فقط برای `s_` Typed مجاز است. **برای ثبت جدید، Resolve هر Pricing با Status غیر Active را رد می‌کند، حتی اگر ID هنوز داخل TTL باشد.**
