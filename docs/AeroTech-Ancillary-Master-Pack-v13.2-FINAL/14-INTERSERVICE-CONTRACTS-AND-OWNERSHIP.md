# 14 — Integration قراردادهای PSS: AirAvail، FlightFlow، Ordering، JetPay، Ticketing

## 14.1 Ownership table
| Fact/Action | صاحب حقیقت | قرارداد Ancillary |
|---|---|---|
| AirFare/FareFamily/FareBasis/RBD/Allowance/Price Offer | AirPrice → AirAvail | S2S `FlightOffers/Details`، فقط projection shopping |
| FlightId/FlightCapacityId/FlightVersion/Seat occupancy/Physical Hold | FlightFlow | Read-only occupancy/layout و physical seat evidence توسط Ordering، no local seat stock |
| Ancillary Definition/Provision/Pricing/Option/Selection/Service cancellation terms | Ancillary | read Shop/Quote و S2S Hold/Cancel |
| Ownership user, Order/OrderService/Traveller, PNR, full Order history | Ordering | supplied/trusted Order context and OrderServiceId links |
| Payment guarantee/authorize/capture/refund/wallet/cash/agency credit | Payment/JetPay | Ordering orchestrates; Ancillary no money movement |
| E-ticket, EMD-A/EMD-S, coupon status, void/exchange/refund | Ordering/Ticketing | Ancillary DocumentRouting metadata only |
| Travel Agency eligibility, Office/PCC/commission and customer channel | authorized Sales/Auth/Ordering reference | Caller-derived trusted sales scope, no public office leaks |
| Supplier actual hold/cancel/confirmation | Fulfillment supplier/provider | provider port with scoped refs and evidence |

## 14.2 AirAvail [CURRENT]
`POST /Service/v1/FlightOffers/Details` `{offerId}` — priced `AirOffer`، validating offer context؛ پاسخ شامل Tickets/Travellers, AirTransports/Flights, FareBasis/FareFamily, allowances, Currency, FX, LastTicketingDate. `AirOfferId` الزام Order هنوز نیست؛ PreOrder Shopping از همین داده شروع می‌شود.

`FlightId` != `FlightCapacityId`؛ S2 `FlightRef` می‌تواند flight instance باشد، اما physical FlightFlow Hold با `FlightCapacityId` مربوط انجام می‌شود. `FareFamily` فعلی در `ServiceOfferFareComponentDto` `string?` است؛ آن را به ID عددی دلخواه تبدیل نکنید. Source `ValidUntilUtc` در DTO منتشرشده نیست.

## 14.3 FlightFlow [CURRENT endpoint observed; EXTERNAL environment separate]
- `POST /Service/v1/Flights/OccupiedSeats` read occupancy.
- `POST /Service/v1/Flights/Seat-Holds` {IdempotencyKey,Reference,ExpiresAt,Passengers[],Flights[FlightCapId,Seats[PaxReference,Revenue,Seat]]} → `{HoldId,Seats[FlightId,PaxReference,Seat,SeatHoldReference,SeatHoldStatus]}`.
- `POST /.../Seat-Holds/{id}/Confirmations`, `PATCH /.../Seat-Holds/{id}`, `DELETE /.../Seat-Holds/{id}`, `POST /.../Seat-Confirmations/{id}/Cancellation` (+ `SeatHoldReferences[]`).

این قرارداد الگوی Group+Unit reference و Partial Cancellation را فراهم می‌کند؛ **Ancillary نباید flight seat reservation را با API خودش دوباره اجرا کند.** در هماهنگی Seat Purchase، Ordering فیزیکی FlightFlow و تجاری Ancillary را با Context یکسان map می‌کند. FlightFlow اخیراً منتشرشده در GitHub به معنی deployed بودن route نیست؛ Owner این وابستگی را **non-blocking برای کد Ancillary** تعیین کرده است.

## 14.4 Ordering [TARGET ONLY — source inaccessible]
### Internal context reader required
`IOrderCommercialContextReader.GetAsync(orderId,callerScope)` باید Snapshot معتبر Order را برای Shopping پس از خرید ارائه کند:
`OrderId,CommercialVersion,OwnerAirlineId,Channel/POS,BuyerSellerReference(optional internal),Travellers[OrderTravellerId,AirOfferTravellerRef?,PTC],Flights[FlightId,FlightVersion,FlightCapacityId,Cabin/FareBasis/FareFamily,BoundRef,Airline],OrderServices[OrderServiceId,Status/Document],Tickets/Coupons[Status,Used/Flown/Refunded],Funding/SettlementSummary minimal,PurchaseStage`.

**این یک پورت دامنه پیشنهادی است؛ وجود REST endpoint در Ordering ادعا نمی‌شود.** سرویس Ordering باید بسته Contract/URL نهایی را با Owner خود تطبیق دهد؛ Ancillary نباید DTO واقعی Ordering را از روی نام حدس بزند.

### Public Selling/Servicing entry points (TARGET, not Verified)
Prefix `{Surface}` = `Backoffice`, `Api`, `OtaPanel`.

