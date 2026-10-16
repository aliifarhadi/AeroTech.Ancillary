# 13 — End-to-End Servicing: Cancel، Penalty، Refund، Voucher، Void و EMD

## 13.1 چهار عمل مستقل
1. **`Release`**: Hold هنوز Confirm نشده را آزاد می‌کند؛ محاسبه Refund الزامی نیست مگر Provider هزینه cancellation/late release معتبر داشته باشد و Ordering واقعاً وجه دریافت کرده باشد.
2. **`Cancel`**: entitlement/reservation تاییدشده را طبق شرایط Supplier/Carrier لغو می‌کند. لزوماً حق Refund ایجاد نمی‌کند.
3. **`Void/Refund Document`**: Ticketing/Ordering روی `ETKT`, `EMD-A`, `EMD-S` و coupon انجام می‌دهد؛ Ancillary EMD صادر یا Refund نمی‌کند.
4. **`Refund Payment/Wallet/Cash/Credit`**: فقط Payment/JetPay و Accounting/Ordering با مرجع پرداخت واقعی. این مرحله پس از Eligibility و تنظیم Document انجام می‌شود، یا طبق workflow تامین‌کننده و payment method با جبران معتبر؛ نتیجه مالی مستقل از لغو عملیاتی است.

## 13.2 چرا Settlement موجود کافی نیست؟
در کد `SettlementDefinition` فقط `ReissueRefund:ReissueRefundPolicy` (`Refundable=1`, `NonRefundable=2`, `NonRefundableReusable=3`)، `FormOfRefund` (`OriginalPayment=1`, `EVoucher=2`)، `Commissionable:bool`, `InterlineSettlement:bool` دارد. **این فیلدها مبلغ Penalty یا Tax refundable را تعریف نمی‌کنند.** بنابراین از `Refundable` نباید Penalty=0 یا 100% return حدس زد.

### کمینه توسعه `[TARGET]`
- از `ReissueRefundPolicy` و `FormOfRefund` فعلی برای eligibility/base disposition استفاده شود؛ بدون ساخت «RefundAggregate» داخل Ancillary.
- برای Carrier-filed cancellation fees، تنها یک Rule ValueObject **اختیاری** زیر Provision (مشترک با Settlement) کافی است (نه Aggregate جدید): `CancellationRules[]` که **مدیریت آن در Stage 1 از دو Endpoint مشترک `GET/PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` بسته می‌شود و فاز ۳ فقط آن را می‌خواند**، از `{RuleRef, ReasonKind, AppliesFromUtc?/BeforeStartMinutes?, AppliesUntilUtc?, NoShowApplicability?, PenaltyKind, PenaltyFixedAmount?, PenaltyPercentage?, PenaltyCurrencyId?, PercentageBase?, WaiverPermitted?, SourceReference?}`. لازم نیست همه فیلدها برای هر Variant پر شوند؛ هر Rule باید غیرمتناقض و قابل محاسبه باشد.
- `PenaltyKind` target constrained values: `None=1`, `FixedAmount=2`, `Percentage=3`, `ProviderQuoted=4`, `ManualReview=5`. اگر Provider معتبر quote مستقیم می‌دهد، `ProviderQuoted` انتخاب شود؛ اگر فقط Refundable flag داریم، **QuoteState=RequiresManualReview** نه Penalty=0.
- `ReasonKind` target values: `Voluntary=1`, `Involuntary=2`, `SupplierFailure=3`, `NoShow=4`; `WaiverRef` فقط با شاهد صادره از منبع معتبر airline/disruption، نه string آزاد آژانس.
- Tax refundability per `TaxTreatment` و authoritative issuance/coupon context جدا مشخص شود؛ مالیات را به‌صورت global refunded فرض نکنید.

## 13.3 دو Quote مجزا؛ حفظ مسئولیت مالی
### Quote 1 — **Ancillary Service Cancellation Quote** (S2S)
Endpoint: `POST /Service/v1/Ancillaries/Service-Confirmations/{holdId}/CancellationQuotes`.

