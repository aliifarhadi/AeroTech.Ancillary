# Continue AeroTech Ancillary Stage 2 without losing context — ChatGPT prompt

در چت جدید **کل فایل ZIP این بسته** یا حداقل ۷ فایل `00` تا `06` را ضمیمه کن و متن زیر را ارسال کن؛ صرفاً این متن به‌تنهایی جزئیات ۲۴ Variant و Scenario Matrix را منتقل نمی‌کند.

---

من Owner پروژه `aliifarhadi/AeroTech.Ancillary` هستم. **همه پاسخ‌ها فارسی**. فایل‌های پیوست بستهٔ رسمی طراحی/اجرای Stage2 هستند؛ فایل `01-S2-OWNER-DESIGN-MASTER-v1.0.md` قرارداد End-to-End فیلدبه‌فیلد و Scenarioهاست؛ فایل `00` Authority؛ `02` پرامپت Agent؛ `03` API Freeze؛ `04` مرز Ordering؛ `05` Acceptance Gate. ابتدا آنها را **کامل** بخوان و نظر یا ممیزی را صرفاً بر پایه آنها و HEAD واقعی Source ارائه بده؛ از حافظه ناقص یا Pack قدیمی حدس نزن.

وضعیت پیش از Coding Agent: Ancillary `k8s-stg@7efee95cbd01b5ff30bd4c8c36326debc5020574` (S2-02)؛ S2 باز، برخی List/Details/Evaluate ساخته شده ولی Order Context، Batch، token، Resolve و Seat E2E تکمیل نیستند. اگر Agent Push کرده است HEAD واقعی را ابتدا Verify کن و source diff را بررسی کن.

تصمیمات طراحی تک‌مرجع: فقط مخزن Ancillary قابل تغییر؛ `CLAUDE.md` rule1/authoring governance؛ یک Shopping Engine، Source Offer/Order از داده معتبر، سه Surface Mapper جدا؛ List Inline کوچک/Details on demand؛ Batch Evaluate mixed Seat; `pricedSelectionRef` protected Stateless self-contained Order-bound با Revalidation، **بدون PricedSelections DB یا Aggregate**؛ `POST /Service/v1/Ancillaries/Selections/Resolve` authenticated؛ SeatMap سه route و DTO دقیق فریز ولی اصلاح رفتار داخلی و تطبیق legacy 43-char ID تا Evaluate/Resolve لازم؛ A02/A24 هر DefinitionRef یک SKU؛ A01–A24 true coverage؛ Unknown price/availability/contacts => fail closed؛ Ordering Payment/EMD/S3 مالک جدا و خارج scope Agent. تمام CONTRACTها و سناریوها در فایل‌هاست؛ هیچ Concept بدون منبع نساخته و Stage1 را بی‌دلیل باز نکن.

اگر از من Audit خواسته شد، فقط verdict با دلیل، file/line و نسبت به Master؛ اگر اصلاح لازم شد **یک فایل Markdown کامل قابل دانلود** برای ایجنت بده، نه دستورات پراکنده؛ source/contract security & money & seat tests را بررسی کن. Stage را خودکار Closed ندان؛ `S2_INTERNAL_CONTRACT_READY`, `S2_EXTERNAL_INTEGRATION_PENDING`, `S2_CLOSED` (Owner only) را تفکیک کن. هیچ Commit/Push/Merge بدون دستور Owner.

---
