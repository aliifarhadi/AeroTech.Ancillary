# 04 — امنیت حداقلی و عمر شناسه‌ها: HMAC Stateless + یک جدول Snapshot

## 0. تنها معماری مجاز

یک `HMAC-SHA256` برای دو توکن `r_` و `d_`؛ یک جدول DB برای `s_` فقط Typed؛ خواندن Rate→Pricing→Provision→Definition از Repository فعلی. **نه Token Store برای Direct، نه جدول Qty، نه موتور Session یا Permission جدید، نه Key Registry داخلی DB، نه Background Worker در این فاز.** APIها از Auth فعلی بهره می‌گیرند.

## 1. منشأ Trust: Shopping

Shopping با `offerId`، trusted source و SalesScope Resolver فعلی، Candidate واقعی Engine را پیدا می‌کند. اگر Rate فعال/واجد شرایط نیست، قیمت قطعی نیست، Quote/Approval لازم دارد، source معتبر نیست یا Scope در Unit قابل تعیین نیست، **هیچ r_/d_ صادر نشود**. Price PTC: `PricingRate.PassengerTypeCode` و سن واقعی همان Traveller را قبل امضا اعمال کنید؛ CHD Rate برای ADT امضا نمی‌شود. POS restrictions، itinerary eligibility، provision rules و original authority تنها هنگام صدور بررسی شوند.

`offerBinding` = digest canonical از شناسه و نسخه معتبر Offer (`offerId`,`trustedSourceVersion`). `salesBinding` = digest canonical از owner/channel/authorized office/POS/agency context **از server-side trusted resolver**. از Client چیزی برای دست‌کاری این دو دریافت نمی‌شود. Digest **هویت لازم برای compare** است نه مجوز. برای خدمات بدون POS وابسته هم از known trusted SalesScope با encoding canonical استفاده شود. Ordering برای اضافه‌کردن به Order باید همین binding را در source خودش تطبیق دهد؛ اگر دسترسی به evidence موجود در Ordering ندارد **Flow integration gated** باشد، نه اینکه فقط امضا را کافی بدانیم.

**No invented source read at Resolve.** `r_` در POST با Actor در Surface مقایسه می‌شود و برای بررسی گزینه‌ها از Definition/Provision/Pricing و قوانین واقعی موجود استفاده می‌شود؛ اگر یک تصمیم وابسته به факт ناشناخته است، fail-closed و عدم Snapshot. **A18 guardian eligibility** تا زمانی که trusted Offer/Traveller source بدون پارامتر Caller واقعاً در دسترس نباشد gated است؛ AllowedTravellers را از hash reconstruct نکنید. Full re-shop فقط اگر واقعاً طبق قرارداد existing component ممکن باشد؛ از حدس و dependency تازه منع شده است.

## 2. قالب منطقی r_/d_ — ساده و قابل تست

`r_` = `"r_" + base64url(CanonicalPayloadBytes) + "." + base64url(HMAC-SHA256(secret, UTF8("AERO-ANC-v1|r|") || CanonicalPayloadBytes))`.

`d_` = `"d_" + base64url(CanonicalPayloadBytes) + "." + base64url(HMAC-SHA256(secret, UTF8("AERO-ANC-v1|d|") || CanonicalPayloadBytes))`.

**Payload canonical ثابت در format v1** با قطعات کوتاه (نه JSON اداری بزرگ):

| Key | Type | r_ | d_ | Meaning |
|---|---|---|---|---|
| `v` | int | Y=1 | Y=1 | format version |
| `p` | `r` or `d` | Y | Y | purpose cannot be cross-used |
| `rate` | long | Y | Y | PricingRateId واقعی |
| `sc` | `O|J|P|F` | Y | Y | Order/JourneyTraveller/PortionTraveller/FlightTraveller |
| `tr` | string? | conditional | conditional | canonical TravellerRef; absent Order |
| `cv` | string | Y | Y | `ORDER`/`JOURNEY`/PortionRef/FlightRef canonical |
| `q` | int? | N | Y | selected Quantity; 1 for Direct, chosen count for Quantity |
| `ob` | base64url digest | Y | Y | Offer binding |
| `sb` | base64url digest | Y | Y | trusted sales context binding |
| `exp` | Unix epoch seconds | Y | Y | logical validity boundary |

