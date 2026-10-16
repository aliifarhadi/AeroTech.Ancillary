# 05 — قواعد تجاری Stage 1 و مرز Stock / Supplier

## Rule publication and eligibility
- Definition باید Active باشد؛ Provision باید فعال/منتشرشده و برای Order/Fare/Passenger/Route/POS/Date قابل اعمال باشد؛ Pricing نسخه فعال/قابل محاسبه داشته باشد مگر `PriceOrigin.Free` با evidence. ترکیب ناسازگار نباید Shopping Offer قابل خرید بدهد.
- Null/empty `SalesRestrictions.PointsOfSale` یعنی unrestricted طبق تصمیم commit فعلی؛ مقادیر غیرخالی دقیقاً با trusted eligible POS match شوند. این به معنی دادن POS دلخواه به Agency نیست.
- `TravelDate` با permitted/blackout date periods، `DayTime` با local time/Timezone، `AdvancePurchase` با lead time، `FlightApplication` با operating marketing facts، `FareApplication` با FareBasis/FareFamily/RBD/cabin/benefits، `PassengerEligibility` با PTC/age/other approved facts.
- برای موارد مشمول محدودیت seat exit-row یا PET/UMNR documents، Eligibility = Blocked/NeedsVerification تا fact واقعی فراهم شود؛ راه میانبرِ تیک «قبول قوانین» جایگزین تایید واقعی eligibility نیست.

## نرخ و Quantity
- `PricingUnit` تعریف می‌کند واحد پول چیست؛ `CoverageScope` تعیین می‌کند آن واحد به کدام پرواز/مسافر/سفارش مربوط است؛ `AncillaryQuantityUnit` تعریف Quantity را مشخص می‌کند. PerPiece ≠ PerKilogram ≠ PerPassenger؛ مقدار معادل بسته وزنی PerItem ممکن است متفاوت باشد.
- `CompleteUnitTotal` شامل نرخ پایه و tax/fee applicable (بدون Feeهای خارج از واحد)؛ `RequestedQuantityTotal` فقط پس از Quantity واقعی و verified، با Tax/Fee Apply درست. Order level fee فقط یک بار در Order جمع می‌شود، نه روی هر Pax/Leg.
- Currency code/decimal places/FX reference باید مشخص باشد. کد Tax، کشور و ایستگاه را در صورت موجود بودن نگه دارید. همواره همان published price revision و applied rate/FX را در `PricedSelectionSnapshot` ضبط کنید.
- `PriceOrigin=ExternalQuote`: quote معتبر ProviderKey/QuoteRef و TTL لازم؛ `Free`: فقط declaration و eligibility معتبر؛ `NotAvailable`: هرگز خودکار 0؛ `Filed`: rate منتشرشده، currency دقیق.
- Commissionable flag، form-of-refund و settlement rule در Ancillary metadata است. مبلغ واقعی Agency commission، Agent fee، Markup/Discount و Payment fee با ترکیب سياست فروش و تعهد مالی در Ordering/Payment تسویه می‌شود. قیمت نمایش‌داده‌شده باید fee application را دقیق و از accounting جدا نگه دارد.

## Stock authority matrix
| Source/Policy | Shopping status | Hold Stage 3 | اصل |
|---|---|---|---|
| Seat (A07/08/09) | `NotApplicable` به StockPolicy داخلی | Ancillary فقط commercial؛ Seat physical در FlightFlow | هیچ ظرفیت/Policy Seat داخلی |
| Local finite Count | Count bounded و verified | unit quantity decrement/hold با منبع معتبر | یک واحد ظرفیت دوبار Hold نشود |
| Local Weight | وزن واقعی/صحت واحد کیلوگرم | وزن Hold/Release با conservation | PerKilogram price با وزن واقعی مرتبط |
| Local AirportSlot | slot/window facility verified | تخصیص interval/count واقعی | تداخل زمانی و Zone مشخص |
| Explicit Unlimited | Unlimited فقط وقتی Policy **فعّال و صریح** | no local consumption، commercial hold | غیاب Policy Unlimited نیست |
| Supplier delegated | availability unknown/needs verification مگر source authoritative | Supplier API/manual capability | response نامعلوم sold-out یا free نیست |
| Missing local configuration | `NotConfigured`/`NeedsVerification` | Hold reject/pending بر اساس rule | ساختگی Count=∞ ممنوع |

`InventoryAuthority` جاری `Unlimited=1`, `Local=2`, `Supplier=3`. نه `FlightFlow=4` و نه renumber. `InventoryCapacityReadState.NotApplicable=8` برای Seat مجاز است. `FlightCountInventory` و `FlightWeightInventory` بی‌دلیل حذف نشوند.

## Filed cancellation rule در مرحله S1
`Settlement.ReissueRefund` فقط Refundability را می‌گوید. برای تعیین جریمه داخل Carrier **در Stage 1** فهرست اختیاری `CancellationRules` در ValueObject Provision و دو Endpoint مشترک `GET/PUT /Backoffice/v1/AncillaryProvisions/{id}/CancellationTerms` ایجاد شود؛ Seat Form موجود بدون تغییر بماند. `FixedAmount`, `Percentage`, `ProviderQuoted`, `ManualReview` و RuleRef/Source در فصل 13 و enumهای فصل 21 هستند. این توسعه به‌معنی Refund مالی داخل Ancillary نیست. برای Supplier با quote، مبلغ فایل نشود. بدون Rule/Provider معتبر در Stage 3 باید ManualReview شود.

## Supplier capability (TARGET metadata on verified provider setup)
| Capability | شرح | Rule |
|---|---|---|
| `HoldConfirm` | provider قابلیت reserve/confirm جدا دارد | پیش از Hold موفق، provider hold ref را ذخیره کن |
| `DirectConfirm` | provider فقط خرید/تأیید مستقیم دارد | در مرحله Hold نتیجه «PendingCommit»/pending به caller؛ پس از verified payment orchestration commit؛ هرگز fake Held |
| `ManualConfirmation` | تماس/manual/supplier panel | task + Pending + evidence؛ Confirm تنها پس از evidence |
| `NoBookingProcessRequired` | entitlement/خدمت بدون عملیات supplier | Commercial-only record و وضعیت شفاف |

از `SupplierFulfillmentKind.Local=1 / External=2` موجود استفاده کنید؛ Capability زیرمجموعه مشخصات Provider است، نه «SupplierKind» موازی. Capability صرفاً وقتی adapter واقعی یا قرارداد business تایید شود فعال گردد. Unknown Provider ⇒ blocked/error, not success.

## Child/family, agency and supplier constraints
- OwnerAirlineId، SupplierId و FulfillmentProviderKey trusted و تغییرناپذیر پس از Hold؛ cross-airline provisioning رد شود.
- Included baggage allowance در AirAvail coupon داده می‌شود؛ Extra checked bags و overweight باید با همان allowance و خریدهای موجود reconcile شوند؛ فروش مجدد حق پایه ممنوع.
- Pet cabin/hold، wheelchair و medical equipment تابع operating airline، محدودیت route/animal/certification/age/SSR واقعی‌اند. نیاز به تایید دستی را به «موجودی نامحدود» تبدیل نکنید.
- Involuntary flight disruption، no-show، partially flown coupon، interline، ancillary provided-by-third-party همگی Reason/Provider context دارند، نه hardcoded blanket refund.

## Output
قواعد Definition/Provision/Pricing قابل انتشار و ارزیابی‌اند؛ nonSeat InventoryPolicy پابرجا؛ Seat Pricing مستقل از StockPolicy؛ هیچ external authority «آزاد/تایید» فرض نشود.
