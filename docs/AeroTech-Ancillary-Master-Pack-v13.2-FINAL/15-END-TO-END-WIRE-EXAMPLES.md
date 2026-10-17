# 15 — نمونه قراردادهای End-to-End (TARGET، نه ادعای وجود در کد)

تمام ID/Amountها صرفاً **مثال خوانایی** هستند. JSONهای زیر «شكل مطلوب Wire» را نشان می‌دهند؛ SeatMaps JSON عمداً بازنویسی نمی‌شود زیرا **خروجی واقعی آن در فصل 09 فریز است**.

## 15.1 Shopping فهرست محصول غیر Seat
```http
POST /Api/v1/AncillaryOffers
Content-Type: application/json
```
```json
{"offerId":"priced-air-offer-A"}
```
پاسخ شامل `offerItemRef` به ازای Candidate محصول و Scope مجاز، `selectionKind`, `priceStatus`, `currency/price`, `quantity min/max`, `offerReadiness` و `reasonCodes` است. Caller نیازی به `OrderId` ندارد؛ POP/POS را از Authorization دریافت می‌کند.

## 15.2 Details واقعی Paid Meal (A12)
```http
POST /Api/v1/AncillaryOffers/Details
```
```json
{"offerId":"priced-air-offer-A","offerItemRef":"offer-item-meal-ADT-F1"}
```
پاسخ **additive بخش مربوط به انتخاب**:
```json
{
  "offerItemRef":"offer-item-meal-ADT-F1",
  "profile":"Meal", "variantCode":"A12",
  "coverageScope":"Sector", "travellerRefs":["ADT-1"], "flightRefs":["F1"],
  "selectionContract":{"kind":"QuantityChoice","fields":[
    {"name":"MenuItemRef","type":"Code","required":true},
    {"name":"Quantity","type":"Integer","required":true}]},
  "optionGroups":[{"fieldName":"MenuItemRef","sourceState":"Known","choices":[
    {"optionRef":"opt-menu-VGML","value":"VGML","label":"Vegetarian meal","pricePreview":{"currencyId":70,"unitTotal":"250000","isComplete":true}},
    {"optionRef":"opt-menu-CHML","value":"CHML","label":"Children's meal","pricePreview":{"currencyId":70,"unitTotal":"180000","isComplete":true}}
  ]}],
  "quantity":{"unit":"Each","min":1,"max":4},
  "priceOrigin":"Filed"
}
```
قیمت/Choiceها فقط وقتی supplier/catalog معتبر واقعاً آن را بازگرداند. اگر Supplier option list ندارد، `optionGroups=[]`, `sourceState=Unknown/NotApplicable` و Typed fields/constraints کافی است؛ بستن فروش با صفر گزینه مجهول غلط است.

## 15.3 Evaluate انتخاب نهایی یک Ancillary
```http
POST /Api/v1/AncillaryOffers/Selections/Evaluate
```
```json
{"offerId":"priced-air-offer-A","offerItemRef":"offer-item-meal-ADT-F1","selection":{"menuItemRef":"VGML","quantity":2}}
```
پاسخ افزوده به DTO فعلی:
```json
{
  "status":"Accepted", "offerItemRef":"offer-item-meal-ADT-F1",
  "currentOfferItemRef":"offer-item-meal-ADT-F1",
  "pricedSelectionRef":"psel-891-2xVGML-r4",
  "price":{"currencyId":70,"completeUnitTotal":"250000","requestedQuantityTotal":"500000","isTotalComplete":true},
  "scope":{"travellerRefs":["ADT-1"],"flightRefs":["F1"],"coverageScope":"Sector"},
  "validUntilUtc":"2026-10-16T19:00:00Z", "fieldIssues":[]
}
```
اینجا `pricedSelectionRef` شکل **illustrative** دارد؛ در تولید opaque/random lookup key خواهد بود. `status=NeedsQuote/NeedsVerification/Rejected` ⇒ no purchasable pricedSelectionRef.

## 15.4 SeatMap بدون تغییر Output
```http
POST /OtaPanel/v1/SeatMaps
```
```json
{"offerId":"priced-air-offer-A"}
```
پاسخ دقیق DTO فعلی فصل 09 و Base Project است: `meta`, `data[].decks[].cabins[].items[].travelerPricing[].id/seatAvailabilityStatus/price`, `dictionaries`. خرید Seat در Ordering از **همان existing seat item `id`** با `offerId+Pax+FlightRef` Resolve می‌شود، بدون فیلد جدید در SeatMap.

