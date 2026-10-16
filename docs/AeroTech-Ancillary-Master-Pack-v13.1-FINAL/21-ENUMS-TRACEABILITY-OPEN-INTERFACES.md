# 21 — Enum catalogue، Traceability و مرزهای بازِ صادقانه

## 21.1 Enum values `[CURRENT]` — از کد Ancillary
| Enum | مقادیر Numeric صحیح |
|---|---|
| `AncillaryProfile` | `Baggage=1`, `Seat=2`, `Upgrade=3`, `Meal=4`, `Pet=5`, `AssistedTravel=6`, `AirportService=7`, `Priority=8`, `Connectivity=9` |
| `ServiceCoverageScope` | `Sector=1`, `Portion=2`, `Journey=3`, `Order=4` |
| `PricingUnit` | `PerPassenger=1`, `PerRoom=2`, `PerItem=3`, `PerVehicle=4`, `PerSeat=5`, `PerPiece=6`, `PerKilogram=7` |
| `AncillaryQuantityUnit` | `Each=1`, `Piece=2`, `Kilogram=3` |
| `PriceOrigin` | `Filed=1`, `ExternalQuote=2`, `Free=3`, `NotAvailable=4`, `LegacyUnspecified=5` |
| `InventoryAuthority` | `Unlimited=1`, `Local=2`, `Supplier=3` |
| `InventoryCapacityReadState` | `NotConfigured=1`, `Unlimited=2`, `ConfiguredNotGuaranteed=3`, `ClosedForSale=4`, `DelegatedCheckRequired=5`, `Unknown=6`, `UnsupportedPattern=7`, `NotApplicable=8` |
| `OfferReadiness` | `NotEligible=1`, `NeedsSelection=2`, `NeedsQuote=3`, `NeedsVerification=4`, `Selectable=5`, `Unavailable=6` |
| `PurchaseStage` | `PreOrder=1`, `PostTicketed=2`, `Both=3`, `LegacyUnspecified=4`, `OnBoard=5` |
| `ProvisionApplicationType` | `Standard=1`, `Baggage=2`, `Seat=3` |
| `SupplierFulfillmentKind` | `Local=1`, `External=2` |
| `AncillaryReservationStatus` | `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` |
| `AncillaryReservationUnitStatus` | `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` |
| `ReissueRefundPolicy` | `Refundable=1`, `NonRefundable=2`, `NonRefundableReusable=3` |
| `FormOfRefund` | `OriginalPayment=1`, `EVoucher=2` |
| `DocumentRouting` | `NoAncillaryDocument=1`, `Emd=2`, `TicketOrExchange=3` |
| `AncillaryDocumentType` | `None=1`, `EmdAssociated=2`, `EmdStandalone=3` |
| `BookingMethod` | `Ssr=1`, `AuxiliarySegment=2`, `DisplayPriceContactCarrierForBooking=3`, `NoBookingProcessRequired=4`, `PerServiceRecord=5` |
| `SelectionKind` | `SimpleOptIn=1`, `QuantityChoice=2`, `TypedForm=3`, `SeatMapSelection=4`, `ExternalQuote=5` |
| `SelectionFieldType` | `Integer=1`, `Decimal=2`, `Text=3`, `Boolean=4`, `Code=5`, `CodeSet=6`, `Reference=7`, `DimensionsCm=8`, `DateTimeWithOffset=9` |
| `SeatPurpose` | `Standard=1`, `Preferred=2`, `ExtraSeat=3` |

`InventoryAuthority.FlightFlow=4` **در HEAD فعلی Ancillary وجود ندارد** و نباید دوباره اضافه شود. Enumهای قدیمی renumber نمی‌شوند.

## 21.2 پیشنهاد محدود enum جدید `[TARGET]`
| Domain | فقط مقادیر واقعا لازم | چرا |
|---|---|---|
| `AncillaryReservationUnitStatus` append | `Requested=6`, `Pending=7`, `Failed=8` | truthful asynchronous/provider unresolved |
| `AncillaryReservationStatus` append | `Pending=6`, `Mixed=7`, `Failed=8` | batch complete vs partial/fail در یک field |
| `PenaltyKind` if filed-policy needed | `None=1`, `FixedAmount=2`, `Percentage=3`, `ProviderQuoted=4`, `ManualReview=5` | penalty source/treatment، فیلد به جای string arbitrary |
| `CancellationReasonKind` if existing enum absent | `Voluntary=1`, `Involuntary=2`, `SupplierFailure=3`, `NoShow=4` | مصوب و auditability؛ به enum همان پروژه مپ شود |

`PendingOperation` و `PendingReasonCode` می‌توانند Code constrained String باقی بمانند و نیاز به Enumهای جدید ندارند. `Outcome=Complete|Partial|Pending|Failed` فقط projection است، نه Domain Aggregate. قبل از اضافه کردن Enum دوم به کد، نام معادل موجود در Repo را چک کن؛ Duplicate نساخته و Numeric را شفاف گزارش کن.