| Intent | Suggested Ordering route | نکته |
|---|---|---|
| ایجاد سفارش پرواز با انتخاب Ancillary | `POST /{Surface}/v1/Orders` | referenced priced AirOffer+selected ancillary refs؛ conforms current Ordering lifecycle after source alignment |
| مشاهده Order/Services | `GET /{Surface}/v1/Orders/{orderId}` | Order status, payment and document refs |
| افزودن خدمات به Order موجود | `POST /{Surface}/v1/Orders/{orderId}/Ancillaries` | input selections و payment intent؛ managed as OrderChange |
| بررسی امکان لغو/مبلغ/جریمه | `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/CancellationQuotes` | returns **OrderCancellationQuoteRef** و full customer refund summary |
| اجرای لغو تاییدشده | `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/Cancellations` | accept quoteRef/selected OrderServices؛ async if necessary |
| وضعیت کارهای تغییر و refund | `GET /{Surface}/v1/Orders/{orderId}/Ancillaries/Changes/{changeId}` | واحدهای Cancel/EMD/Payment statuses، immutable history |

**Confirm و Payment Refund نیازی به Endpoint عمومی موازی داخل Ancillary ندارند.** Checkout/issuance/Refund orchestration داخل Ordering انجام می‌شود و Surface فقط نتیجه/Status را می‌خواند. مسیرهای پیشنهادی در Repo Ordering قبل از اعمال **باید با مسیرهای واقعی آن تطبیق و در صورت تعارض بدون duplication اصلاح شوند**؛ تغییر آن repo خارج Scope Agent Ancillary است.

## 14.5 Payment/JetPay [INTERFACE CONTRACT]
Ancillary endpoint Payment ندارد. برای حفظ E2E، Ordering این intentها را از Payment owner می‌گیرد: `CreateIntent/Guarantee/Authorize/Capture/ReleaseAuthorization/Refund/PartialRefund/RefundStatus`, wallet ledger/agency-credit/cash settlement. Actual API routes بر اساس قرارداد JetPay معتبر شوند؛ **هیچ مسیر خیالی `/Service/v1/Payments/Refunds` را به‌عنوان Current اعلام نکنید.**

ترتیب `Hold -> Payment guarantee -> Confirm` برای Providerهای HoldConfirm استاندارد داخلی است. DirectConfirm supplierها می‌توانند بعد از payment authorization یا credit guarantee تایید شوند و در شکست Supplier، Compensation/Refund لازم است. فروش Airport Cash یا Agency billing با تعهد معتبر ممکن است Payment فوری نداشته باشد؛ Eligibility مالی را Ordering تعیین می‌کند.

## 14.6 Ticketing / EMD contract
Ordering از Ancillary per Unit این fields را می‌خواهد: `OrderServiceId,HoldId,UnitId,ProviderUnitRef,Traveller/Coverage,Price/currency/tax components,ServiceSubCode/Source,DocumentRouting,AncillaryDocumentType,RFIC/RFISC,BookingMethod,SSR/SSIM,ConfirmationRequirement,Service Status,Refundability/Eligible cancellation terms`.

Ticketing EMD-A با coupon مرتبط، EMD-S standalone، ETKT exchange برای upgrade؛ شماره سند و state `Issued|Exchanged|Refunded|Void` را فقط Owner سند ایجاد می‌کند. Ancillary event lifecycle نباید بدون OrderDocument proof EMD issued ادعا کند.

## 14.7 Webhook/events [TARGET minimal]
`AncillaryHoldOutcomeChanged`, `AncillaryUnitConfirmed`, `AncillaryUnitCancelled`, `AncillaryUnitReleaseCompleted`, `AncillaryProviderOutcomeUnknown`؛ payload `EventId,CorrelationId,OrderId,OrderServiceId,HoldId,UnitId,ProviderKey?(internal),Status,OccurredAtUtc,ReasonCode?,ChangedAt`. Contract واقعی Messaging را پیش از انتشار با Ordering مطابقت دهید؛ cross-office PCC/PII leak ممنوع. Event حداقل یک‌بار، Ordering idempotent consumption/reconciliation.

## 14.8 Auth cross-surfaces
- Backoffice: Caller Airline/Office/POS مجاز؛ internal manual fulfillment only approved role.
- Api: Agency/Partner authenticated؛ همان Order owner و SupplierProducts، no forged sales scope.
- OtaPanel: همان business rules ولی mapper/view جدا، نه Caller bypass.
- Service: trusted Ordering internal; dedicated capabilities برای Manual Supplier webhooks؛ External provider secret internal only.

## 14.9 No accidental external dependency blocking
Stage 1 and 2 ذاتاً با مرجع کد و صحت Internal Contracts قابل بسته شدن‌اند؛ اگر real FlightFlow/Ordering/Payment environment ready نیست، `EXTERNAL_PENDING` جدا در Report بیاید. P3 Ancillary closure صرفا صحت قرارداد و رفتار خود Ancillary را ثابت می‌کند؛ Cross-service E2E verification فقط وقتی همه سرویس‌ها مستقرند، جداگانه قابل اعلام است.
