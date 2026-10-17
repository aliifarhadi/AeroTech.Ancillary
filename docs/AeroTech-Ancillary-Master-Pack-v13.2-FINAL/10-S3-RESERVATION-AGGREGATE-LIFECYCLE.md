# 10 — Stage 3: دامنه Reservation، فیلدهای گروه و Unit و چرخه عمر

## 10.1 فقط Aggregate فعلی توسعه می‌یابد
`AncillaryReservation` و `AncillaryReservationUnit` در Repo موجودند و فعلاً از OrderService/Definition/Provision/Quantity یک Hold تجاری می‌سازند. `Confirm` همه Units را صرفاً به status Confirmed منتقل می‌کند؛ External supplier و stock واقعی را انجام نمی‌دهد. قرارداد جدید **نسخه تکمیل‌شده همان مدل** است، نه دومین Aggregate یا ReservationPreparation.

## 10.2 Header `AncillaryReservation` — فیلدهای الزامی
| Field | Type | Rule |
|---|---|---|
| `Id` (`HoldId`) | `long` | Group reference برگشتی به Ordering |
| `OrderId` | `long` required | verified Owner Ordering |
| `IdempotencyKey` | `string` required, max فعلی 128 | Scoped by Order/operation؛ same key same canonical payload ⇒ replay |
| `Reference` | `string` required | client Order correlation، نه PNR/EMD |
| `RequestedExpiresAt`, `ExpiresAt` | `DateTimeOffset?` | حداکثر واقعی supplier/local TTL؛ null تنها اگر Business rule اجازه دهد |
| `Status` | `AncillaryReservationStatus` | derived from Units، هرگز Held/Confirmed ساختگی |
| `Outcome` | `Complete|Partial|Pending|Failed` response projection | برای batch mixed، نه جایگزین Unit truth |
| `CreatedAt`, `UpdatedAt` | `DateTimeOffset` | audit |
| `Units` | `List<AncillaryReservationUnit>` | حداقل یک Unit واقعی؛ OrderService duplicate guard |

## 10.3 `AncillaryReservationUnit` — هر خرید عملیاتی مستقل
| Field | Type | Rule |
|---|---|---|
| `Id` (`UnitId`) | `long` | مرجع جزء به جزء برای Confirm/Cancel/Retry |
| `OrderServiceId` | `long` | یک selected service از Ordering؛ هر واحد ناشی از یک selection دقیقا map شود |
| `PricedSelectionRef` | `string` | resolved & revalidated؛ never arbitrary price from Caller |
| `DefinitionVersionId`, `ServiceDefinitionId`, `ProvisionId`, `PricingRevisionId?` | `long` | snapshot of rule identity |
| `TravellerId` | `long?` | null only when valid Order scope |
| `BeneficiaryTravellerIds` | `long[]?` only if legitimately shared purchase | بدون Duplication قیمت؛ عدم نیاز برای تک‌مسافر/Order-simple |
| `CoverageScope` | enum | Sector/Portion/Journey/Order مطابق priced selection |
| `CoveredFlightIds` | `long[]` | Count/scope requirements؛ Order-only may empty |
| `Quantity` | `int` | هماهنگ با `Provision.Quantity` فعلی؛ وزن اندازه‌گیری‌شده جدا decimal |
| `SelectedOptions` | existing typed selection normalized | SeatNumber/MealCode/Package/Weight/Facility/Plan/... |
| `CurrencyId`, `PriceOrigin`, `UnitAmount?`, `TotalAmount?` | int/enum/decimal? | source-verified money; not client supplied |
| `TaxFeeComponents`, `FXEvidence?` | typed existing | برای reconciliation و Order totals |
| `FulfillmentProviderKey` | string | immutable پس از ایجاد Unit |
| `ProviderUnitRef?`, `StockPoolId?` | string?/long? | تنها پس از واقعی شدن commitment |
| `SupplierActionRef?`, `ProviderEvidenceAtUtc?` | string?/DateTimeOffset? | webhook/poll/manual trace |
| `Status` | unit status | حقیقی؛ با Pending pending operation |
| `PendingOperation?`, `PendingReasonCode?` | string? constrained | Hold/Confirm/Release/Cancel؛ Supplier/Manual/Reconcile |
| `FailureReasonCode?`, `IsRetryable` | string?/bool | بدون نمایش جزئیات حساس Supplier |
| `CreatedAt`, `UpdatedAt` | DateTimeOffset | trace |

برخی فیلدهای Unit فعلی (`StockPoolId?`, `ProviderUnitRef?`) وجود دارند؛ دو بار نسازید. `QuantityRule.Unit` Enum `Each/Piece/Kilogram` و `QuantityRule.Min/Max:int` است؛ وزن `MeasuredWeightKg` decimal Selection است، نه Quantity int را به decimal تغییر بدهیم مگر Rule واقعی لازم شود.