**حداقل Unit detail:** `unitId`, `canCancel`, `canRefund`, `quoteState=Quoted|NotPermitted|NeedsManualReview`, `originalServiceAmount?`, `currencyId`, `penaltyAmount?`, `penaltyCurrencyId?`, `penaltyKind`, `providerCancellationCharge?`, `refundableServiceComponents[]`, `nonRefundableComponents[]`, `taxComponentDisposition[]`, `ruleRef?`, `providerQuoteRef?`, `waiverApplied`, `waiverEvidenceRef?`, `noShow/used evidence`, `quoteValidUntilUtc`, `reasonCodes[]`.

### Quote 2 — **Ordering Refund Quote** (مشتری/آژانس)
Ordering این Quote را با `OrderServiceAcceptedTotal`, coupon/EMD state, payment transactions, agent/customer/commission, prior refunds, currency/FX, wallet/agency credit, voucher option و caps ترکیب می‌کند. خروجی `netAmountToRefund`, `refundDestination`, `documentAction`, `serviceCancellationAction`, `penaltyBreakdown`, `isFinal/approvalNeeded` است. Service quote مستقیماً دستور Bank/Wallet refund نیست.

**قانون ساده:** `NetRefund = max(0, refundablePaidComponents - legitimatePenalties - nonRefundablePaidFees - priorRefundedAmount)` **فقط روی اقلامی که واقعاً قبلاً پرداخت شده‌اند** و پس از policy سند/Tax/commission. اگر Penalty جداگانه بیشتر از مبلغ استرداد است، هیچ مبلغ منفی را به Payment Refund نفرستید؛ هر بدهی جدید در Ordering/Payment فرآیند جدا دارد. مثال عددی به‌تنهایی قانون مالی همگانی نیست.

## 13.4 Flow عمومی کنسلی و Refund (تحت Ordering)
1. Surface مجاز ⇒ `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/CancellationQuotes` با `orderServiceIds[]`, `reasonKind`.
2. Ordering authorization/ownership+coupon/EMD/payment/usage validation؛ `UnitId[]` را از OrderService links به دست می‌آورد.
3. Ordering ⇒ Ancillary S2S `CancellationQuotes`؛ Ancillary Rule/Supplier quote را با current status/reason/expires بررسی می‌کند.
4. Ordering ⇒ Ticketing/Payment checks و یک **OrderCancellationQuoteRef** با مبلغ، جریمه، اسناد و destination ایجاد می‌کند؛ به Surface نشان می‌دهد. `ManualReview` به‌جای عدد نامعلوم.
5. User/Agency مجاز تأیید می‌کند: `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/Cancellations` با `orderCancellationQuoteRef`.
6. Ordering Quote، price/coupon/supplier status را revalidate و cancellation workflow را شروع می‌کند؛ Ancillary `.../Cancellations` فقط Units انتخاب‌شده را با `cancellationQuoteRef` معتبر لغو می‌کند؛ physical seat cancellation از FlightFlow به‌صورت جدا.
7. Document owner EMD/ETKT `Void` یا `Refund` / coupon adjustment را مطابق قواعد انجام می‌دهد؛ Payment Refund/Wallet/agency settlement توسط مسئول پول؛ نتیجه‌ها ممکن است async باشند. **ترتیب دقیق commit هر Provider طبق قابلیت جبران، pending/unknown evidence و سند است**؛ نباید فرض شود همگی atomically تغییر می‌کنند.
8. Ordering `OrderChange`, `OrderServiceStatus`, penalty/refund/payment refs و immutable history را ثبت و وضعیت قابل پیگیری به Surface می‌دهد.