## 15.5 ایجاد Order و Hold گروهی (از طریق Ordering)
UI ⇒ Ordering (route نهایی با واقعی Ordering تطبیق شود) `POST /{Surface}/v1/Orders` با AirOffer/انتخاب‌ها و traveler data. Ordering یک `OrderId` و برای هر انتخاب `OrderServiceId` می‌سازد، سپس S2S:
```http
POST /Service/v1/Ancillaries/Service-Holds
```
```json
{
 "orderId":81235,"reference":"ORDER-81235","idempotencyKey":"O81235-H1",
 "requestedExpiresAt":"2026-10-16T19:30:00Z",
 "services":[
   {"orderServiceId":891,"pricedSelectionRef":"psel-bag-P1-2pcs","travellerId":154,"coverageScope":"Portion","coveredFlightIds":[8001,8002]},
   {"orderServiceId":892,"pricedSelectionRef":"psel-lounge-order","travellerId":null,"coverageScope":"Order","coveredFlightIds":[]}
 ]
}
```
Response target:
```json
{"holdId":11882,"orderId":81235,"status":"Pending","outcome":"Partial",
 "units":[
   {"unitId":11901,"orderServiceId":891,"status":"Held","providerUnitRef":"BAG-H-27","quantity":2},
   {"unitId":11902,"orderServiceId":892,"status":"Pending","pendingOperation":"Hold","pendingReasonCode":"ManualConfirmation","providerUnitRef":null,"quantity":1}
 ]}
```
**Ordering نمی‌تواند کل Group را Confirmed اعلام کند تا Unit دوم evidence معتبر بگیرد؛ Unit اول از بین نرفته است.**

## 15.6 Confirmation/Provider callback/Recovery
```http
POST /Service/v1/Ancillaries/Service-Holds/11882/Units/11902/ProviderOutcomes
```
```json
{"providerOperationRef":"REQ-CIP-07","result":"Held","providerUnitRef":"CIP-H-552","evidenceAtUtc":"2026-10-16T19:10:00Z","sourceKind":"AuthorizedManual"}
```
Caller internal approved operator only. سپس Ordering بعد payment/guarantee:
```http
POST /Service/v1/Ancillaries/Service-Holds/11882/Confirmations
```
```json
{"idempotencyKey":"O81235-CONFIRM-1","unitIds":[11901,11902],"paymentOrGuaranteeReference":"PG-81235"}
```
اگر Provider دوم Confirm را نمی‌پذیرد، Unit2 `Pending(Confirm)` است؛ Group `Pending/Mixed`، نه Confirmed. بعد reconnect GET Hold و Retry همان Unit، نه ساخت Group جدید.

## 15.7 CancellationQuote جزئی و Refund مالی
```http
POST /Service/v1/Ancillaries/Service-Confirmations/11882/CancellationQuotes
```
```json
{"orderId":81235,"unitIds":[11901],"reasonKind":"Voluntary"}
```
Service quote target:
```json
{"cancellationQuoteRef":"anc-cq-1","holdId":11882,"orderId":81235,
 "validUntilUtc":"2026-10-16T20:00:00Z",
 "units":[{"unitId":11901,"orderServiceId":891,"canCancel":true,"canRefund":true,
   "quoteState":"Quoted","originalServiceAmount":"1800000","currencyId":70,
   "penaltyAmount":"350000","penaltyCurrencyId":70,"penaltyKind":"FixedAmount",
   "ruleRef":"BAG-CANCEL-RULE","waiverApplied":false,"requiresManualReview":false}]}
```
Ordering این مورد را با `EMD-Coupon` و پرداخت/Tax و Refund قبل بررسی می‌کند و **OrderCancellationQuoteRef** جدا به مشتری ارائه می‌دهد. پس از تأیید:
```http
POST /Service/v1/Ancillaries/Service-Confirmations/11882/Cancellations
```
```json
{"idempotencyKey":"O81235-CANCEL-11901","orderId":81235,"unitIds":[11901],
 "cancellationQuoteRef":"anc-cq-1","reasonKind":"Voluntary"}
```
مبلغ پرداخت نهایی در Ancillary هیچ API ندارد؛ همان‌جا `UnitStatus=Cancelled` فقط معنای لغو entitlement دارد. Ordering EMD و Payment Refund را پیگیری می‌کند.

## 15.8 Add-on پس از Ticket
`GET /{Surface}/v1/Orders/{orderId}` ⇒ Shop همان Engine با trusted Order Context؛ `Details` ⇒ `Evaluate` ⇒ `pricedSelectionRef` ⇒ `POST /{Surface}/v1/Orders/{orderId}/Ancillaries` (Ordering target) ⇒ new OrderService/Unit, payment/guarantee, provider confirm, EMD issue. **هیچ AirOffer قدیمی را به‌عنوان منبع post-order price مجدداً مصرف نکنید**.