## 10.4 Statusها و semantics بدون دروغ
**Existing numeric values حفظ می‌شوند:** `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` برای گروه و Unit.

**افزودنی حداقلی P3 فقط برای ثبت نتیجه‌نامعلوم واقعی:**
- `AncillaryReservationUnitStatus.Requested=6`, `Pending=7`, `Failed=8`؛ `PendingOperation` و `PendingReasonCode` نوع انتظار را روشن می‌کنند؛ PendingManual/Unknown دو state entity مستقل نمی‌خواهند.
- `AncillaryReservationStatus.Pending=6`, `Mixed=7`, `Failed=8`؛ وضعیت گروه از Units به‌روز می‌شود: تمام Held ⇒ Held؛ تمام Confirmed ⇒ Confirmed؛ تمام Released ⇒ Released؛ تمام Cancelled ⇒ Cancelled؛ تمام Expired ⇒ Expired؛ any Pending/Requested ⇒ Pending؛ ترکیب کارهای کامل/ناموفق ⇒ Mixed؛ all Failed ⇒ Failed. `Outcome` مکمل برای Partial success.

این **پیشنهاد قطعی افزودنی به enumهای Ancillary در Stage 3** است؛ نه ادعای وجود در کد فعلی. Existing status numberها هرگز عوض نشوند. اگر EF Framework روش دیگری برای Derived Status دارد، equivalent با همین behavior قابل قبول است؛ دو Status منبع تناقض نسازید.

## 10.5 Operation semantics
| Command | Preconditions | Per-unit effect | Idempotency / failure |
|---|---|---|---|
| Hold | verified OrderService + pricedSelectionRef + valid offer/amount/stock/policy | Requested→Held یا Pending/Failed | same request replay; same key different data 409 |
| Confirm | unit Held و payment/authority gate از Ordering تأیید | Held→Confirmed (یا Pending when provider pending) | repeat confirmed returns same refs |
| Release | unit Held و not confirmed | Held→Released، recover resource/provider hold | unknown supplier release ⇒ Pending، not released locally falsely |
| Extend | unit Held با capability تمدید و Rule expiry | update effective expiry فقط پس از provider acceptance | no TTL beyond source rule |
| Expire | clock >= expiresAt، برای held units | Expired و آزادسازی/reconciliation | expired unit never confirmed |
| Retry | only Failed(retryable)/Pending after reconciliation | same unit/provider idempotency | no duplicate booking; no new commercial charge |
| Cancel | confirmed selected units, valid quote/authority | Confirmed→Cancelled only after supplier/local result | void/refund not inside Ancillary |
| Cancel Partial | selected `UnitIds[]` belong same Order/Hold | فقط Units مربوط تغییر می‌کنند | mixed group outcome authentic |

`Confirmed -> Released` و `Cancelled -> Confirmed` ممنوع. لغو بعد استفاده خدمت بدون waiver/Provider rule مسدود است؛ `NoShow` و involuntary رویدادهای مستقل دارند. **هیچ Confirm group کلّی مادامی که حتی یک Unit نامعلوم است گزارش نشود.**

## 10.6 Multi-unit semantics
- یک request می‌تواند A01 برای Pax1 Qty2 + A12 برای Pax2 + A22 Order-scoped را با هم داشته باشد؛ هر کدام Unit مستقل.
- چند انتخاب متفاوت یک Definition با Scope یکسان (مثلاً دو Menu یا دو Baggage Option) مجاز هستند اگر `OrderServiceId`, `PricedSelectionRef` و Domain uniqueness درست باشد؛ Code فعلی که `Definition+Coverage` را unique فرض می‌کند باید اصلاح شود.
- یک انتخاب واحد با همان OrderService نباید همزمان دوبار reserved شود؛ تنها راه retry، عملیات idempotent همان Unit است.
- Group reference `HoldId` متعلق به Ancillary است، `OrderId` متعلق به Ordering، `ProviderUnitRef` متعلق به Supplier، `SeatHoldReference` متعلق به FlightFlow. هیچ کدام را جای دیگری ننویسید.

## 10.7 Seat specific
`AncillaryReservationUnit` برای A07/A08 هزینه/حق تجاری را Hold/Confirm می‌کند ولی **انجام Hold فیزیکی را ادعا نمی‌کند**. Ordering برای same Traveller/Flight/Seat باید FlightFlow `Seat-Holds` با `SeatHoldReference` دریافت کرده باشد و فقط با شاهد آن، Confirm نهایی مجاز است. Ancillary FlightFlow Seat inventory را تخصیص نمی‌دهد. A09 EXST بسته به ticket/adjacent-seat، نیاز به co-confirm چند رفرنس physical و ticket action دارد؛ انجامش در Ordering است.

## خروجی
همان Aggregate موجود با truthful گروه/Unit، complete/partial/pending و امکان recovery و cancel جزئی؛ بدون invent کردن یک ReservationCoordinator داخل Ancillary.