## 13.5 سناریوها و احکام خاص
| سناریو | Service operation | Document/Financial | Rule |
|---|---|---|---|
| مشتری قبل Confirm انصراف دهد | Release Held | اگر وجه capture شده، مالی جدا | معمولاً EMD نداریم؛ supplier late-release fee فقط با evidence |
| مشتری بعد Confirm ولی قبل Issue انصراف دهد | Cancel confirmed | document yet absent؛ refundable amount ممکن | enforce provider policy و penalty |
| paid bag با EMD-A صادرشده | Cancel bag اگر مجاز | EMD-A coupon refund/void توسط Ticketing؛ Payment توسط owner | flight coupon state مهم |
| Lounge EMD-S | Cancel Supplier + entitlement | EMD-S standalone refund/void و payment | no flight coupon dependency unless rule says |
| Free SSR | Cancel SSR اگر ممکن | غالباً مبلغ refund صفر چون پرداخت صفر | Source Free روشن باشد |
| NonRefundable ticketed ancillary | Cancel ممکن یا ناممکن | `NetRefund=0` یا voucher طبق policy | `Refundable` ≠ `CanCancel` |
| Reusable non-refundable | Cancel/credit | فقط credit reuse با شرایط اصل قرارداد | cash refund جعل نشود |
| بعد از استفاده از سرویس | معمولاً blocked | penalty/refund طبق UseStatus/provider | صرف Paid ⇒ Refundable نیست |
| No-show | reason `NoShow` | filed/supplier rule/waiver | تاریخ قطعی سفر/airport time zone |
| Flight disruption involuntary | cancel/reaccommodate | waiver/EMD/Payment طبق verified disruption | reason/waiverRef لازم |
| Partial cancel یک Order | فقط UnitIdهای انتخابی | فقط اسناد/قیمت همان Units | سایر Services untouched |
| Payment با wallet/agency credit/cash | same service cancel | refund destination تابع funding liability | Ancillary payment model ندارد |
| supplier cancel timeout | PendingReconciliation | **پول خودکار refund قطعی نشود** تا service/document effect معلوم شود | recover idempotently |
| اختلاف نرخ پس از Quote | quote stale | درخواست تأیید جدید | بدون silent penalty increase |
| خرید پس از ticket به صورت ancillary add-on | عملیات OrderChange + Hold Confirm | EMD/Payment افزایشی | AirAvail priced offer اولیه ضروری نیست |
| refund فقط taxes (service nonrefundable) | supplier/document quote | tax component refundable موردی | Gross zero فرض نشود |

## 13.6 تفاوت EMD-A/EMD-S و A10
`AncillaryDocumentType` فعلی `None=1`, `EmdAssociated=2`, `EmdStandalone=3`; `DocumentRouting` `NoAncillaryDocument=1`, `Emd=2`, `TicketOrExchange=3`. A10 Cabin Upgrade گاهی تعویض بلیت می‌خواهد و نمی‌شود آن را به EMD عمومی تبدیل کرد؛ تصمیم doc routing از Definition و Ordering ticket context می‌آید. EMD-A به coupon/e-ticket مرتبط است؛ EMD-S standalone است. تغییر Service نباید بی‌اجازه خودکار coupon پرواز را Cancel کند؛ تغییر itinerary و involuntary disruption هماهنگ‌کننده Ordering دارد.

## 13.7 Refund quote expiry و double refund
- هر QuoteId به specific OrderServiceId+UnitId+reason+amount+document snapshot+recipient destination+expiry و user approval متصل است؛ دوباره استفاده پس از commit ممنوع.
- Ancillary Unit `Cancelled` به معنی `Refunded` نیست. Ordering transaction/PaymentRef/DocumentCoupon status را مستقل نگه می‌دارد؛ repeat request همان نتیجه را با idempotency برمی‌گرداند.
- Chargeback/failed refund/partial processor settlement به Payment/Ordering error/retry stream می‌رود؛ Ancillary status کاذب تغییر نکند.

## خروجی
Ancillary قادر به اعلام Service cancellation eligibility، penalty و اجرای cancellation واقعی است؛ کل چرخه مالی و EMD در Ownerهای درست مدیریت می‌شود، با quote دو مرحله‌ای مشابه الگوهای صنعت.
