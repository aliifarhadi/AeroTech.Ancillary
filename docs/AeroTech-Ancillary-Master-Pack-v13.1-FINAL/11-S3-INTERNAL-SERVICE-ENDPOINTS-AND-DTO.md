# 11 — قرارداد نهایی Service-to-Service Ancillary (Stage 3)

تمام مسیرهای این فصل صرفاً برای Caller سرویس معتبر (`Ordering`/approved internal provider) هستند؛ هیچ `Api`/`OtaPanel` اجازه فراخوانی مستقیم آنها را ندارد. Prefix فعلی: `Service/v1/Ancillaries`.

## 11.1 Endpoint inventory
| عمل | Endpoint | وضعیت | هدف |
|---|---|---|---|
| Resolve selections | `POST /Service/v1/Ancillaries/Selections/Resolve` | `[TARGET]` | snapshot price/options/scope before hold؛ no reservation |
| Hold Group | `POST /Service/v1/Ancillaries/Service-Holds` | `[CURRENT]` route، `[TARGET]` payload/behavior | batch چند selected services |
| Query Hold | `GET /Service/v1/Ancillaries/Service-Holds/{holdId}` | `[CURRENT]` route، `[TARGET]` enriched response | per unit status + refs + evidence |
| Confirm | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations` | `[CURRENT]` route، `[TARGET]` truthful | commit only held/eligible units |
| Release All | `DELETE /Service/v1/Ancillaries/Service-Holds/{holdId}` | `[TARGET]` | release all held units only |
| Release Selected | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Releases` | `[TARGET]` | body `unitIds[]`, all when omitted if explicit policy |
| Extend Hold | `PATCH /Service/v1/Ancillaries/Service-Holds/{holdId}` | `[TARGET]` | `{expiresAt,idempotencyKey}` under provider capability |
| Retry unresolved | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Retries` | `[TARGET]` | `{unitIds[],idempotencyKey}`, same physical unit refs |
| Cancellation Quote | `POST /Service/v1/Ancillaries/Service-Confirmations/{holdId}/CancellationQuotes` | `[TARGET]` | read-only per unit penalty/supplier eligibility |
| Cancel Confirmed | `POST /Service/v1/Ancillaries/Service-Confirmations/{holdId}/Cancellations` | `[TARGET]` | quoteRef+unitIds+reason, cancel selected after check |
| Manual/Supplier outcome | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Units/{unitId}/ProviderOutcomes` | `[TARGET]` strictly internal | authenticated supplier/manual receipt/evidence/reconciliation |

**قرارداد API**: ۱۱ عملیات؛ هیچ `/Baggage/Holds`/`Meal/Confirm` یا aggregate جدید. Expand endpoint only for a proven new provider use case.

## 11.2 Hold request **wire contract [TARGET]**
```json
{
  "idempotencyKey": "ORD-81235-ANC-HOLD-v1",
  "orderId": 81235,
  "reference": "ORDER-81235",
  "requestedExpiresAt": "2026-10-16T19:00:00Z",
  "services": [
    {"orderServiceId": 891, "pricedSelectionRef": "psel-bag-1", "travellerId": 154, "coverageScope": "Portion", "coveredFlightIds": [8001,8002]},
    {"orderServiceId": 892, "pricedSelectionRef": "psel-cip-order", "travellerId": null, "coverageScope": "Order", "coveredFlightIds": []}
  ]
}
```
در Code فعلی `AncillaryHoldServiceInput` دارای `OrderServiceId,ServiceDefinitionId,ProvisionId,TravellerId,CoverageScope,CoveredFlightIds,Quantity` است؛ Target جدید: **`PricedSelectionRef` الزامی و مرجع trusted قیمت/Qty/Options**. Body `ServiceDefinitionId/ProvisionId/Quantity` قدیمی اگر باقی ماند صرفاً cross-check می‌شود، حق override Resolve ندارد. نیاز به `OrderId` فعلی حفظ. Request بدون `OrderId` در فاز ۳ Hold نمی‌شود؛ Shopping preOrder بدون OrderId آزاد است.