`p` domain separation حتی اگر Key مشترک باشد؛ signature input **دقیقاً همان رشته bytes نمایش‌داده‌شده در فرمول بالا** است (prefix UTF8 + raw canonical payload bytes)؛ هیچ فرمول جایگزین مجاز نیست. **کنترل دقیق byte-level با golden vectors در Gate1 ضروری است؛ از concatenation نامشخص جلوگیری کنید.** CanonicalEncoding صرفاً همان رشته UTF-8 با field order مندرج در §2.1 است و JSON استفاده نمی‌شود. payload بسیار بزرگ/بیش از محدوده به صورت قطعی رد شود. MAC compare constant-time، integrity covered by full message. طول IDs واقعی به Encoding بستگی دارد؛ **عدد ۲۰ کاراکتر تضمین نکنید**؛ کوتاهی نسبی با کوتاه‌کردن key names است. افزودن state اضافی صرفاً برای کوتاه‌ترشدن d_ ممنوع.

**Key:** Secret از deployment secrets؛ نه hardcoded، نه return به Front، نه Log؛ محیط staging/production separate. Rotation/revocation/`kid` infrastructure در این فاز **اضافه نشود**؛ rotate با deployment هماهنگ و invalidate existing token تا TTL کوتاه. HMAC 256-bit full tag برای سادگی تشخیص، از truncated MAC DIY اجتناب کنید. App config: `AncillarySelectionMaxAge=15 minutes` **پیشنهاد صریح معماری، مقدار کسب‌وکاری قابل تغییر**؛ exp=`min(Shopping.SourceIdentity.ValidUntilUtc, now+AncillarySelectionMaxAge)`؛ اگر trusted ValidUntilUtc در context موجود نیست، ID صادر نشود. TTL عدد مثال نیست؛ Owner می‌تواند پیش از production config را تغییر دهد. هیچ token بدون `exp` پذیرفته نشود.

**Consistency:** برای یک Rate+Unit+Quantity+SourceVersion+SalesContext تا وقتی source/expiry برابر است token deterministic (same bytes). اگر re-shop timestamp جدید باشد exp جدید است و token متفاوت، قابل قبول.

### 2.1 مشخصات Byte-level بدون تصمیم باز برای ایجنت

به‌جای اتکا به ترتیب مبهم JSON، **CanonicalPayloadBytes** رشته UTF-8 زیر است (کاراکتر `|` یک بایت ASCII 0x7C؛ هیچ newline یا space اضافی):

```text
v|p|rate|sc|base64url(UTF8(tr))|base64url(UTF8(cv))|q|ob|sb|exp
```