## 21.3 جای دقیق Rule جریمه و ارتباط Stageها
`SettlementDefinition.ReissueRefund` **تنها eligibility** است؛ برای Carrier-filed penalty باید یک `CancellationRules` ValueObject اختیاری زیر Provision/Settlement ثبت شود. برای اینکه S3 دوباره S1 را باز نکند، **Authoring shared آن در S1 بسته می‌شود** و S3 فقط محاسبه/Quote/استفاده می‌کند.

قرارداد مدیریتی مشترک جدید `[TARGET]`، بدون تغییر request/response فرم موجود Seat:
```
GET /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms
PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms
```
`PUT` فقط Draft Provision را تغییر می‌دهد؛ بعد Publish/Revision immutable rules فعال‌اند. بدنه `CancellationRules[]` و eligibility evidence با ValueObject فصل 13؛ فقط Rules قابل استناد Carrier/Supplier، نه جریمه فرضی همه Variantها. اگر Policy معتبر نزد Supplier است، `PenaltyKind=ProviderQuoted` و جزئیات مبلغ در S1 فایل نمی‌شود؛ برای Free/no document نیز Rule `None` **صریح** لازم است اگر محاسبه خودکار quote مجاز باشد. هیچ Create/Manage Aggregate/DbSet جدید برای Penalty لازم نیست.

## 21.4 Known external pending — بدون تظاهر به تطبیق
| Dependency | وضع موجود | شرط تصدیق real end-to-end |
|---|---|---|
| AirAvail priced offer | Contract واقعی و سورس قابل خواندن | Running connection، correctness per-channel |
| FlightFlow OccupiedSeats/physical holds | سورس Contract قابل خواندن؛ Owner گفته deployment work غیر gate | runtime response، flightIds complete، physical refs صحه‌گذاری |
| AirInfo Aircraft layout | Ancillary Adapter current | matching AircraftVersion + accurate seat map |
| Ordering source and APIs | Repo inaccessible current session | owner-consumed contract `IOrderCommercialContextReader`, OrderService identity/reconciliation |
| JetPay/Payment | boundary identified، routes not verified | guarantee/capture/refund status+idempotency |
| Ticketing EMD-A/EMD-S | boundary identified، routes not verified | approved issuance/void/refund per coupon and status |
| Supplier external/manual | adapter capability per provider not generally deployed | actual hold/confirm/cancel evidence; unsupported capabilities reported |

این لیست مانع نوشتن دامنه Ancillary نیست و جایگزین ارائه Contract شفاف نیز نیست.

## 21.5 Traceability contracts→scenario→owner
| Requirement | Domain/API chapter | سناریوها در Ch16 | Owner |
|---|---|---|---|
| Typed forms ۹ Profile، بدون تغییر Seat | 03–06 | C01..C04, C11..C12 | Ancillary |
| Rules/penalty authoring بدون تغییر Seat Form | 05,13,21 | C04,C06,F06,F07,F08 | Ancillary |
| Price/Quantity/Tax/Benefit/Options | 05–08 | S01..S15,S23 | Ancillary + AirAvail facts |
| SeatMap DTO Freeze و physical availability | 09,14 | S16..S20,R14,R15 | Ancillary read facade + FlightFlow |
| `pricedSelectionRef` و context secure | 07–08 | S21,S22,S26,S27 | Ancillary |
| Group Hold/Unit/Idempotency | 10–12 | R01..R18 | Ancillary / Ordering |
| Cancellation quote/penalty | 11,13 | F01..F13,F19..F21 | Ancillary service portion |
| EMD/Payment/Refund/History | 13–14 | F03..F22 | Ordering + Ticketing + Payment |

## 21.6 Superseded conflicts resolved
- v12.3 `SeatOffers` separate **رد شد**؛ current SeatMaps retained.
- v12.2 «Shopping هنوز فاز بعد است» رد شد؛ S2 کد موجود دارد و تکمیل می‌شود.
- v13.0 «refund rule only ReissueRefund flag» برای محاسبه Penalty ناکافی بود؛ v13.1 filed rule optional و ProviderQuote/ManualReview را مشخص کرده است.
- v13.0 «P3 status pending بدون enum values» در v13.1 numeric append دارد.
- v13.0 راه‌اندازی Stage3 از `serviceDefinitionId/provisionId` غیرقابل اعتماد بود؛ v13.1 `pricedSelectionRef` و Resolve را mandatory کرده است.
- Stage1 closed بدون CancellationTerms باعث باز شدن دوباره Authoring در Stage3 می‌شد؛ v13.1 دو Route shared CancellationTerms را در S1 می‌بندد، بدون دست‌زدن به Seat Form.
- old «FlightFlow missing OccupiedSeats» صرفاً گزارش SHA قدیمی است؛ در SHA `962f4188` route مشاهده شده؛ با این وجود Owner deployment external را gate نکرده است.
