# 12 — Stage 3: Provider Strategies و رفتار واقعی ظرفیت، عملیات دستی و جبران

## 12.1 ProviderCapability per ServiceDefinition/Provision
Strategy یک‌بار بر اساس trusted `FulfillmentProviderKey`، Supplier و Provision resolved می‌شود. `SupplierFulfillmentKind` فعلی `Local=1`, `External=2` کافی برای مالکیت است ولی Capability را به‌تنهایی بیان نمی‌کند؛ Capability جدول زیر از Provider معتبر خوانده/پیکربندی می‌شود، **نه از Body Agency**.

| Capability | Hold | Confirm | Release/Cancel | Reference/Evidence |
|---|---|---|---|---|
| `LocalFiniteCount` | decrement/reserve quantity واقعی | commit allocation | recover counted units / restock if permitted | `StockPoolId`, UnitId ledger |
| `LocalFiniteWeight` | reserve verified kg | commit measured weight | return kg if allowed | immutable measured weight |
| `LocalAirportSlot` | book time interval/quota | confirm slot | cancel/release slot according to policy | slot/time/location ref |
| `ExplicitUnlimited` | تجاری، بدون stock subtraction | confirm entitlement | cancel entitlement | reason, no fake stock count |
| `ExternalHoldConfirm` | provider hold receipt | provider confirm receipt | provider release/cancel if available | ProviderUnitRef/ProviderOperationRef |
| `ExternalDirectConfirm` | no external hold: PendingCommit | after verified guaranteed payment/approval provider commit | cancel only if provider supports | truthful tentative state, operational risk |
| `ManualConfirmation` | PendingManual + operator work item | confirmed **only with verified manual result** | manual cancel with evidence | agent/time/supplier proof |
| `NoBookingProcessRequired` | commercial record; eligible | confirm after allowed Order/payment gate | status reversal only | no fictitious supplier hold |
| `Seat` | commercial price/selection only | requires coordinator's physical FlightFlow evidence | commercial cancel; physical in FlightFlow | no Ancillary physical seat stock |

## 12.2 قوانین شکست/جبران
- یک Unit محلی به‌صورت Atomic و idempotent ظرفیت مصرف می‌کند؛ در ۲ درخواست همزمان بیش از ظرفیت معتبر صادر نمی‌شود. Release/Cancel دوباره نباید دو بار ظرفیت برگرداند.
- در Supplier API call پیش از جواب ممکن است Provider عملیات را انجام داده باشد. Timeout ⇒ `Pending` و `PendingReasonCode=Reconciliation`؛ retry با **همان** Provider operation key و GET/status پیش از ایجاد booking دوم. وضعیت «موفق» یا «شکست قطعی» جعل نشود.
- اگر Group شامل ۳ Supplier است و یکی failure قطعی دارد، موفقیت بقیه با Unit refs حفظ می‌شود؛ Ordering تصمیم به compensation/retry می‌گیرد. درخواست Group به هیچ وجه یک transaction مشترک بانکی/هواپیمایی نیست.
- اگر Confirm از لحاظ Provider ممکن نیست اما Payment guarantee معتبر است، تا دریافت ProviderResult `Pending` است؛ Payment captured الزاماً به معنی Supplier Confirmed نیست و در صورت ناتوانی در fulfillment، Ordering باید refund/rebook/notification را مدیریت کند.
- اگر Provider اصلاً Hold را پشتیبانی نمی‌کند، Stage 3 با `ExternalDirectConfirm` و policy verified معامله می‌کند. برای paid item، Ordering قبل از commit باید payment authorization/guarantee مناسب و راه جبران داشته باشد؛ ترتیب برای Providerهای متفاوت قابل تنظیم است، نه hardcoded یک نسخه.
- Manual Supplier فقط توسط اپراتور مجاز می‌تواند evidence رویداد `ProviderOutcomes` بدهد؛ `PendingManual` در **PendingReason** است نه وضعیت مستقل اجباری. نبود evidence = Pending.
- اگر Confirm/Cancel دیرتر از expiry یا cancellationQuoteTTL برسد، authoritative recheck و تشخیص اثر انجام‌شده لازم است؛ side-effect بدون idempotency دوباره اجرا نشود.

## 12.3 Reservation references و partial operations
`HoldId` ۱ group؛ `UnitId` برای هر service؛ provider may have ۱ ProviderRef برای چند Unit، اما mapping Unit→ProviderRef explicit باشد و هیچ حدس زده نشود. `POST .../Releases` و `POST .../Cancellations` فقط روی UnitIdهای انتخاب‌شده اثر کنند. برای order-scoped CIP چند beneficiary مجاز اگر supplier policy اجازه دهد، یک Unit ممکن است چند covered persons داشته باشد؛ Qty/Price فقط یک‌بار ثبت شود.

## 12.4 Inventory source missing
- Missing Policy برای nonSeat با availability-required ⇒ `NeedsVerification` در Shopping و Hold noncommittal.
- Local/External provider unavailable ⇒ `Pending` یا `Failed` با reason، نه Unlimited.
- A07/A08/A09 Seat هیچ `AncillaryInventoryPolicy` ندارد؛ تصویر صندلی و رزرو physical در FlightFlow است و در این سرویس دوباره کپی نمی‌شود.

## 12.5 Post-sale changes
افزودن Meal/Baggage به Order صادرشده، درخواست جدید در Ordering است که Ancillary را با Order commercial context re-shop می‌کند؛ Unit جدید Hold و Confirm می‌شود. حذف/تغییر انتخاب، واحد قدیمی Cancel + انتخاب جدید Hold/Confirm با اختلاف قیمت/Refund جدا، بدون mutation تاریخچه اولیه. Flight disruption/irregular ops/aircraft swap ممکن است Availability و pricing قبلی را باطل کند؛ Owner orchestrator تکلیف Unit/EMD/physical assignment را هماهنگ می‌کند.

## 12.6 Event and eventual consistency contract
از الگوهای Event/Outbox فعلی سرویس پیروی شود؛ دامین Eventهای موردنیاز S3 با پیشنهاد نام:
`AncillaryHoldOutcomeChanged`, `AncillaryUnitConfirmed`, `AncillaryUnitReleased`, `AncillaryUnitCancelled`, `AncillaryUnitProviderReconciliationRequired`.

حداقل payload همه: `EventId`, `OccurredAtUtc`, `CorrelationId`, `OrderId`, `HoldId`, `UnitId`, `OrderServiceId`, `Status`, `ReasonCode?`, `ProviderUnitRef?` فقط Service scope مجاز، `Quantity`, `EffectiveAt` در صورت نیاز. `OrderPaymentStatus` یا EMD number از Ancillary صادر نشود مگر fact از مالک آن دریافت شده و لازم برای correlation باشد. **اینها Target event names هستند؛ وجودشان در فعلی ادعا نمی‌شود.**