## 11.3 Hold/Status response **wire contract [TARGET]**
```json
{
  "holdId": 11882,
  "orderId": 81235,
  "reference": "ORDER-81235",
  "idempotencyKey": "ORD-81235-ANC-HOLD-v1",
  "status": "Pending",
  "outcome": "Partial",
  "expiresAt": "2026-10-16T19:00:00Z",
  "units": [
    {"unitId": 11901,"orderServiceId": 891,"pricedSelectionRef":"psel-bag-1",
     "status":"Held","quantity":2,"currencyId":70,"totalAmount":"1800000",
     "fulfillmentProviderKey":"LOCAL_BAG","providerUnitRef":null,"retryable":false},
    {"unitId":11902,"orderServiceId":892,"pricedSelectionRef":"psel-cip-order",
     "status":"Pending","pendingOperation":"Hold","pendingReasonCode":"ManualConfirmation",
     "quantity":1,"providerUnitRef":null,"retryable":true}
  ]
}
```
مقادیر نمونه و نام Provider illustrative هستند؛ External ProviderRef و Internal status فقط S2S. `GET Hold` همین Snapshot را با evidence و history/reconciled status می‌دهد؛ Caller پس از timeout باید GET/reconcile کند، نه تکرار کورکورانه درخواست Supplier.

## 11.4 Confirm/Release/Cancel bodies
- `POST .../Confirmations`: Body `{idempotencyKey, unitIds?:long[], paymentOrGuaranteeReference?, externalSeatEvidenceRefs?:[]}`؛ null/empty `unitIds` = همه Unitهای Held، اما اگر mixed result پس باید response per Unit بازگردد؛ هر evidence از Caller trusted و قابل اعتبارسنجی باشد. در صورت حفظ route بدون Body، compatibility با command فعلی برقرار بماند، ولی no fake confirm.
- `DELETE .../Service-Holds/{id}`: release all (reason optional sourced from Order). `POST .../Releases`: `{idempotencyKey,unitIds:[...],reasonCode}`. `PATCH`: `{idempotencyKey,expiresAt}`، supplier TTL مطابقت.
- `POST .../Retries`: `{idempotencyKey,unitIds:[...],reasonCode?}` فقط failed/pending قابل پیگیری؛ no duplicate expense.
- `POST .../CancellationQuotes`: `{orderId,unitIds:[...],reasonKind:"Voluntary|Involuntary|SupplierFailure|NoShow",disruptionRef?,requestedAtUtc?}`؛ `requestedAtUtc` از server trusted است نه تاریخ جعلی Caller.
- `POST .../Cancellations`: `{idempotencyKey,orderId,unitIds:[...],cancellationQuoteRef,reasonKind,waiverRef?}`؛ Quote valid برای همین Group/Unit/Reason/Order، otherwise 409.
- `POST .../ProviderOutcomes`: `{providerOperationRef,result,providerUnitRef?,evidenceAtUtc,providerDecisionCode?,sourceKind}` authenticated internal provider/manual agent only.

## 11.5 CancellationQuote result [TARGET]
`CancellationQuoteRef:string`, `HoldId:long`, `OrderId:long`, `IssuedAtUtc`, `ValidUntilUtc`, `ReasonKind`, `OverallQuoteState`, `Units[]`:
`{UnitId, OrderServiceId, CanCancel, CanRefund, QuoteState, GrossServiceAmount?, CurrencyId?, FiledPenaltyAmount?, SupplierPenaltyAmount?, EligibleTaxComponents[], NonRefundableComponents[], WaiverApplied, WaiverEvidenceRef?, ProviderQuoteRef?, RuleSourceRef?, ReasonCodes[], RequiresManualReview}`.

**اصل:** `NetCustomerRefund` در این پاسخ **نیست**؛ Ordering باید Payment/EMD/Coupon/agency settlement را بررسی و quote نهایی Refund بسازد. `EligibleRefundAmount` اگر Ancillary ارائه می‌کند «service-component estimate» با qualifier است نه دستور پرداخت.

## 11.6 HTTP/Domain errors
- 400/422 invalid selection, quantity, coverage, field, provider capability.
- 401/403 unauthorized Service caller, wrong OwnerAirline/Order binding.
- 404 unknown Hold/Unit/selection/quote; ID متعلق به Caller دیگر و unauthorized must not leak existence.
- 409 expired/stale quote, conflict on IdempotencyKey, invalid unit transition, changed amount, stock contention.
- 422 manual/provider quote unknown (business result `RequiresManualReview`) بدون pretend success.
- 502/503 supplier outage: if action side effect may have occurred, persist Pending/Reconciliation and provide status; response HTTP به تنهایی حقیقت Outcome Supplier نیست.

از کدهای error موجود Ancillary استفاده کنید؛ برای کد جدید exact numeric اختراع نکنید مگر در قرارداد API مصوب مرحله ۳ با تعریف enum/error catalog واحد. در هر خطا `correlationId`, `reasonCode`, `unitId?`, `retryable` در سطوح مجاز قابل تشخیص باشد.
