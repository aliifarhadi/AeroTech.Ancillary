# 08 — کنترل بسته واحد و گیت سازگاری Pack v8

**نکته:** این سند گزارش آزمون فایل‌های Pack است و ادعا نمی‌کند Build یا Test پروژه اجرا شده است.

## Contract invariants checked by automated verifier

1. وجود همه ۱۵ سند `00,01,02,03,04,05,06,07,08,10,11,12,13,14,15` و همه ۶ Prompt اجرایی.
2. ماتریس `05` یگانه و دارای **۷۸ Scenario** با ID یکتا: S01..S16, H01..H13, T01..T19, O01..O12, R01..R14, N01..N04.
3. Quantity-only: Shopping دارای `quantityOptions[]` و شناسه امضاشده `d_`، **هرگز Snapshot و GET/POST Options ندارد**.
4. Typed: فقط `r_ → GET/POST Options → s_`، یک جدول SelectionSnapshot. Direct و Qty stateless هستند.
5. Rate Retired/Suspended: فقط Pricing aggregate والد Status دارد؛ صدور/POST/Resolve انتخاب ثبت‌نشده باید **رد** کند. Snapshot نیز مجوز Retired نیست.
6. `ResolveGrouped`: فقط `{ids:string[]}`، بدون OfferId/OrderId/POS/Country/SelectedOptions در Request؛ خروجی specialized family arrays.
7. JSON: نمونه A01 به‌صورت Envelope واقعی، نمونه A11 و A15 به‌صورت ردیف/بخش مشخص، همه باید مطابق فیلدهای الزامی `02` باشند. Free: Rate معتبر موجود و قیمت صفر؛ Currency EUR Platform ID 47 بر اساس اعلام Owner.
8. A07/A08 SeatMap untouched؛ A09/A17/A19/Insurance GATED تا دلیل معتبر؛ A18 در نبود trusted source gated.
9. هیچ فایل Pack قدیمی یا مرجع ماتریس غیر v8 در ZIP وجود ندارد؛ لینک‌ها فقط به فایل‌های همان Pack.
10. هَش و نام تمام فایل‌ها در `SHA256SUMS.txt` ثبت شده و `VERIFY-PACK.py` روی فایل‌های استخراج‌شده اجراپذیر است.

## Scope of verification

`PASS` کنترل فایل/ارجاع/ساختار JSON/HMAC-golden/test-ID به معنی `PASS` رفتار ران‌تایم سرویس نیست. Gate0 باید روی Repository واقعی و Stage5 روی build/test/HTTP/EF، با شواهد مستقل اجرا شوند.

## Auto verification summary

**کنترل‌هایی که در هنگام تحویل باید PASS شوند:** ۱۵ سند، ۶ پرامپت 10..15، ۷۸ شناسه سناریو، JSONهای قابل parse، عدم ارجاع Authority به نسخه قدیمی، HMAC golden bytes، SHA256 هر فایل، استخراج ZIP و تطابق byte-for-byte با پوشه تولید. نتیجه چاپ‌شده توسط `VERIFY-PACK.py` سند نهایی کنترل فایل‌هاست. این کنترل‌ها هر بار پس از extract قابل تکرارند.