- رشته‌های `tr` و `cv` بدون تغییر معنایی و با UTF-8 به Base64Url بدون Padding تبدیل می‌شوند تا Separator نتواند مبهم شود؛ **Order بدون Traveller مقدار tr خالی** دارد.
- اعداد `v`, `rate`, `q`, `exp` در فرهنگ invariant، فقط ASCII decimal، بدون `+` و صفرهای اضافی است؛ `v=1`؛ `q` در `r_` خالی و در `d_` عدد مثبت است.
- `p` فقط `r`/`d`، `sc` فقط `O`/`J`/`P`/`F`؛ `cv` به ترتیب `ORDER`/`JOURNEY`/`PortionRef`/`FlightRef` است.
- `ob` و `sb` دقیقاً SHA256/Base64Url بدون Padding (43 کاراکتر) از داده معتمد هستند. برای Bindingها نیز از رشته با قطعات مشخص استفاده می‌کنیم:
  - `ob=base64url(SHA256(UTF8("AERO-OFFER-v1|" + B64(offerId) + "|" + B64(trustedSourceVersion))))`.
  - `sb=base64url(SHA256(UTF8("AERO-SALE-v1|" + ownerAirlineId + "|" + channelNumeric + "|" + pointOfSaleId + "|" + travelAgencyIdOrDash + "|" + airlineOfficeIdOrDash + "|" + customerIdOrDash + "|" + countryIdOrDash)))`.
  - `B64(x)` یعنی Base64Url بدون Padding از UTF-8 x، نه URL encode؛ در `sb` اعداد invariant و null دقیقاً `-` هستند. `channelNumeric` همان مقدار عددی enum موجود `Channel` است، نه رشته Title آن.
  - `pointOfSaleId` شناسه مؤثر **همان Candidate** است، نه تعداد یا فهرست POSهای مجاز.
- ورودی `r_`/`d_` از Client هیچ‌گاه Parse برای **صدور** نمی‌شود؛ Encoder فقط از Rate/Candidate معتمد استفاده می‌کند. Decoder باید `payload` را دوباره Canonicalize کند و Byte-for-byte با اصل برابر بداند؛ UTF-8 نامعتبر، قسمت اضافی/کم، empty mandatory, invalid digest/exp رد می‌شود.
- SHA256 binding محرمانگی واقعی ID را تضمین نمی‌کند؛ `r_`/`d_` نباید PII، شماره پزشکی یا انتخاب‌های تایپی را حمل کند؛ Front همان ID را Opaque می‌بیند.
- Full 32-byte HMAC output + Constant-time compare. Key حداقل 32 بایت تصادفی از Secrets فعلی؛ Key جدید برای هر Environment.
- طول توکن `r_`/`d_` در decode حداکثر **1024 نویسه ASCII** (فقط محدودیت ورودی دفاعی، نه محدودیت فروش محصول)؛ `s_` دقیقاً ۲۴ نویسه. Payloadها با ۹ separator (`|`) دقیقاً ۱۰ قطعه دارند.
- **Ordering handoff:** تطبیق `ob` و `sb` فقط با همان الگوریتم و داده‌های خود Ordering انجام می‌شود. اگر آن منبع هنوز در دسترس نیست، فقط Integration Gate بسته می‌ماند؛ Ancillary Resolver را به Order وابسته نکنید.

### 2.2 بردارهای Golden برای Encoder/Decoder (کلید تست، نه Secret واقعی)

مقدارهای تست: `key = 00 01 02 ... 1f` (۳۲ بایت؛ **فقط تست**)، `offerId="OFFER-TEST-01"`, `trustedSourceVersion="v1"`, `ownerAirlineId=7`, `channelNumeric=2`, `pointOfSaleId=12`, تمام شناسه‌های agency/office/customer/country خالی، `pricingRateId=5001`, `scope=P`, `travellerRef="ADT-1"`, `portionRef="B1"`, `quantity=2`, `exp=1792780800`.

```text
ob = M-MA5VKuL5_rH_1I4c_tsI-qG4hpA7Y-iroa5EL_RPU
sb = 0F6SbltTMkN38Tu15JTdY5sJpY8SqXmBCuaAt7R7iDI
```

`r_` و `d_` دقیقاً با بایت‌های زیر تولید شوند و tests هر کاراکتر را مقایسه کنند:

