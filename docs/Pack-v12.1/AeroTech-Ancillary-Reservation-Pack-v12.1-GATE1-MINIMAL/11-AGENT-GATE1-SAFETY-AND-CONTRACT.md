# Gate 1 — دستور اجرایی ایجنت — حداقل اصلاح ایمنی Hold API (v12.1)

## 0. Context و Authority

Repo: `aliifarhadi/AeroTech.Ancillary`; branch: `feat/ancillary-offers-v9`; Gate0 source pin: `e6a2012ee101546f5728db6a68844adcb6051204`. قبل از کار HEAD/status را دوباره بخوان؛ از commit جدید بی‌خبر عبور نکن. Authority: `CLAUDE.md` > `00/02/05/07/09` همین Pack v12.1 و تصمیم‌های مصوب Owner. گزارش ممیزی موجود: `reports/V12-GATE0-SOURCE-AND-CONFLICTS.md`. **Gate0 را تکرار نکن.** هیچ `commit/push/merge` بدون دستور Owner. این Prompt صرفاً تغییر محدود و تست ایزوله را مجاز می‌داند؛ هیچ عملیات روی dev/prod، token یا داده مالک مجاز نیست.

## 1. تصمیم‌های بسته‌شده — دوباره سؤال نپرس

- Route rename **انجام شده**: `POST /Service/v1/Ancillaries/Hold`, `GET/DELETE /Service/v1/Ancillaries/Hold/{id}`, `POST /Service/v1/Ancillaries/Hold/{id}/Confirmations`, `POST /Service/v1/Ancillaries/Hold/{id}/Cancellations`. نگارش مفرد `Hold` دقیقاً تصمیم Owner است. `SeatMaps/Holds` و `SeatMaps/Resolve` تغییر نکنند. **نه alias مسیر قدیمی، نه endpoint جدید.**
- F02/P0 پیشین **قابل تکرار نبود**: HTTP binder `?unitIds=` → 400، `?unitIds=0` و duplicate → validation failure. روی این مورد Domain/Validator را تغییر نده.
- F04–F06 فقط Doc Errata (در Pack اصلاح شده). F07 (Offers/Resolve options union) و F09 (صفر IRR در catalog تستی) از Scope خارج‌اند.
- v11 Aggregate/Typed DTO/SQL ledger/EF migration/SeatMap lifecycle صحیح باقی بمانند؛ هیچ change architecture، migration، domain rewrite، invented code/Provider یا cross-service integration نساز.

## 2. تنها تغییر کد مجاز — F03 Query-key guard

در فایل `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` فقط داخل `DELETE Hold/{holdId:long}`، قبل از ارسال `ServiceReleaseAncillaryHoldCommand`، وجود **Query Key ناشناخته** را بررسی کن. تنها نام مجاز برای Query این Action `unitIds` است (مقایسه case-insensitive، مطابق رفتار Model Binder). اگر query دارای کلید دیگری مانند `unitId`, `unitIds[]`, `unitIds[0]`, `foo` بود، **400** با ساختار error موجود و **بدون اجرای MediatR** برگردان. اگر کلیدی وجود نداشت Release-All فعلی را حفظ کن؛ اگر `unitIds=positive` داده شد فقط همان Units؛ نامعتبر/خالی با Model Binder/Validator فعلی fail شود. هیچ Query/Body/Route parameter تازه و هیچ global filter/middleware/binder جدید اضافه نکن. Error code جدید نساز.

پیاده‌سازی باید کوتاه و محلی بماند (در حد یک guard، نه abstraction). اگر Source نشان داد Query Key ناشناخته از قبل با 400 رد می‌شود، **بدون تغییر کد** گزارش کن و تست واقعی ارائه بده.

## 3. Regression Testهای لازم، محدود و ایزوله

از Test Host/DB ایزوله استفاده کن. هرگز Host را با hosted services متصل به Owner dev DB اجرا نکن؛ اگر ایزوله‌سازی ممکن نبود، اجرای live را متوقف و علت را دقیق گزارش کن. موارد:

1. `DELETE /Service/v1/Ancillaries/Hold/{holdId}` بدون query: **Release-All** موجود (positive).
2. `?unitIds=<valid-id>`: **فقط Unit مشخص** آزاد شود؛ بقیه باقی بمانند (positive).
3. `?unitIds=` و `?unitIds=abc`, `?unitIds=0`, duplicate: **400** و وضعیت Units/ledger قبل و بعد یکسان (preserve already-covered cases; نیاز به بازنویسی تست‌ها نیست).
4. `?unitId=<valid-id>`, `?unitIds[]=<valid-id>`, `?unitIds[0]=<valid-id>`: **400** بدون handler/DB mutation. این F03 جدید است؛ هر موردِ syntax که عملاً Binder قبول می‌کند نباید All شود.
5. `Cancel {"unitIds":[]}` همچنان fail-closed؛ `GET` و `Confirm` روی Route `Hold` برقرار.
6. `SeatMaps/Holds`, `SeatMaps/Resolve`, Typed Hold request/response و old route non-alias unchanged.

تا حد امکان به suiteهای موجود route و controller یک یا دو تست اضافه کن؛ test project جدید نساز. تعداد discovered/executed/pass/fail/skip را برای suite مرتبط بعد از تغییر گزارش کن. Full-suite rebuild فقط اگر تغییر باعث شکست مرتبط شد یا Owner در Gate Closure دستور داد. ادعای full green از روی logs قدیمی ممنوع.

## 4. گزارش و حد توقف

فقط `reports/V12-GATE1-MINIMAL-SAFETY.md` را تولید کن، شامل HEAD و dirty status، فایل‌های تغییریافته، rationale guard یا NO CODE CHANGE REQUIRED، نتیجه HTTP cases با response status، before/after Unit/ledger برای F03، command و counts واقعی tests، تأیید no migration/no schema/no Ordering callback/no FlightFlow call، و باقی‌مانده‌های مستقل (I05 generic Hold race test در Gate3؛ Ordering E2E NOT VERIFIED؛ Local stock dev GATED). **بعد از گزارش توقف کن؛ Gate2/3/4 را شروع نکن.** No commit/push/merge.
