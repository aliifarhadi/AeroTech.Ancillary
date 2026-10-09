# 06 — ماتریس تست سناریوها و معیار بسته‌شدن فاز

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


**این‌ها تست طراحی/پذیرش پیشنهادی‌اند و هنوز اجرا نشده‌اند.** Scenario IDs باید مستقیماً به test methodها و شواهد CommandDb/QueryDb وصل شوند. هیچ Agent حق ندارد با یک تست End-to-end ساده کل خانواده‌ها را PASS اعلام کند.

| ID | سناریو | انتظار دقیق |
|---|---|---|
| C01 | محصول truly unlimited | Policy=Unlimited، هیچ Counter/Capacity row و Amount مصنوعی ندارد |
| C02 | محصول بدون policy | NotConfigured / fail-closed، نه unlimited |
| C03 | supplier-managed | عدم تولید local stock؛ provider/observedAt/stale مشخص |
| C04 | FlightFlow seat | capacity mirror ساخته نشود؛ blocked/occupied/available متمایز |
| C05 | مقدار ظرفیت 0 | ZeroConfigured/ClosedForSale distinct from Unlimited; no guaranteed booking SoldOut in Phase2 |
| C06 | service eligibility NotAvailable | نتیجه CommercialNotAvailable، حتی اگر capacity positive باشد |
| C07 | qty/price/capacity unit mismatch | activation/assignment رد می‌شود |
| F01 | flight F1، 4 PETC slots | کلید FlightId+Resource، count=4 |
| F02 | دو carrier-defined PETC SKU یک slot مشترک | یک ResourceId و فقط یک مخزن واقعی |
| F03 | flight F1 vs F2 | ظرفیت کاملاً مستقل، flight number مشترک irrelevant |
| F04 | airline+supplier mismatch | FK/owner check reject |
| F05 | double active source for same resource key | unique DB rejection |
| F06 | capacity change 8→5 با expected version درست | adjustment immutable با actor/reason |
| F07 | stale expected version | 409 conflict بدون overwrite |
| F08 | FlightCount Status Suspended | فروش بسته؛ اصل مقدار حفظ |
| F09 | Catering meal CHML/VGML از 30 meals quota مشترک | دو Product بدون double-count mapping |
| F10 | Meal order cutoff 24h | Provision gate نه Stock counter |
| W01 | 5kg fixed excess bag package با PriceUnit=PerItem | consumption=5kg صریح، نه 1kg |
| W02 | 2 بسته 10kg | 20kg commercial weight demand |
| W03 | qty decimal fractional kg در قرارداد weight-input | precision 18,3 و reject unsupported |
| W04 | bag piece + kg dual-control | دو typed resource bindings در یک Policy |
| W05 | flight cargo safety actual vs commercial quota | independent authority، no false safety approval |
| W06 | Weight decrease below historical obligations بعد از فاز3 | reject; در فاز2 فقط قرارداد و test future-gate |
| A01 | airport IKA lounge L1 vs L2 | per Facility capacity مستقل |
| A02 | 20 seats / 30-min slots | slot model bounded، canonical nonoverlap |
| A03 | arrival at 10:30 / duration 90min | تمام slotهای intersecting را مصرف خواهد کرد |
| A04 | two coincident slots for same Facility | conflict at authoring |
| A05 | contiguous slots [10,11),[11,12) | permitted |
| A06 | DST local ambiguous time | reject or resolve with explicit offset; never silently guess |
| A07 | lounge pass capped per traveller/visit | PassengerUsageLimit + AirportSlot separate |
| A08 | Facility sold out، AirportId same | other Facility still bookable |
| D01 | quota daily 20/date 2026-10-10 | unique DateOnly bucket |
| D02 | quota daily next date 2026-10-11 | independent bucket |
| D03 | one request bulk 365 dates | 365 **capacity** buckets، not one shared counter |
| D04 | missing daily bucket | NotConfigured/Closed by policy، no unlimited fallback |
| H01 | Hotel 5 doubles on Night Oct10 | room-night unique key |
| H02 | sell Oct10-Oct12 | nights 10 and 11 consumed; not 12 (Phase3 contract) |
| H03 | one night 0 available | whole stay unavailable |
| H04 | rate Plan A/B same RoomType | share same physical room-night inventory |
| H05 | remote PMS authoritative | SupplierManaged, no duplicated local stock |
| H06 | guaranteed allotment to OTA | only allocated allotment local; not all hotel property inventory |
| H07 | room closure with rooms remaining | ClosedForSale preserved amount |
| P01 | per pax max one meal per flight | PassengerUsageLimit and FlightCount co-exist |
| P02 | same pax / two orders / same flight | future Phase3 require stable identity; scope contract explicit |
| P03 | different pax same flight | no false duplicate |
| P04 | same pax different flight | independent usage limit |
| P05 | named guest entitlement 3 lounge visits | delegated entitlement or future separate ledger; no stock spoof |
| R01 | named vehicle occupied on interval | exclusive asset reserved in Phase3; no overlap |
| R02 | neighboring asset intervals | allowed |
| R03 | no confirmed business need for assets | not build speculative asset aggregate |
| X01 | two API requests update same capacity | exactly one succeeds with expectedVersion |
| X02 | negative capacity / invalid times / wrong ids | 422/409 domain errors |
| X03 | batch create with partial data error | atomicity per declared command, no partial silent acceptance |
| X04 | remove/retire active source with obligations | reject/cutover and reconcile |
| X05 | command/read projection | fields/types, exact keys, audit lines match |
| X06 | existing M1 Hold/Confirm regressions | no touched Reservation files; tests remain passing |
| X07 | no changes Ordering/FlightFlow/JetPay/AirPrice | repo diff proves |
| X08 | capacity Get before Phase3 | clearly non-guaranteed config snapshot |
| X09 | 31 legacy definitions lacking policy | migration exceptions reported; no inferred unlimited |
| X10 | Inventory policy for same identity duplicated | unique key + concurrency guard |
| X11 | supplier unavailable/timeout | Unknown/SupplierCheckRequired، no false stock |
| X12 | FlightFlow seat occupied in seatmap | no ANC-local available override |

## تست‌های استرسی اجباری در طراحی Phase 3، نه تکلیف ساخت حالای Phase 2

- 100 concurrent holds for Flight Count 10; exactly <=10 accepted.
- Decimal weight 100 kg with 50 parallel 5kg requests; accepted total <=100.
- Mixed Count+Weight flight resources and one failure -> zero leaked holds.
- Multi-night room atomicity with one blocked night and 100 concurrent requests.
- Lounge 90-minute occupancy overlapping 3 buckets with 100 concurrent visitors.
- One passenger attempts same service in two Orders concurrently.
- TTL expiry and confirm vs expire vs cancel race; exactly-once release.
- Provider snapshot stale / external hold fails after availability read; no fake guarantee.

## Phase 2 GO/NO-GO

- Owner approved mode/model matrix, fields and scope uniqueness.
- All active local type configurations persisted in CommandDb/QueryDb with migrations and audit.
- Required 50+ non-deferred scenario IDs mapped to tests and passing; intentionally deferred scenarios explicitly labeled rather than silently skipped.
- No Hold/Confirm/Release/Expire or Order integration implemented.
- Repo diffs prove old reservation and other microservices unchanged.
- Failing provider external is `Unknown`, no fallback.
- Role/tenant authorization and concurrency errors stable.
- Production readiness for limited inventory is **NO-GO** until Phase 3 allocation integration and cutover reconciliation tests pass.