```text
r_payload = 1|r|5001|P|QURULTE|QjE||M-MA5VKuL5_rH_1I4c_tsI-qG4hpA7Y-iroa5EL_RPU|0F6SbltTMkN38Tu15JTdY5sJpY8SqXmBCuaAt7R7iDI|1792780800
r_token   = r_MXxyfDUwMDF8UHxRVVJVTFRFfFFqRXx8TS1NQTVWS3VMNV9ySF8xSTRjX3RzSS1xRzRocEE3WS1pcm9hNUVMX1JQVXwwRjZTYmx0VE1rTjM4VHUxNUpUZFk1c0pwWThTcVhtQkN1YUF0N1I3aURJfDE3OTI3ODA4MDA.WFG1e1jpYGzHr7n8e9AEYFOjSrEETY71TTO-P7HObRI
d_payload = 1|d|5001|P|QURULTE|QjE|2|M-MA5VKuL5_rH_1I4c_tsI-qG4hpA7Y-iroa5EL_RPU|0F6SbltTMkN38Tu15JTdY5sJpY8SqXmBCuaAt7R7iDI|1792780800
d_token   = d_MXxkfDUwMDF8UHxRVVJVTFRFfFFqRXwyfE0tTUE1Vkt1TDVfckhfMUk0Y190c0ktcUc0aHBBN1ktaXJvYTVFTF9SUFV8MEY2U2JsdFRNa04zOFR1MTVKVGRZNXNKcFk4U3FYbUJDdWFBdDdSN2lESXwxNzkyNzgwODAw.mvVrhkI71kyirgc7YMMaEVWMUacwwalMjQKBuc7XSFU
```

- تغییر حتی یک رقم در Rate/Quantity/Ref/Binding/Expiry ⇒ MAC نامعتبر.
- اگر `exp <= now`، ID منقضی است. `r_` برای Resolve و `d_` برای POST Options پذیرفته نشود.
- این vectors یک Fixture داخلی برای ثبات serialization هستند؛ داده واقعی کاربران/کلید Deployment نیستند.

## 3. s_ و جدول SelectionSnapshot (فقط Typed)

`s_` + Base64URL از 16 byte CSPRNG (128-bit unpredictable, no PII). در DB با Unique PK ذخیره شود. نه قابل decode، نه HMAC ضروری: Possession/lookup و issuerBinding/signed reference verified at POST، Service Auth و Ordering source check الزامی‌اند.

| Column | CLR/SQL | Null | Rule |
|---|---|---|---|
| `Id` | `varchar(24)` primary unique | N | `s_` plus base64url(16 bytes) length 24 |
| `Version` | `smallint` | N | 1 |
| `PricingRateId` | `bigint` | N | actual rate id |
| `CoverageScope` | `smallint` | N | existing enum Sector/Portion/Journey/Order |
| `TravellerRef` | `nvarchar(100)` | Y | absent Order; never full traveler PII |
| `CoverageRef` | `nvarchar(100)` | N | canonical FlightRef/PortionRef/`JOURNEY`/`ORDER` |
| `OfferBinding` | `char(43)` | N | SHA256 digest base64url full-size (43chars) |
| `SalesBinding` | `char(43)` | N | SHA256 digest base64url full-size |
| `Quantity` | `int` | N | actual selected count, >=1 |
| `SelectedJson` | `nvarchar(max)` | N | **typed Variant-specific options only**, allowlisted, no unrelated fields; not entire request/offer |
| `CreatedAtUtc` | `datetimeoffset` | N | server UTC |
| `ExpiresAtUtc` | `datetimeoffset` | N | min(reference expiry, policy TTL), immutable |

Type/Variant need not be replicated as columns: `PricingRateId → Pricing → Provision → Definition` identifies Profile/Variant. Do **not** create a second aggregate or redundant Offer table. Use existing EF conventions + one migration only; no schema changes to SeatMap. `Id` random, duplicate collision retry once with new random bytes. Client cannot choose Id.

**A17 medical:** Approved Owner D2 requires two domain fields (`OxygenFlowRateLitersPerMinute`, `FrequentTravellerMedicalCardNumber`) in this phase, but enabling live storage/purchase is a SEPARATE gate. If the existing platform has NO approved at-rest encryption mechanism, DO NOT store FREMEC/medical values in SelectedJson. `A17` stays gated. Do not implement custom cryptography / log sensitive values; only owner-approved existing KMS/encrypted storage may be reused. A19 Guardian PII likewise gated until representation and handling agreed.

