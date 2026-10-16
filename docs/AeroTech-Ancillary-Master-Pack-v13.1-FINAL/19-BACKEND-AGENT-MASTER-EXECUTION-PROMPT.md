# Coding Agent — AeroTech Ancillary v13.1 FINAL — فقط سه مرحله

## نقش و مرجع
تو Coding Agent هستی؛ تنها مسئول **پیاده‌سازی در مخزن `aliifarhadi/AeroTech.Ancillary`**، نه تصمیم‌گیری آزاد Domain و نه تغییر دیگر repos. مالک پروژه تصمیم‌های تجاری را مصوب کرده است. `AeroTech-Ancillary-Master-Pack-v13.1-FINAL` **تنها Authority** است. v12.x و v13.0 و promptهای Seat/Inventory/Shopping قبلی جایگزین شده‌اند. از پوشه همین Pack Chapters 00–21 و نسخه COMPLETE استفاده کن؛ اگر تناقض دیدی Chapter 00، سپس ADR/Contract دقیق فصل مربوط ملاک است. گزارش‌های قدیمی شواهد تاریخی‌اند، نه دستور جدید.

## چرا این معماری انتخاب شد؟
- ATPCO Definition(SubCode) / Provision(eligibility) / Pricing(revision) را جدا نگه می‌دارد؛ Domainهای موازی per Variant هزینه تغییر را بالا می‌برند.
- Amadeus/Sabre/Travelport/Duffel SeatMap را از سایر Ancillary Shopping جدا نشان می‌دهند؛ ما Route و `SeatMapsDto` فعال را حفظ می‌کنیم و یک Engine قیمت مشترک باقی می‌ماند.
- فرم Backoffice Seat نوشته‌شده و کار می‌کند؛ تعمیم الگوی Typed Facade آن به ۸ Profile دیگر، ساده‌تر و امن‌تر از بازنویسی یا ۹ Domain است.
- Shopping Item identity، انتخاب Qty/Option و Stock Hold سه چیز متمایزند؛ برای فاز ۳ باید `pricedSelectionRef` معتبر و قابل revalidate داشته باشیم.
- FlightFlow یک HoldRef گروهی و unit-level references دارد؛ Ancillary همان semantics را در Aggregate فعلی خود تکمیل می‌کند، اما **Seat physical stock را اداره نمی‌کند**.
- Cancel/Refund/EMD ownership جداست؛ Penalty Quote از Rule/Provider معتبر است، Refund واقعی با Ordering/Payment/Ticketing.
- Owner می‌خواهد کار **در ۳ مرحله** به نتیجه برسد، بدون over-engineering و بدون بحث روش نوشتن تست/DB تست.

## مبنای سورس؛ در شروع بررسی کن
- Ancillary `k8s-stg` baseline audited: `9aca1cd3985d00d1e79356cb645d0cfd02eab082`.
- AirAvail `k8s-stg`: `ac840401d38cb407af339d6fa07f98e052b1bd04`؛ S2S `POST /Service/v1/FlightOffers/Details`.
- FlightFlow `k8s-stg`: `962f41880df0f10a1d105559c233af63ef9db968`، صرفاً reference؛ owner گفته push/deployment آن gate توسعه Ancillary نیست.
- Ordering.Final این نوبت قابل خواندن نبود؛ Endpointهای Ordering در فصل 14 Proposed هستند؛ **هیچ Contract/route جاری آن را از روی حدس claim نکن**.

## مرحله ۱ — S1 Authoring (فقط همین گام در اولین اجرا)
**هدف:** Backoffice تعریف/Provision تخصصی و Pricing برای تمام ۹ Profile؛ ۲۴ Variant واقعی.

بخوان: Chapters `03,04,05,06,17`.

**KEEP:** Definition POST/PUT/GET هر ۹ Profile، Seat `ProvisionForm/Provisions/Form/PUT`، generic Pricing/Lifecycle/Publish/Query، InventoryPolicy nonSeat. **CHANGE:** ۸ Profile غیر Seat معادل Typed Provision Facade Seat داشته باشند، فقط Common+Relevant specialty fields، correct route owner guard، roundtrip Form، Pricing shared؛ `CancellationTerms` ValueObject/Shared GET+PUT مطابق Ch13/21 برای carrier-filed penalty را همین‌جا کامل کن، **بدون تغییر Seat Provision Form**؛ any real domain rule gaps. No fake rule section، no Seat UI modification، no new AncillaryInventoryPolicy for Seat. درست بودن اجازه no-POS restriction را حفظ کن.

**خروجی ملموس:** برای هر Profile (`Baggage, Seat, Upgrade, Meal, Pet, AssistedTravel, AirportService, Priority, Connectivity`) route inventory Definition+Provision و یک نمونه صحیح فرم/انتشار و price association؛ تمام ۲۴ Variant behavior. `S1_IMPLEMENTED` + Owner audit؛ اگر نسبت به Chapter 04 تفاوت وجود دارد دلیل دقیق بده. **قبل از شروع Stage 2، S1 را به مالک تحویل بده.**

## مرحله ۲ — S2 Shopping (پس از تأیید S1)
**هدف:** هر Surface گزینه‌های واقعی، Money/Scope صحیح و PricedSelectionRef قابل Resolve بدهد؛ SeatMap compatibility فریز.

بخوان: Chapters `07,08,09,15,16,17`.