**POST idempotency intentionally minimal:** A network retry may create a different `s_` with identical payload. Both are readable until TTL; no duplicate semantics promised. This avoids an idempotency fingerprint/index/locking system. Front sends the successful ID used for Order; Ordering prevents duplicate commercial service for one Unit/Variant/allowed group. If exact same-ID-on-retry later becomes required, decision must be explicit; don't silently introduce complex dedup. `Resolve` itself is read-only and retry-safe for any given ID.

**TTL deletion:** `now>=ExpiresAtUtc` means 410 now, whether DB row still exists or not. No background cleanup Job in this phase. Purge rows on separate authorized project stage, as requested. `s_` existence doesn't authorize cross-Order use.

## 4. How `POST Options` proves a selection

1. Authenticate Surface Caller using existing Policy; decode and verify `r_`, purpose=`r`, exp>now, version and MAC.
2. Match `salesBinding` to current trusted Caller context. Failure ⇒ 403.
3. Load `PricingRateId`, its parent Pricing, Provision, Definition using existing DB readers. Only Pricing.Status=Active, Provision.Status=Active and Definition.Status=Active; source status/intent must not be guessed. Match Rate/variant/Unit scope to source claims; if PricingRate only usable CHD ensure signed unit and shopping evidence match PTC.
4. Enforce **only type-specific** required/conditional fields and actual allowed values, Qty rules, no Quote/Approval, no incomplete final amount; if source eligibility cannot be verified, DO NOT ISSUE (409).
5. Create one typed snapshot inside DB transaction; return `{selectionId, expiresAtUtc}`. **Never return Echo medical values**.

HMAC key only at trusted Ancillary server. Front never signs request. `POST Options` cannot choose an arbitrary Rate; rate comes only from signed reference. A MAC-protected Reference alone **does not** override per-caller auth or live option constraints.

## 5. How `ResolveGrouped` works

1. Check Service auth and `ids` distinct, count 1..50, nonblank. No other request fields.
2. `d_`: verify MAC/purpose/exp; read PricingRate by Id ONLY through parent Pricing.Status=Active and active Provision/Definition; quantity from payload. `s_`: load snapshot and expiry; quantity and selected from DB; read Rate chain ONLY with Pricing.Status=Active, Provision.Status=Active, Definition.Status=Active.
3. Verify typed ID belongs to known `Variant` and unit scope does not contradict Provision. No re-shop, Offer reader, external inventory, reprice. If parent Pricing has become Retired/Suspended or Provision/Definition inactive reject (not silently use historical price).
4. Output each selected item under correct family; echo `sourceBinding` for Ordering. No write and do not mark consumed.

**Known limitation:** signed `d_` for Portion has PortionRef, not necessarily covered FlightRefs. Do not infer empty legs from a false engine; return `portionRefs:[ref]`,`flightRefs:[]` if unknown. Ordering expands and validates against its own canonical itinerary and Offer binding.

## 6. Misuse cases (negative tests mandatory)

- Forge CHD rate for ADT; forge POS or another airline Rate; forge retired cheap Rate.
- Mutate quantity from 1 to 2 inside direct ID; mutate FlightRef/PortionRef; mutate expiry; signature fails.
- Use r_ in Resolve or d_ in POST Options; bad-purpose rejection.
- Reuse signed r_ in a different authorized tenant/agency; 403.
- Read expired token or expired s_ before physical purge; 410.
- Reuse valid d_/s_ from Order A for Order B; **Ancillary alone cannot know OrderId**; Ordering handoff must catch via offer/sales/unit source-binding check.
- Send A17 FREMEC sensitive data without approved encryption; no table write and gate remains closed.