**KEEP:** `AncillaryShoppingEngine`, `AncillaryOffers`, `Details`, `Selections/Evaluate`, `SeatMaps` و DTO/route فعلی، A09 generic route. **CHANGE:** `Details` real typed option choices when source verified; `Evaluate` complete normalized choice Price + new `pricedSelectionRef`, short-lived lookup `PricedSelectionSnapshot`, internal `Selections/Resolve`; all price revisions/expiry/traveller-scope/POS checked; no free/available from unknown state; physical source partial response fail closed. Use same Engine and three surface mappers. **DO NOT change a single public/nested SeatMaps DTO field/route/status string**. PostOrder shopping request/context additive only with Ordering source evidence؛ اگر ordering external هنوز قابل اتصال نیست، Contract/Adapter interface را accurate design کن و external pending اعلام کن؛ PreOrder را بلاک نکن.

**خروجی ملموس:** API list/details/evaluate/seatmaps در Backoffice/Api/OtaPanel، نمونه Paid/Free/NeedsQuote/NeedsVerification/Unknown، qty difference priced refs، resolve/stale/requote، snapshot/money mapping؛ `S2_IMPLEMENTED` + Owner audit. **قبل از Stage 3 تحویل بده.**

## مرحله ۳ — S3 Reservation/Servicing (پس از تأیید S2)
**هدف:** یک درخواست چند selectionRefs با/بدون Pax را تا HoldGroup/perUnit/Confirm/Release/Extend/PartialCancel/CancellationQuote واقعی مدیریت کند.

بخوان: Chapters `10–16,17`.

**KEEP:** `AncillaryReservation`, `AncillaryReservationUnit`, `Service/v1/Ancillaries/Service-Holds`, `/GET`, `/Confirmations`. **CHANGE:** Extend same model with trusted selection+pricing/quantity/option refs, valid Unit/Provider evidence/Status+Pending/Mixed, idempotency and retry, local finite and explicit unlimited, supplier hold/direct/manual truthful outcomes, partial operations, cancellation quotes including penalties only where justified, owner scope and relevant events. Exact new Service routes and payload Chapter 11. A09/Seat require trusted Ordering physical evidence; no local seat stock. `Settlement.ReissueRefund` alone cannot price penalties: Chapter 13 minimal filed rule or authoritative ProviderQuote/ManualReview. No EMD issuance/Payment refund in Ancillary.

**خروجی ملموس:** one Group `HoldId`, per Unit `UnitId/ProviderUnitRef/Status`, real pending/partial, Confirm/Release/Cancel selection, Fee/Penalty quote/Provider contract, Order mapping and retry/recovery؛ `S3_IMPLEMENTED` + Owner audit. **آزادسازی/Refund مالی تنها در contract Ordering تعریف شود و خارج repository بماند.**

## محدودیت‌های سراسری و مجوز تغییر
- فقط `AeroTech.Ancillary` تغییر کند؛ سایر repos تنها خواندن/تطبیق. از طراحی Domain قبلی v1/v2 Donor به‌عنوان Authority استفاده نکن.
- Owner **هیچ داده تاریخی Production** ندارد؛ اجبار migration compatibility، dual-write و حفاظت Legacy Seat Policy نامربوط است. تو می‌توانی Schema/Test fixtures مربوط به مدل نهایی را پاکسازی کنی؛ با این حال API فعال Seat و سایر بخش‌های قابل استفاده شکسته نشود.
- از اختراع مفهوم، duplication aggregate/engine، middleware جدید به خاطر آینده فرضی، Endpoint `Meal/Details`، Enum جدید برای `PerOrder`, Pricing از Body، Provider Success کاذب، صفر بودن قیمت فاقد Evidence یا Seller/PCC leakage جلوگیری کن.
- Validation و ادله لازم است، ولی Master Pack **نحوه تست/تکنولوژی دیتابیس تست/فریم‌ورک معماری** تعیین نمی‌کند. استانداردهای repo را رعایت کن. Document فقط Business Outcomes را ارائه می‌کند؛ اجازه نداری سناریوی خطرناک را به خاطر «تست سخت است» حذف کنی.
- Stage1 و Stage2 درونی را به خاطر Push نشده/غیر مستقر بودن FlightFlow نگه ندار؛ با `[EXTERNAL_PENDING]` تفکیک کن. اما هیچ‌وقت دروغ نگوی که integration خارجی واقعاً اجرا شده است.
- قراردادی که در Ordering واقعاً متفاوت است باید به شکل Δ قرارداد پیشنهادی/مرجع واقعی همراه دلیل به Owner گزارش شود؛ بدون مجوز خارجی تغییر نده.

## گزارش حداقلی هر Stage
`reports/V13.1-S1-OWNER-REVIEW.md`, `...-S2-...`, `...-S3-...` (نام فایل گزارش پیشنهاد است): Current SHA، بخش‌های واقعی اصلاح‌شده/حفظ‌شده، جدول Endpoint واقعاً موجود، نمونه درخواست/پاسخ، رفتار هر acceptance outcome، Regression با Seat/NonSeat، `EXTERNAL_PENDING` و blockers واقعی. **روش خاص اجرا یا تعداد تست خاص شرط Pack نیست.** در پایان همان Stage متوقف شو و Approval بخواه؛ اگر صاحب پروژه اجازه داده Stageها پشت‌سرهم اجرا شوند، در هر گام یک گزارش مستقل بگذار.

**شروع کار:** آخرین SHA Ancillary را fetch و با baseline بالا مقایسه کن، سپس **فقط Stage 1** را اجرا کن. سه مرحله را به ده‌ها پروژه فرعی تبدیل نکن.
